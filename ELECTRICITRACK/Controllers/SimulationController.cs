using ELECTRICITRACK.Data;
using ELECTRICITRACK.Models;
using ELECTRICITRACK.Services;
using ELECTRICITRACK.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELECTRICITRACK.Controllers;

public class SimulationController(
    AppDbContext db,
    ConsumptionAnalysisService analysis) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var appliances = analysis.SortByMonthlyConsumption(
            await db.Appliances.AsNoTracking().ToListAsync());
        var settings = await db.HouseholdSettings.AsNoTracking().SingleAsync();
        ViewData["ElectricityRate"] = settings.ElectricityRatePerKwh.ToString("N2");
        ViewData["MonthlyBudget"] = settings.MonthlyBudget.ToString("N2");
        return View(BuildViewModel(appliances, settings, null));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(SimulationApplyInput input)
    {
        if (input.Items.Select(item => item.Id).Distinct().Count() != input.Items.Count)
        {
            ModelState.AddModelError(string.Empty, "Each appliance can only appear once in the scenario.");
        }

        var appliances = await db.Appliances.ToListAsync();
        var validIds = appliances.Select(item => item.Id).ToHashSet();
        if (input.Items.Any(item => !validIds.Contains(item.Id)) ||
            input.Items.Count != appliances.Count)
        {
            ModelState.AddModelError(string.Empty, "The appliance list changed. Reload the simulation and try again.");
        }

        if (!ModelState.IsValid)
        {
            var settingsForView = await db.HouseholdSettings.AsNoTracking().SingleAsync();
            ViewData["ElectricityRate"] = settingsForView.ElectricityRatePerKwh.ToString("N2");
            ViewData["MonthlyBudget"] = settingsForView.MonthlyBudget.ToString("N2");
            return View("Index", BuildViewModel(
                appliances,
                settingsForView,
                input.Items.ToDictionary(item => item.Id, item => item.Hours)));
        }

        var proposedHours = input.Items.ToDictionary(item => item.Id, item => item.Hours);
        foreach (var appliance in appliances)
        {
            appliance.HoursPerDay = proposedHours[appliance.Id];
        }

        await db.SaveChangesAsync();
        TempData["Notice"] = "The scenario was applied to your saved appliance hours.";
        return RedirectToAction(nameof(Index));
    }

    private static SimulationViewModel BuildViewModel(
        IReadOnlyList<Appliance> appliances,
        HouseholdSettings settings,
        IReadOnlyDictionary<int, decimal>? adjustedHours)
    {
        var rows = appliances.Select(appliance =>
        {
            var hours = adjustedHours is not null &&
                        adjustedHours.TryGetValue(appliance.Id, out var proposed)
                ? proposed
                : appliance.HoursPerDay;
            var adjustedAppliance = new Appliance
            {
                Name = appliance.Name,
                PowerWatts = appliance.PowerWatts,
                HoursPerDay = hours,
                DaysPerMonth = appliance.DaysPerMonth
            };
            var currentCost = ConsumptionCalculator.MonthlyCost(appliance, settings.ElectricityRatePerKwh);
            var adjustedCost = ConsumptionCalculator.MonthlyCost(adjustedAppliance, settings.ElectricityRatePerKwh);
            return new SimulationRowViewModel
            {
                Appliance = appliance,
                AdjustedHours = hours,
                AdjustedKwh = ConsumptionCalculator.MonthlyKwh(adjustedAppliance),
                SavedCost = currentCost - adjustedCost
            };
        }).ToList();

        var currentKwh = appliances.Sum(ConsumptionCalculator.MonthlyKwh);
        var adjustedKwh = rows.Sum(row => row.AdjustedKwh);
        var guidanceRow = rows.FirstOrDefault();
        var otherCost = guidanceRow is null
            ? 0m
            : rows.Where(row => row.Appliance.Id != guidanceRow.Appliance.Id)
                .Sum(row => row.AdjustedKwh * settings.ElectricityRatePerKwh);
        var remainingBudget = Math.Max(0, settings.MonthlyBudget - otherCost);
        var costPerDailyHour = guidanceRow is null
            ? 0m
            : guidanceRow.Appliance.PowerWatts / 1000m *
              guidanceRow.Appliance.DaysPerMonth * settings.ElectricityRatePerKwh;
        var recommendedHours = costPerDailyHour <= 0
            ? 0m
            : Math.Clamp(remainingBudget / costPerDailyHour, 0m, 24m);

        return new SimulationViewModel
        {
            Rows = rows,
            RatePerKwh = settings.ElectricityRatePerKwh,
            MonthlyBudget = settings.MonthlyBudget,
            CurrentKwh = currentKwh,
            CurrentCost = currentKwh * settings.ElectricityRatePerKwh,
            AdjustedKwh = adjustedKwh,
            AdjustedCost = adjustedKwh * settings.ElectricityRatePerKwh,
            GuidanceAppliance = guidanceRow?.Appliance,
            RecommendedHours = Math.Round(recommendedHours, 1),
            BudgetRemainingForGuidance = remainingBudget,
            IsApplied = false
        };
    }
}
