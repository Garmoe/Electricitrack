# ELECTRICITRACK: System Architecture & Technical Documentation
**An Appliance Power Consumption Tracker for Filipino Households**

---

## 1. Executive Summary & Purpose

**ELECTRICITRACK** is an appliance-level electrical energy tracking and simulation web application tailored specifically to the economic realities of Filipino households. While electric distribution utilities (such as Meralco or regional electric cooperatives) provide end-of-month aggregate billing statements showing total kilowatt-hours ($\text{kWh}$) consumed and total amount due in Philippine Pesos ($\text{₱}$), they do not break down which specific appliances contribute most to the bill, nor do they offer tools for scenario modeling.

ELECTRICITRACK resolves this by:
1. **Centralizing appliance-level energy tracking** based on wattage ($\text{W}$), daily operating hours ($\text{h/day}$), and monthly operating days ($\text{days/month}$).
2. **Prioritizing power-hungry appliances** at the top of the hierarchy using an optimized **Heap Sort** algorithm ($O(N \log N)$).
3. **Enabling rapid prefix searching** for appliances using a custom **Binary Search** algorithm ($O(\log N)$).
4. **Simulating what-if scenarios** (e.g., reducing air conditioner hours from 8 to 5 hours) and computing exact monthly monetary savings and budget feasibility in real time.
5. **Providing verified reference baseline presets** for standard Filipino appliances (e.g., refrigerator, rice cooker, electric stand fan, air conditioner, TV, washing machine).

---

## 2. Overall Web Application Architecture

ELECTRICITRACK is built upon a **Three-Tier, Layered Model-View-Controller (MVC)** architectural pattern using **ASP.NET Core (.NET 10.0)** and **Entity Framework Core 10** with an embedded **SQLite** relational database engine.

```
+-----------------------------------------------------------------------------------+
|                            1. CLIENT / PRESENTATION TIER                          |
|  - Web Browser (Desktop / Tablet / Mobile)                                        |
|  - Semantic HTML5, Custom CSS3 Design System (Responsive Flexbox & Grid)         |
|  - Vanilla JavaScript (ES6+ Reactive Calculators, Steppers & Dynamic DOM Updates) |
+-----------------------------------------------------------------------------------+
                                         |
                                   HTTP Requests
                            (GET / POST + AntiForgery)
                                         v
+-----------------------------------------------------------------------------------+
|                        2. APPLICATION & CONTROLLER TIER                           |
|  - ASP.NET Core Routing Engine (Endpoint Routing: {controller}/{action}/{id?})    |
|  - Controllers:                                                                   |
|      * DashboardController      * AppliancesController                            |
|      * SimulationController     * SettingsController                              |
|  - ViewModels (Strongly-typed DTOs transferring structured data to Razor Views)  |
|  - Razor View Engine (Compiles .cshtml into dynamic server-rendered HTML)         |
+-----------------------------------------------------------------------------------+
                                         |
                            Dependency Injection (Scoped)
                                         v
+-----------------------------------------------------------------------------------+
|                      3. DOMAIN LOGIC & ALGORITHM SERVICES                         |
|  - ConsumptionAnalysisService:                                                    |
|      * Heap Sort (Descending order by monthly kilowatt-hour consumption)          |
|      * Binary Search (Case-insensitive appliance prefix lookup)                   |
|  - ConsumptionCalculator:                                                         |
|      * Monthly kWh Math, Monthly Cost Math, Currency Formatting (₱)              |
+-----------------------------------------------------------------------------------+
                                         |
                             Entity Framework Core 10
                                         v
+-----------------------------------------------------------------------------------+
|                        4. DATA ACCESS & PERSISTENCE TIER                          |
|  - AppDbContext (Unit of Work & Repository abstraction over SQLite)               |
|  - Database Migrations & Auto-Seeding (SeedDatabaseAsync in Program.cs)           |
|  - SQLite Database File (electricitrack.db)                                       |
+-----------------------------------------------------------------------------------+
```

