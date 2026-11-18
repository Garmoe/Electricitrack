using System.ComponentModel.DataAnnotations;

namespace ELECTRICITRACK.Models;

public class HouseholdSettings
{
    public int Id { get; set; }

    [Range(typeof(decimal), "0.01", "100000")]
    [Display(Name = "Electricity rate")]
    public decimal ElectricityRatePerKwh { get; set; } = 12m;

    [Range(typeof(decimal), "0.01", "100000000")]
    [Display(Name = "Monthly electricity budget")]
    public decimal MonthlyBudget { get; set; } = 5000m;
}
