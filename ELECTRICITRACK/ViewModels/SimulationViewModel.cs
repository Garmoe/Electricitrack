using System.ComponentModel.DataAnnotations;
using ELECTRICITRACK.Models;

namespace ELECTRICITRACK.ViewModels;

public class SimulationRowViewModel
{
    public Appliance Appliance { get; set; } = new();
    public decimal AdjustedHours { get; set; }
    public decimal AdjustedKwh { get; set; }
    public decimal SavedCost { get; set; }
}

public class SimulationViewModel
{
    public IReadOnlyList<SimulationRowViewModel> Rows { get; set; } = [];
    public decimal RatePerKwh { get; set; }
    public decimal MonthlyBudget { get; set; }
    public decimal CurrentKwh { get; set; }
    public decimal CurrentCost { get; set; }
    public decimal AdjustedKwh { get; set; }
    public decimal AdjustedCost { get; set; }
    public Appliance? GuidanceAppliance { get; set; }
    public decimal RecommendedHours { get; set; }
    public decimal BudgetRemainingForGuidance { get; set; }
    public bool IsApplied { get; set; }
}

public class SimulationApplyInput
{
    public List<SimulationApplyItem> Items { get; set; } = [];
}

public class SimulationApplyItem
{
    public int Id { get; set; }

    [Range(typeof(decimal), "0", "24")]
    public decimal Hours { get; set; }
}
