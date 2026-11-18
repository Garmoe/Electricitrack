using ELECTRICITRACK.Models;

namespace ELECTRICITRACK.ViewModels;

public class ConsumptionShareViewModel
{
    public string Name { get; set; } = string.Empty;
    public decimal Kwh { get; set; }
    public decimal Percentage { get; set; }
    public string Color { get; set; } = "#2463eb";
}

public class DashboardViewModel
{
    public IReadOnlyList<Appliance> Appliances { get; set; } = [];
    public IReadOnlyList<ConsumptionShareViewModel> Shares { get; set; } = [];
    public decimal TotalKwh { get; set; }
    public decimal TotalCost { get; set; }
    public decimal RatePerKwh { get; set; }
    public decimal MonthlyBudget { get; set; }
    public string? Search { get; set; }
    public string Sort { get; set; } = "high-to-low";
    public Appliance? LargestContributor { get; set; }
}
