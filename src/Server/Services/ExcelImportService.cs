using ClosedXML.Excel;
using FinancialFreedom.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Services;

public interface IExcelImportService
{
    Task ImportAccountsAsync(Guid householdId, Stream xlsx, CancellationToken cancellationToken = default);
}

/// <summary>
/// Imports account rows from the first worksheet. Expected columns (header row):
/// Account name, Category, Institution, Balance, Tax treatment (optional), Owner prefix (H -/W -/Combined -) in account name.
/// </summary>
public class ExcelImportService(AppDbContext db) : IExcelImportService
{
    public async Task ImportAccountsAsync(Guid householdId, Stream xlsx, CancellationToken cancellationToken = default)
    {
        using var wb = new XLWorkbook(xlsx);
        var ws = wb.Worksheets.First();
        var firstRow = ws.FirstRowUsed();
        var lastRow = ws.LastRowUsed();
        if (firstRow is null || lastRow is null)
            return;

        var headerRow = firstRow.RowNumber();
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in ws.Row(headerRow).CellsUsed())
        {
            var t = cell.GetString().Trim();
            if (!string.IsNullOrEmpty(t))
                map[t] = cell.Address.ColumnNumber;
        }

        static int Col(Dictionary<string, int> m, params string[] names)
        {
            foreach (var n in names)
            {
                if (m.TryGetValue(n, out var c))
                    return c;
            }

            return 0;
        }

        var cName = Col(map, "Account name", "Account", "Name");
        var cCat = Col(map, "Category", "Asset Type");
        var cInst = Col(map, "Institution", "Financial company", "Company");
        var cBal = Col(map, "Balance", "Amount", "Net assets");
        if (cName == 0 || cCat == 0 || cBal == 0)
            throw new InvalidOperationException("Worksheet must include columns for account name, category, and balance.");

        var cInstF = cInst == 0 ? cName : cInst;

        var categories = await db.AssetCategories
            .Where(c => c.HouseholdId == householdId)
            .ToListAsync(cancellationToken);
        var members = await db.FamilyMembers
            .Where(m => m.HouseholdId == householdId)
            .ToListAsync(cancellationToken);
        var adults = members.Where(m => m.Type == FamilyMemberType.Adult).OrderBy(m => m.DisplayOrder).ToList();

        for (var r = headerRow + 1; r <= lastRow.RowNumber(); r++)
        {
            var name = ws.Cell(r, cName).GetString().Trim();
            if (string.IsNullOrEmpty(name))
                continue;

            var catName = ws.Cell(r, cCat).GetString().Trim();
            var inst = ws.Cell(r, cInstF).GetString().Trim();
            if (!decimal.TryParse(ws.Cell(r, cBal).GetString(), out var bal))
                bal = (decimal)ws.Cell(r, cBal).GetDouble();

            var cat = categories.FirstOrDefault(c => c.Name.Equals(catName, StringComparison.OrdinalIgnoreCase));
            if (cat is null)
            {
                cat = new AssetCategory
                {
                    Id = Guid.NewGuid(),
                    HouseholdId = householdId,
                    Name = catName,
                    DisplayOrder = categories.Count,
                };
                db.AssetCategories.Add(cat);
                categories.Add(cat);
            }

            var (cleanName, ownerSpec) = ParseOwnerPrefix(name, members);
            var tax = InferTax(cleanName, cat);

            var account = new Account
            {
                Id = Guid.NewGuid(),
                HouseholdId = householdId,
                AssetCategoryId = cat.Id,
                AccountName = cleanName,
                FinancialCompany = inst,
                CurrentBalance = bal,
                AssetClass = AssetClass.Liquid,
                TaxTreatment = tax,
                IncludeInRetirementProjections = cat.IsRetirement,
            };
            db.Accounts.Add(account);

            var owners = ownerSpec;
            if (owners.Count == 0 && adults.Count >= 2)
            {
                owners =
                [
                    (adults[0].Id, 50m),
                    (adults[1].Id, 50m),
                ];
            }
            else if (owners.Count == 0 && adults.Count == 1)
            {
                owners = [(adults[0].Id, 100m)];
            }

            foreach (var o in owners)
            {
                db.AccountOwners.Add(new AccountOwner
                {
                    AccountId = account.Id,
                    FamilyMemberId = o.MemberId,
                    OwnershipPercent = o.Pct,
                });
            }

            db.AccountBalanceSnapshots.Add(new AccountBalanceSnapshot
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                AsOfDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Balance = bal,
                Source = SnapshotSource.Imported,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static TaxTreatment InferTax(string accountName, AssetCategory cat)
    {
        var n = accountName.ToUpperInvariant();
        if (cat.IsHsa)
            return TaxTreatment.Hsa;
        if (cat.IsEducation || n.Contains("529"))
            return TaxTreatment.Education;
        if (n.Contains("ROTH"))
            return TaxTreatment.Roth;
        if (n.Contains("401") || n.Contains("IRA") || n.Contains("403"))
            return TaxTreatment.PreTax;
        return TaxTreatment.Taxable;
    }

    private static (string Name, List<(Guid MemberId, decimal Pct)> Owners) ParseOwnerPrefix(
        string raw,
        List<FamilyMember> members)
    {
        var adults = members.Where(m => m.Type == FamilyMemberType.Adult).OrderBy(m => m.DisplayOrder).ToList();
        var prefix = raw.TrimStart();
        if (prefix.StartsWith("H -", StringComparison.OrdinalIgnoreCase))
        {
            var name = raw.Substring(Math.Min(3, raw.Length)).TrimStart('-', ' ');
            var dad = adults.FirstOrDefault(m => m.Name.Equals("Dad", StringComparison.OrdinalIgnoreCase)) ?? adults.FirstOrDefault();
            return (name, dad is null ? new List<(Guid, decimal)>() : new List<(Guid, decimal)> { (dad.Id, 100m) });
        }

        if (prefix.StartsWith("W -", StringComparison.OrdinalIgnoreCase))
        {
            var name = raw.Substring(Math.Min(3, raw.Length)).TrimStart('-', ' ');
            var mom = adults.FirstOrDefault(m => m.Name.Equals("Mom", StringComparison.OrdinalIgnoreCase)) ?? adults.Skip(1).FirstOrDefault();
            return (name, mom is null ? new List<(Guid, decimal)>() : new List<(Guid, decimal)> { (mom.Id, 100m) });
        }

        if (prefix.StartsWith("Combined -", StringComparison.OrdinalIgnoreCase))
        {
            var name = raw.Substring("Combined -".Length).Trim();
            var list = new List<(Guid, decimal)>();
            if (adults.Count >= 2)
            {
                list.Add((adults[0].Id, 50m));
                list.Add((adults[1].Id, 50m));
            }
            else if (adults.Count == 1)
                list.Add((adults[0].Id, 100m));

            return (name, list);
        }

        if (adults.Count == 1)
            return (raw, new List<(Guid, decimal)> { (adults[0].Id, 100m) });

        return (raw, new List<(Guid, decimal)>());
    }
}
