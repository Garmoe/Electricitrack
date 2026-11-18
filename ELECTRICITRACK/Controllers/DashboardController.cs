using ELECTRICITRACK.Data;
using ELECTRICITRACK.Services;
using ELECTRICITRACK.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELECTRICITRACK.Controllers;

public class DashboardController(
    AppDbContext db,
    ConsumptionAnalysisService analysis) : Controller
{
    /// <summary>
    /// Generates a dynamic thermal / intensity consumption gradient based on appliance rank and total count.
    /// Higher power consumers are styled with hot warning tones (Warm Coral / Crimson), transitioning
    /// smoothly through Amber and Indigo down to a calm Electric Blue for low-consumption appliances.
    /// </summary>
    private static string GetThermalIntensityGradient(int index, int totalCount)
    {
        if (totalCount <= 1)
        {
            return "#f43f5e"; // Warm Coral Red
        }

        // Normalized position from 0.0 (top consumer) to 1.0 (lowest consumer)
        var t = Math.Clamp((double)index / (totalCount - 1), 0.0, 1.0);

        // 3-stage thermal ramp:
        // Stage 1 (0.00 - 0.35): Coral Red (#F43F5E) -> Warm Amber (#F59E0B)
        // Stage 2 (0.35 - 0.70): Warm Amber (#F59E0B) -> Royal Purple (#8B5CF6)
        // Stage 3 (0.70 - 1.00): Royal Purple (#8B5CF6) -> Electric Blue (#2563EB)
        int r, g, b;
        if (t <= 0.35)
        {
            var factor = t / 0.35;
            r = (int)Math.Round(244 + factor * (245 - 244));
            g = (int)Math.Round(63 + factor * (158 - 63));
            b = (int)Math.Round(94 + factor * (11 - 94));
        }
        else if (t <= 0.70)
        {
            var factor = (t - 0.35) / 0.35;
            r = (int)Math.Round(245 + factor * (139 - 245));
            g = (int)Math.Round(158 + factor * (92 - 158));
            b = (int)Math.Round(11 + factor * (246 - 11));
        }
        else
        {
            var factor = (t - 0.70) / 0.30;
            r = (int)Math.Round(139 + factor * (37 - 139));
            g = (int)Math.Round(92 + factor * (99 - 92));
            b = (int)Math.Round(246 + factor * (235 - 246));
        }

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    public async Task<IActionResult> Index(string? search, string? sort = "high-to-low")
    {
        var appliances = await db.Appliances.AsNoTracking().ToListAsync();
        var settings = await db.HouseholdSettings.AsNoTracking().SingleAsync();
        ViewData["ElectricityRate"] = settings.ElectricityRatePerKwh.ToString("N2");
        ViewData["MonthlyBudget"] = settings.MonthlyBudget.ToString("N2");
        var ranked = analysis.SortByMonthlyConsumption(appliances);
        var totalKwh = appliances.Sum(ConsumptionCalculator.MonthlyKwh);
        var shares = ranked.Select((item, index) =>
        {
            var kwh = ConsumptionCalculator.MonthlyKwh(item);
            return new ConsumptionShareViewModel
            {
                Name = item.Name,
                Kwh = kwh,
                Percentage = totalKwh == 0 ? 0 : kwh / totalKwh * 100m,
                Color = GetThermalIntensityGradient(index, ranked.Count)
            };
        }).ToList();

        var searched = analysis.SearchByNamePrefix(appliances, search);
        var sortedAppliances = (sort?.ToLowerInvariant()) switch
        {
            "low-to-high" => analysis.SortByMonthlyConsumption(searched).AsEnumerable().Reverse().ToList(),
            "name-asc" => searched.OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase).ToList(),
            "name-desc" => searched.OrderByDescending(a => a.Name, StringComparer.OrdinalIgnoreCase).ToList(),
            "daily-hours" => searched.OrderByDescending(a => a.HoursPerDay).ToList(),
            _ => analysis.SortByMonthlyConsumption(searched) // default: high-to-low via Heap Sort
        };

        var viewModel = new DashboardViewModel
        {
            Appliances = sortedAppliances,
            Shares = shares,
            TotalKwh = totalKwh,
            TotalCost = totalKwh * settings.ElectricityRatePerKwh,
            RatePerKwh = settings.ElectricityRatePerKwh,
            MonthlyBudget = settings.MonthlyBudget,
            Search = search,
            Sort = sort ?? "high-to-low",
            LargestContributor = ranked.FirstOrDefault()
        };

        return View(viewModel);
    }
}
