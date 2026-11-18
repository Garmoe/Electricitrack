using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELECTRICITRACK.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Appliances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    PowerWatts = table.Column<decimal>(type: "TEXT", nullable: false),
                    HoursPerDay = table.Column<decimal>(type: "TEXT", nullable: false),
                    DaysPerMonth = table.Column<int>(type: "INTEGER", nullable: false),
                    IsReference = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appliances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HouseholdSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ElectricityRatePerKwh = table.Column<decimal>(type: "TEXT", nullable: false),
                    MonthlyBudget = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseholdSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appliances");

            migrationBuilder.DropTable(
                name: "HouseholdSettings");
        }
    }
}
