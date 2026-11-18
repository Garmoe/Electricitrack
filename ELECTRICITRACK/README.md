# ELECTRICITRACK

An ASP.NET Core MVC appliance power-consumption tracker built with Razor Views,
Entity Framework Core, and SQLite.

## Run the application

Open a terminal in this folder and run:

```powershell
dotnet run
```

Open the HTTPS address printed by ASP.NET Core. The first run applies the EF Core
migrations, creates `electricitrack.db`, and adds illustrative sample appliances
and default household settings.

## Main features

- Household overview with monthly kWh and cost estimates, a consumption breakdown,
  and appliances ranked by monthly use.
- Appliance add, edit, and delete screens with input validation.
- Usage simulation that previews hour changes, estimates savings, checks a budget,
  and calculates a budget-based daily-hours target.
- Household settings for the electricity rate and monthly budget.
- Prefix search uses binary search; appliance ranking uses heap sort.
- Sample appliance values are illustrative references. Replace them with values
  from appliance labels or manuals.

## Database

SQLite is stored locally in `electricitrack.db`. To update the database after
changing the EF Core model, create a migration and apply it:

```powershell
dotnet ef migrations add DescribeYourChange
dotnet ef database update
```

The app applies existing migrations automatically at startup.