### End-to-End Request Lifecycle
1. **User Request:** The browser sends an HTTP request (e.g., `GET /Dashboard` or `POST /Appliances/Save`).
2. **Routing & Dispatch:** ASP.NET Core matches the route and activates the corresponding Controller.
3. **Dependency Injection:** The runtime injects `AppDbContext` and `ConsumptionAnalysisService` into the controller primary constructor.
4. **Data Retrieval & Business Logic:** The controller fetches database records, applies Heap Sort / Binary Search, and evaluates energy equations.
5. **ViewModel Construction:** The controller prepares a tailored ViewModel containing only the required view properties.
6. **Razor Compilation:** Razor renders the `.cshtml` view with embedded C# expressions into clean HTML.
7. **Client Delivery & Execution:** The HTML, CSS (`site.css`), and JavaScript (`site.js`) load in the client browser, running interactive calculators locally with zero network delay.

---

## 3. Project Directory Layout & File Architecture

ELECTRICITRACK is organized into clear architectural layers: **Controllers** for HTTP handling, **Services** for algorithmic computation (Heap Sort & Binary Search), **Models & Data** for persistence, and **Views & ViewModels** for presentation.

### 3.1 Complete Project Directory Tree

```
ELECTRICITRACK/
├── Controllers/
│   ├── AppliancesController.cs           # Appliance CRUD & validation handling
│   ├── DashboardController.cs            # Overview, ranking, & thermal gradient
│   ├── HomeController.cs                 # Default routing & error handling
│   ├── SettingsController.cs             # Utility rate & monthly budget updates
│   └── SimulationController.cs           # Scenario modeling & budget guidance
├── Data/
│   └── AppDbContext.cs                   # EF Core SQLite database context
├── Models/
│   ├── Appliance.cs                      # Domain model with [Range] validation
│   ├── ErrorViewModel.cs                 # Application error tracking
│   └── HouseholdSettings.cs              # Rate (₱/kWh) & budget ceiling entity
├── Services/
│   └── ConsumptionAnalysisService.cs     # Heap Sort, Binary Search, & Math Engine
├── ViewModels/
│   ├── AppliancePageViewModel.cs         # Appliance manager UI data contract
│   ├── DashboardViewModel.cs             # Overview UI contract & category shares
│   └── SimulationViewModel.cs            # Scenario simulator UI & guidance contract
├── Views/
│   ├── _ViewImports.cshtml               # Global Razor directives & tag helpers
│   ├── _ViewStart.cshtml                 # Master layout configuration
│   ├── Appliances/
│   │   └── Index.cshtml                  # Master-detail list & reactive editor
│   ├── Dashboard/
│   │   └── Index.cshtml                  # Metric cards, thermal bar, & sorted table
│   ├── Home/
│   │   └── Privacy.cshtml                # Privacy disclosure view
│   ├── Settings/
│   │   └── Index.cshtml                  # Rate & monthly budget configuration form
│   ├── Shared/
│   │   ├── _Layout.cshtml                # App shell, responsive sidebar, navigation
│   │   ├── _Layout.cshtml.css            # Scoped layout stylesheet
│   │   ├── _ValidationScriptsPartial.cshtml # jQuery unobtrusive validation
│   │   └── Error.cshtml                  # Friendly error page template
│   └── Simulation/
│       └── Index.cshtml                  # Interactive stepper scenario sandbox
├── wwwroot/
│   ├── css/
│   │   └── site.css                      # Design tokens, variables, & responsive grid
│   └── js/
│       └── site.js                       # Real-time DOM calculators & stepper math
├── appsettings.json                      # Connection strings & app configuration
├── ELECTRICITRACK.csproj                 # .NET 10 project & package definitions
├── electricitrack.db                     # Embedded SQLite relational database
└── Program.cs                            # Entry point, DI container, & DB seeder
```

### 3.2 Architectural Layer Mapping

