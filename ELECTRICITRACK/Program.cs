using ELECTRICITRACK.Data;
using ELECTRICITRACK.Models;
using ELECTRICITRACK.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ConsumptionAnalysisService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

await SeedDatabaseAsync(app);
app.Run();

static async Task SeedDatabaseAsync(WebApplication app)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (!await db.HouseholdSettings.AnyAsync())
    {
        db.HouseholdSettings.Add(new HouseholdSettings
        {
            ElectricityRatePerKwh = 12m,
            MonthlyBudget = 5000m
        });
    }

    if (!await db.Appliances.AnyAsync())
    {
        db.Appliances.AddRange(
            new Appliance { Name = "Air conditioner", PowerWatts = 1000m, HoursPerDay = 8m, DaysPerMonth = 30, IsReference = true },
            new Appliance { Name = "Refrigerator", PowerWatts = 150m, HoursPerDay = 8m, DaysPerMonth = 30, IsReference = true },
            new Appliance { Name = "Rice cooker", PowerWatts = 700m, HoursPerDay = 1m, DaysPerMonth = 30, IsReference = true },
            new Appliance { Name = "Electric fan", PowerWatts = 60m, HoursPerDay = 10m, DaysPerMonth = 30, IsReference = true },
            new Appliance { Name = "Television", PowerWatts = 80m, HoursPerDay = 4m, DaysPerMonth = 30, IsReference = true },
            new Appliance { Name = "Washing machine", PowerWatts = 500m, HoursPerDay = 1m, DaysPerMonth = 12, IsReference = true });
    }

    await db.SaveChangesAsync();
}
