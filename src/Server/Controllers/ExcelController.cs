using ClosedXML.Excel;
using FinancialFreedom.Server.Data;
using FinancialFreedom.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialFreedom.Server.Controllers;

[Route("api/excel")]
public class ExcelController(AppDbContext db, IExcelImportService excelImport) : HouseholdControllerBase(db)
{
    [HttpGet("export")]
    public async Task<IActionResult> Export(CancellationToken cancellationToken)
    {
        var h = await GetHouseholdAsync(cancellationToken);
        if (h is null)
            return NotFound();

        var accounts = await db.Accounts.AsNoTracking()
            .Include(a => a.AssetCategory)
            .Include(a => a.Owners)
            .Where(a => a.HouseholdId == h.Id)
            .OrderBy(a => a.AccountName)
            .ToListAsync(cancellationToken);

        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Accounts");
        ws.Cell(1, 1).Value = "Account name";
        ws.Cell(1, 2).Value = "Category";
        ws.Cell(1, 3).Value = "Institution";
        ws.Cell(1, 4).Value = "Balance";
        ws.Cell(1, 5).Value = "Tax treatment";
        ws.Cell(1, 6).Value = "Owners";

        var row = 2;
        foreach (var a in accounts)
        {
            ws.Cell(row, 1).Value = a.AccountName;
            ws.Cell(row, 2).Value = a.AssetCategory.Name;
            ws.Cell(row, 3).Value = a.FinancialCompany;
            ws.Cell(row, 4).Value = a.CurrentBalance;
            ws.Cell(row, 5).Value = a.TaxTreatment.ToString();
            ws.Cell(row, 6).Value = string.Join(", ", a.Owners.Select(o => $"{o.FamilyMemberId}:{o.OwnershipPercent}%"));
            row++;
        }

        await using var stream = new MemoryStream();
        wb.SaveAs(stream);
        var bytes = stream.ToArray();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "financial-freedom-accounts.xlsx");
    }

    [HttpPost("import")]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult> Import(IFormFile file, CancellationToken cancellationToken)
    {
        var h = await GetHouseholdTrackedAsync(cancellationToken);
        if (h is null)
            return NotFound();

        if (file.Length == 0)
            return BadRequest("Empty file.");

        await using var stream = file.OpenReadStream();
        await excelImport.ImportAccountsAsync(h.Id, stream, cancellationToken);
        return NoContent();
    }
}
