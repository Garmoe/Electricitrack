using ELECTRICITRACK.Models;

namespace ELECTRICITRACK.ViewModels;

public class AppliancePageViewModel
{
    public IReadOnlyList<Appliance> Appliances { get; set; } = [];
    public Appliance Appliance { get; set; } = new();
    public decimal RatePerKwh { get; set; }
    public bool IsCreating { get; set; }
    public decimal MonthlyKwh { get; set; }
    public decimal MonthlyCost { get; set; }
}