| Layer / Folder         | Component Files                | Core Architectural Role     |
| :--------------------- | :----------------------------- | :-------------------------- |
| **`Services/`**        | `ConsumptionAnalysisService.cs`| Heap Sort, Binary Search, ₱ |
| **`Controllers/`**     | `DashboardController.cs`, etc. | Request routing & ViewModels|
| **`Data/` & `Models/`**| `AppDbContext`, `Appliance`    | SQLite persistence & rules  |
| **`ViewModels/`**      | `DashboardViewModel.cs`, etc.  | Strongly-typed UI contracts |
| **`Views/`**           | `Dashboard/`, `Appliances/`    | Razor server-rendered HTML  |
| **`wwwroot/`**         | `css/site.css`, `js/site.js`   | Design tokens & client DOM  |

### 3.3 Front-End Component Reference Table

| Component View         | File Location                  | Primary Role                |
| :--------------------- | :----------------------------- | :-------------------------- |
| **App Shell Layout**   | `Views/Shared/_Layout.cshtml`  | Sidebar, rate note, topbar  |
| **Dashboard Overview** | `Views/Dashboard/Index.cshtml` | Metrics, bar chart, table   |
| **Appliance Manager**  | `Views/Appliances/Index.cshtml`| Appliance list & edit form  |
| **Scenario Simulator** | `Views/Simulation/Index.cshtml`| What-if steppers & savings  |
| **Settings View**      | `Views/Settings/Index.cshtml`  | Rate & monthly budget form  |
| **Design System CSS**  | `wwwroot/css/site.css`         | Design tokens & responsive  |
| **Client Scripting**   | `wwwroot/js/site.js`           | Live math, DOM, & steppers  |

#### Detailed Component Roles
* **App Shell Layout (`_Layout.cshtml`):** Provides the persistent sidebar navigation, household identifier ("Santos home"), active electricity rate indicator (₱/kWh), top breadcrumb bar, and user profile avatar.
* **Dashboard Overview (`Dashboard/Index.cshtml`):** Renders summary metric cards (kWh, Cost in ₱, Budget status), the thermal consumption gradient bar, top consumer callout, and the Heap-sorted appliance table.
* **Appliance Manager (`Appliances/Index.cshtml`):** Dual-pane master-detail view: left panel lists all tracked appliances; right panel hosts reactive edit/create forms with live calculation preview badges.
* **Scenario Simulator (`Simulation/Index.cshtml`):** What-if modeling lab: stepper buttons (`−` / `＋`) adjust daily hours; real-time monthly savings and budget-clamping guidance update live without page reloads.
* **Settings View (`Settings/Index.cshtml`):** Form for configuring household electricity rate per kWh (default ₱12.00) and monthly budget ceiling (default ₱5,000.00).
* **Design System CSS (`site.css`):** Global design tokens (`--page`, `--surface`, `--ink`, `--blue`, `--danger`), flexbox/grid containers, and responsive media queries.
* **Client Scripting (`site.js`):** Zero-dependency ES6 script for live form calculation, half-hour steppers, progress bar animation, and currency formatting (`Intl.NumberFormat`).

---

## 4. Programming Languages & Technologies

| Technology             | Specification         | Layer / Area          | Primary Role                |
| :--------------------- | :-------------------- | :-------------------- | :-------------------------- |
| **C#**                 | C# 13 (.NET 10.0)     | Backend, Logic, Model | Primary app language & math |
| **Razor (`.cshtml`)**  | ASP.NET Core Razor    | Server Presentation   | Dynamic server-rendered HTML|
| **HTML5**              | Modern Web Standard   | Document Structure    | Semantic accessible markup  |
| **CSS3**               | CSS Variables & Grid  | Visual Styling        | Design tokens & responsive  |
| **JavaScript**         | ECMAScript 6+         | Client Interaction    | Real-time DOM calculations  |
| **SQLite**             | SQLite 3 Relational DB| Persistence Layer     | Embedded relational database|

