using ELECTRICITRACK.Data;
using ELECTRICITRACK.Models;
using ELECTRICITRACK.Services;
using ELECTRICITRACK.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELECTRICITRACK.Controllers;

public class AppliancesController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index(int? id, bool create = false)
    {
        if (id.HasValue && !await db.Appliances.AnyAsync(item => item.Id == id.Value))
        {
            return NotFound();
        }

        var appliances = await db.Appliances.AsNoTracking()
            .OrderBy(item => item.Name)
            .ToListAsync();
        var isCreating = create || appliances.Count == 0;
        var appliance = isCreating
            ? new Appliance { DaysPerMonth = 30 }
            : appliances.First(item => item.Id == (id ?? appliances[0].Id));
        var settings = await db.HouseholdSettings.AsNoTracking().SingleAsync();
        ViewData["ElectricityRate"] = settings.ElectricityRatePerKwh.ToString("N2");
        ViewData["MonthlyBudget"] = settings.MonthlyBudget.ToString("N2");

        return View(new AppliancePageViewModel
        {
            Appliances = appliances,
            Appliance = appliance,
            RatePerKwh = settings.ElectricityRatePerKwh,
            IsCreating = isCreating,
            MonthlyKwh = ConsumptionCalculator.MonthlyKwh(appliance),
            MonthlyCost = ConsumptionCalculator.MonthlyCost(appliance, settings.ElectricityRatePerKwh)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Appliance appliance)
    {
        if (!ModelState.IsValid)
        {
            return await ReturnEditorAsync(appliance);
        }

        if (appliance.Id == 0)
        {
            appliance.IsReference = false;
            db.Appliances.Add(appliance);
        }
        else
        {
            var existing = await db.Appliances.FindAsync(appliance.Id);
            if (existing is null)
            {
                return NotFound();
            }

            existing.Name = appliance.Name;
            existing.PowerWatts = appliance.PowerWatts;
            existing.HoursPerDay = appliance.HoursPerDay;
            existing.DaysPerMonth = appliance.DaysPerMonth;
            existing.IsReference = false;
        }

        await db.SaveChangesAsync();
        TempData["Notice"] = "Appliance saved.";
        return RedirectToAction(nameof(Index), new { id = appliance.Id == 0 ? (int?)null : appliance.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var appliance = await db.Appliances.FindAsync(id);
        if (appliance is null)
        {
            return NotFound();
        }

        db.Appliances.Remove(appliance);
        await db.SaveChangesAsync();
        TempData["Notice"] = "Appliance deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> ReturnEditorAsync(Appliance appliance)
    {
        var appliances = await db.Appliances.AsNoTracking()
            .OrderBy(item => item.Name)
            .ToListAsync();
        var settings = await db.HouseholdSettings.AsNoTracking().SingleAsync();
        ViewData["ElectricityRate"] = settings.ElectricityRatePerKwh.ToString("N2");
        ViewData["MonthlyBudget"] = settings.MonthlyBudget.ToString("N2");
        return View("Index", new AppliancePageViewModel
        {
            Appliances = appliances,
            Appliance = appliance,
            RatePerKwh = settings.ElectricityRatePerKwh,
            IsCreating = appliance.Id == 0,
            MonthlyKwh = ConsumptionCalculator.MonthlyKwh(appliance),
            MonthlyCost = ConsumptionCalculator.MonthlyCost(appliance, settings.ElectricityRatePerKwh)
        });
    }
}
