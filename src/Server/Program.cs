using System.Security.Claims;
using FinancialFreedom.Server.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies(o =>
    {
        o.ApplicationCookie!.Configure(c =>
        {
            c.Events ??= new CookieAuthenticationEvents();
            c.Events.OnRedirectToLogin = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }

                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            };
            c.Events.OnRedirectToAccessDenied = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }

                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            };
        });
    });

builder.Services.AddAuthorizationBuilder();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var useInMemory = builder.Configuration.GetValue("Database:UseInMemory", false);
var inMemoryName = builder.Configuration["Database:InMemoryDatabaseName"] ?? "FinancialFreedom";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (useInMemory)
        options.UseInMemoryDatabase(inMemoryName);
    else if (!string.IsNullOrWhiteSpace(connectionString))
        options.UseSqlServer(connectionString);
    else
        throw new InvalidOperationException(
            "Configure Database:UseInMemory=true for local work without SQL Server, or set ConnectionStrings:DefaultConnection.");
});

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.Stores.MaxLengthForKeys = 128;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders()
    .AddApiEndpoints();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (useInMemory)
        await db.Database.EnsureCreatedAsync();
    else
        await db.Database.MigrateAsync();

    if (app.Environment.IsDevelopment())
        await IdentityDataSeeder.SeedAsync(scope.ServiceProvider);
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<ApplicationUser>();

app.MapPost(
        "/logout",
        async (SignInManager<ApplicationUser> signInManager, [FromBody] object? empty) =>
        {
            if (empty is null)
                return Results.Unauthorized();

            await signInManager.SignOutAsync();
            return Results.Ok();
        })
    .RequireAuthorization();

app.MapGet(
        "/roles",
        (ClaimsPrincipal user) =>
        {
            if (user.Identity is not { IsAuthenticated: true })
                return Results.Unauthorized();

            if (user.Identity is not ClaimsIdentity identity)
                return Results.Unauthorized();

            var roles = identity.FindAll(identity.RoleClaimType)
                .Select(c => new { c.Issuer, c.OriginalIssuer, c.Type, c.Value, c.ValueType });
            return TypedResults.Json(roles);
        })
    .RequireAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }
