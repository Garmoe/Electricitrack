using System.ComponentModel.DataAnnotations;

namespace ELECTRICITRACK.Models;

public class Appliance
{
    public int Id { get; set; }

    [Required]
    [StringLength(80)]
    [Display(Name = "Appliance name")]
    public string Name { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.1", "100000")]
    [Display(Name = "Power rating (watts)")]
    public decimal PowerWatts { get; set; }

    [Range(typeof(decimal), "0", "24")]
    [Display(Name = "Hours used per day")]
    public decimal HoursPerDay { get; set; }

    [Range(1, 31)]
    [Display(Name = "Days used per month")]
    public int DaysPerMonth { get; set; } = 30;

    public bool IsReference { get; set; }
}
