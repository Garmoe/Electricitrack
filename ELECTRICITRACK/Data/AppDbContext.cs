using ELECTRICITRACK.Models;
using Microsoft.EntityFrameworkCore;

namespace ELECTRICITRACK.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Appliance> Appliances => Set<Appliance>();
    public DbSet<HouseholdSettings> HouseholdSettings => Set<HouseholdSettings>();
}