#### Technology Details
* **C# 13 / .NET 10.0:** Uses primary constructors, pattern matching, LINQ, and collection expressions (`[]`).
* **ASP.NET Core Razor:** Blends standard HTML5 with C# server expressions (`@Model`, `@foreach`) for dynamic, XSS-safe view rendering.
* **HTML5:** Semantic elements (`<main>`, `<article>`, `<section>`, `<aside>`, `<table>`) with accessibility attributes (`aria-label`, `role`).
* **CSS3:** Custom properties (variables), Flexbox layouts, multi-column Grid layouts, and responsive breakpoints down to mobile.
* **Vanilla JavaScript:** Zero-dependency client scripting for live form calculation, half-hour steppers, and currency formatting.
* **SQLite:** Embedded relational database management system managed via Entity Framework Core code-first migrations.

---

## 5. Comprehensive Inventory of Methods & Functions

### 5.1 Business Logic & Algorithm Service (`Services/ConsumptionAnalysisService.cs`)

| Method Name                    | Return Type       | Algorithm / Formula       | Primary Purpose            |
| :----------------------------- | :---------------- | :------------------------ | :------------------------- |
| **`MonthlyKwh`**               | `decimal`         | `(W / 1000) * hrs * days` | Monthly energy in kWh      |
| **`MonthlyCost`**              | `decimal`         | `MonthlyKwh * rate`       | Monthly cost in ₱          |
| **`Money`**                    | `string`          | `$"₱{amount:N2}"`         | Localized ₱ currency string|
| **`SortByMonthlyConsumption`** | `List<Appliance>` | **Heap Sort** (O(N log N))| Ranks by kWh descending    |
| **`SiftDown`**                 | `void`            | **Max-Heapify** (O(log N))| Sifts down root in heap    |
| **`CompareConsumption`**       | `int`             | Comparator                | Compares kWh / name tie    |
| **`SearchByNamePrefix`**       | `List<Appliance>` | **Binary Search** (O(logN)| Fast prefix search lookup  |

#### Method Details
* **`MonthlyKwh(Appliance)`:** Formula: $(\text{PowerWatts} / 1000) \times \text{HoursPerDay} \times \text{DaysPerMonth}$. Calculates monthly kWh.
* **`MonthlyCost(Appliance, decimal rate)`:** Formula: $\text{MonthlyKwh} \times \text{rate}$. Calculates monthly cost in Philippine Pesos.
* **`Money(decimal amount)`:** Formats numbers into Philippine Peso currency strings (e.g. `₱1,250.00`).
* **`SortByMonthlyConsumption(apps)`:** **Heap Sort ($O(N \log N)$)** builds an in-place Max-Heap via `SiftDown`, extracts maxima, and yields a descending list.
* **`SiftDown(items, root, count)`:** Internal Max-Heapify helper comparing child nodes with parents to restore heap property.
* **`CompareConsumption(left, right)`:** Compares monthly kWh between appliances; falls back to an alphabetical name comparison on ties.
* **`SearchByNamePrefix(apps, search)`:** **Binary Search ($O(\log N)$)** finds the starting index of matching prefix names in logarithmic time, then aggregates all matches.

---

### 5.2 Controller Actions (`Controllers/`)

| Controller                  | Action Method                         | Verb       | Primary Purpose            |
| :-------------------------- | :------------------------------------ | :--------- | :------------------------- |
| **`DashboardController`**   | `Index(string? search, string? sort)` | `GET`      | Overview, Heap Sort, search|
| **`DashboardController`**   | `GetThermalIntensityGradient(i, N)`   | `Internal` | 3-stage Coral-to-Blue ramp |
| **`AppliancesController`**  | `Index(int? id, bool create)`         | `GET`      | Appliance list & editor    |
| **`AppliancesController`**  | `Save(Appliance appliance)`           | `POST`     | Insert / update appliance  |
| **`AppliancesController`**  | `Delete(int id)`                      | `POST`     | Remove appliance record    |
| **`AppliancesController`**  | `ReturnEditorAsync(...)`              | `Internal` | Validation error reload    |
| **`SimulationController`**  | `Index()`                             | `GET`      | Baseline simulation view   |
| **`SimulationController`**  | `Apply(SimulationApplyInput)`         | `POST`     | Save adjusted daily hours  |
| **`SimulationController`**  | `BuildViewModel(...)`                 | `Internal` | Savings & budget guidance  |
| **`SettingsController`**    | `Index()`                             | `GET`      | Display rate & budget      |
| **`SettingsController`**    | `Save(HouseholdSettings)`             | `POST`     | Save utility settings      |
| **`SettingsController`**    | `SetSidebarValues(...)`               | `Internal` | Update ViewData sidebar    |

#### Controller Action Details
* **`DashboardController.Index`:** Fetches appliances and settings, applies Heap Sort for ranking, executes prefix Binary Search if requested, generates thermal heatmap colors, and renders overview.
* **`DashboardController.GetThermalIntensityGradient`:** 3-stage RGB color ramp: Coral Red (`#F43F5E`) for top consumer $\rightarrow$ Warm Amber (`#F59E0B`) $\rightarrow$ Royal Purple (`#8B5CF6`) $\rightarrow$ Electric Blue (`#2563EB`) for lowest consumer.
* **`AppliancesController.Save`:** Validates model inputs; inserts new appliance or updates existing entity properties in SQLite, then saves changes.
* **`AppliancesController.Delete`:** Removes target appliance by primary key and commits transaction.
* **`SimulationController.Apply`:** Validates proposed scenario adjustments and persists new hours per day to database.
* **`SimulationController.BuildViewModel`:** Evaluates adjusted costs, calculates savings, and computes recommended hours boundary clamping: $\text{Clamp}(\frac{\text{RemainingBudget}}{\text{HourlyCost}}, 0, 24)$.

---

### 5.3 Client-Side JavaScript Functions (`wwwroot/js/site.js`)

| Function Name            | Trigger / Event                      | Primary Purpose             |
| :----------------------- | :----------------------------------- | :-------------------------- |
| **`money(value)`**       | Helper function                      | Formats numbers into ₱      |
| **`decimal(value)`**     | Helper function                      | Formats kWh to 1 decimal    |
| **`updateEstimate()`**   | `input` event on wattage, hours, days| Real-time form math preview |
| **`updateSimulation()`** | Stepper clicks or manual hour edits  | Computes savings & budget   |
| **Stepper Handlers**     | Click on `[data-step]` (`−` or `＋`) | Adjusts hours by ±0.5 hours |

#### Function Details
* **`money(value)`:** Uses `Intl.NumberFormat("en-PH")` to format numbers into two-decimal Philippine Peso currency strings.
* **`decimal(value)`:** Formats numbers to one decimal place for kilowatt-hour displays.
* **`updateEstimate()`:** Multiplies form values locally and updates the live estimate card in real time without page reload.
* **`updateSimulation()`:** Iterates across all simulation table rows, calculates dynamic energy savings per row, aggregates total adjusted cost and kWh, updates progress bar widths, and checks budget status.
* **Stepper Handlers:** Adjusts target hour field by $\pm 0.5$ hours within $[0, 24]$ boundary and invokes `updateSimulation()`.

---

## 6. Object-Oriented Programming (OOP) Principles Applied

ELECTRICITRACK is structured around the four fundamental pillars of Object-Oriented Programming, combined with SOLID clean architecture principles:

| OOP Principle     | Where Applied                 | Core Mechanism                       |
| :---------------- | :---------------------------- | :----------------------------------- |
| **Encapsulation** | `Appliance`, Service helpers  | Private `SiftDown`, `[Range]` bounds |
| **Abstraction**   | Services & ViewModels         | Hides Heap Sort tree & raw SQL       |
| **Inheritance**   | Controllers, `AppDbContext`   | Subclasses `Controller`, `DbContext` |
| **Polymorphism**  | Service method signatures     | Accepts `IEnumerable<Appliance>`     |

### 6.1 Detailed OOP Principles Breakdown

* **1. Encapsulation**
  * **Where Applied:** `Appliance.cs`, `HouseholdSettings.cs`, `ConsumptionAnalysisService.cs`
  * **Mechanism:** Auto-properties with getters/setters, validation attributes (`[Range]`, `[Required]`, `[StringLength]`), and `private static` helpers (`SiftDown`, `CompareConsumption`).
  * **Benefit:** Prevents illegal state (e.g. negative hours or >24h days); hides internal heap array indexing from external consumers.

* **2. Abstraction**
  * **Where Applied:** `ConsumptionAnalysisService.cs`, `AppDbContext.cs`, ViewModels
  * **Mechanism:** `SortByMonthlyConsumption` hides binary tree math behind a clean public method; `AppDbContext` abstracts raw SQL behind LINQ; ViewModels decouple DB entities from Razor views.
  * **Benefit:** Controllers only request sorted data without needing knowledge of binary heap algorithms; database changes don't break UI views.

* **3. Inheritance**
  * **Where Applied:** All Controllers (`DashboardController`, `AppliancesController`, etc.) and `AppDbContext`
  * **Mechanism:** Subclasses ASP.NET Core `Controller` and Entity Framework Core `DbContext`.
  * **Benefit:** Inherits built-in HTTP dispatch, `View()`, `RedirectToAction()`, `ModelState`, and change tracking without boilerplate.

* **4. Polymorphism**
  * **Where Applied:** Service method signatures and framework overrides
  * **Mechanism:** Accepts `IEnumerable<Appliance>` interface abstractions; overrides framework lifecycle hooks (`OnModelCreating`, `Dispose`).
  * **Benefit:** Algorithms seamlessly process an in-memory `List<Appliance>`, an EF Core queryable stream, or an `Appliance[]` array interchangeably.

---

### 6.2 Code Highlights of OOP Principles

#### Encapsulation Example
```csharp
// Data validation and private algorithmic helpers
public class Appliance
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "24")]
    public decimal HoursPerDay { get; set; }
}

public class ConsumptionAnalysisService
{
    // Public API exposes clean interface
    public List<Appliance> SortByMonthlyConsumption(IEnumerable<Appliance> appliances) { ... }

    // Internal heapification is strictly private
    private static void SiftDown(Appliance[] items, int root, int count) { ... }
}
```

#### Abstraction & Inheritance Example
```csharp
// Inheritance from base Controller & abstraction of services
public class DashboardController(
    AppDbContext db,
    ConsumptionAnalysisService analysis) : Controller // <-- Inheritance
{
    public async Task<IActionResult> Index(string? search, string? sort = "high-to-low")
    {
        var appliances = await db.Appliances.AsNoTracking().ToListAsync();
        
        // Abstraction: Controller has no knowledge of tree/heap logic
        var ranked = analysis.SortByMonthlyConsumption(appliances); 
        
        return View(new DashboardViewModel { Appliances = ranked });
    }
}
```

#### Polymorphism Example
```csharp
// Polymorphic parameter: accepts any collection implementing IEnumerable<Appliance>
public List<Appliance> SortByMonthlyConsumption(IEnumerable<Appliance> appliances)
{
    var result = appliances.ToArray();
    // Operates identically whether passed a List, an Array, or an EF Core queryable
    ...
}
```

---

## 7. SOLID Principles & Architecture Summary

* **Single Responsibility Principle (SRP):**
  * `Appliance.cs` defines data structure and validation rules.
  * `ConsumptionCalculator` handles mathematical formulas.
  * `ConsumptionAnalysisService` executes sorting and search algorithms.
  * Controllers handle HTTP routing and view coordination.
* **Dependency Injection (IoC):**
  All services are registered in the DI container in `Program.cs` (`builder.Services.AddScoped<ConsumptionAnalysisService>()`) and injected into controller primary constructors, enabling loose coupling and testability.

---

*Document compiled and placed on Desktop: `C:\Users\julez\Desktop\ELECTRICITRACK_Architecture_and_Technical_Documentation.md`*
