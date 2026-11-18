using ELECTRICITRACK.Data;
using ELECTRICITRACK.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELECTRICITRACK.Controllers;

public class SettingsController(AppDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var settings = await db.HouseholdSettings.AsNoTracking().SingleAsync();
        SetSidebarValues(settings);
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(HouseholdSettings input)
    {
        if (!ModelState.IsValid)
        {
            SetSidebarValues(input);
            return View("Index", input);
        }

        var settings = await db.HouseholdSettings.SingleAsync();
        settings.ElectricityRatePerKwh = input.ElectricityRatePerKwh;
        settings.MonthlyBudget = input.MonthlyBudget;
        await db.SaveChangesAsync();
        TempData["Notice"] = "Household settings saved.";
        return RedirectToAction(nameof(Index));
    }

    private void SetSidebarValues(HouseholdSettings settings)
    {
        ViewData["ElectricityRate"] = settings.ElectricityRatePerKwh.ToString("N2");
        ViewData["MonthlyBudget"] = settings.MonthlyBudget.ToString("N2");
    }
}
