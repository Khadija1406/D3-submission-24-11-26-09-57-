# Pulse — ASP.NET Core MVC Conversion

This project is a complete, enterprise-grade conversion of the standalone Pulse application into an **ASP.NET Core MVC** architecture adhering to clean architecture principles.

---

## Architecture & Feature Highlights

1. **Clean MVC Structure & Partial Views**:
   - The main `Index.cshtml` view is neat and modular, composed entirely of partial views.
   - Master layout `_Layout.cshtml` implements standard sections: `@RenderSection("Styles")` and `@RenderSection("Scripts")`.
   - 100% of the original visual design, themes, OKLCH colors, responsive layout, SVGs, DM Sans typography, and interactions are preserved.

2. **Dependency Injection & Interface Segregation**:
   - `IDbConnectionFactory` / `SqliteConnectionFactory`: Decouples database connection lifetime.
   - `IProblemRepository` / `ProblemRepository`: High-performance data access with Dapper.
   - `IUserRepository` / `UserRepository`: User management repository with Dapper.
   - `IMigrationRunner` / `MigrationRunner`: Automated SQL migration engine.
   - Registered cleanly in `Program.cs` using `.AddScoped<>()`, `.AddSingleton<>()`, and `.AddTransient<>()`.

3. **Database Access with Dapper & Automated Migrations**:
   - Uses **Dapper** for fast, parameterized SQL queries.
   - Self-contained SQLite database (`pulse.db`) with zero external database configuration needed.
   - Automated migration system tracking executed scripts in `__MigrationsHistory`.
   - Migration scripts located in `Data/Migrations/Scripts/`:
     - `001_CreateIdentityTables.sql`: Identity schema (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`).
     - `002_CreateProblemsTables.sql`: Problems schema (`Problems`, `ProblemOccurrences`, `ProblemTriedSolutions`).

4. **Real Database Data (No Hardcoded Mock Data)**:
   - On initial startup, `DatabaseSeeder` automatically creates a real user:
     - **Email / Username**: `alex@pulse.app`
     - **Password**: `Password123!`
     - **Full Name**: `Alex Rivera`
   - Real problem records with historical occurrences and tried solutions are stored in `pulse.db` and fetched dynamically on page load.

5. **Custom ASP.NET Core Identity**:
   - Custom `ApplicationUser` extending `IdentityUser` with `FullName`, `CreatedAt`, and `AvatarUrl`.
   - Custom `DapperUserStore` implementing:
     - `IUserStore<ApplicationUser>`
     - `IUserEmailStore<ApplicationUser>`
     - `IUserPasswordStore<ApplicationUser>`
     - `IUserSecurityStampStore<ApplicationUser>`
   - Custom `DapperRoleStore` implementing `IRoleStore<IdentityRole>`.
   - Integrated with standard `SignInManager<ApplicationUser>` and `UserManager<ApplicationUser>`.

6. **AJAX Implementation**:
   - **Authentication (`wwwroot/js/auth.js`)**:
     - `POST /Account/LoginAjax`: Async authentication returning user credentials, toast feedback, and immediate dashboard navigation.
     - `POST /Account/RegisterAjax`: Async user registration, password validation, session creation, and onboarding.
     - `POST /Account/LogoutAjax`: Async session termination.
   - **Problems Tracker (`wwwroot/js/pulse-app.js`)**:
     - `GET /Problems/GetAll`: Loads real problems from the database via Dapper.
     - `POST /Problems/Create`: Adds new problems with initial occurrence into the database.
     - `POST /Problems/UpdateStatus`: Updates problem status ('active', 'progress', 'resolved').
     - `POST /Problems/AddOccurrence`: Logs historical occurrences.
     - `POST /Problems/SetTriedSolution`: Tracks suggestion feedback.
     - `POST /Problems/Delete`: Deletes problems and their cascading histories.

---

## Directory Structure

```
PulseMvc/
├── Controllers/
│   ├── HomeController.cs           # Landing page controller
│   ├── AccountController.cs        # Identity authentication & AJAX endpoints
│   └── ProblemsController.cs       # Problem tracking Dapper endpoints
├── Models/
│   ├── ApplicationUser.cs          # Custom Identity user
│   ├── Problem.cs                  # Problem entity
│   ├── ProblemOccurrence.cs        # Occurrence entity
│   ├── ProblemTriedSolution.cs     # Tried solution entity
│   └── ViewModels/
│       ├── LoginViewModel.cs       # Login form validation model
│       ├── RegisterViewModel.cs    # Registration form validation model
│       ├── ProblemDto.cs           # JSON transfer models
│       └── ApiResponse.cs          # Standard API envelope
├── Data/
│   ├── IDbConnectionFactory.cs     # Connection factory interface
│   ├── SqliteConnectionFactory.cs  # SQLite factory implementation
│   ├── DapperContext.cs            # Dapper connection context
│   ├── Migrations/
│   │   ├── IMigrationRunner.cs     # Migration runner interface
│   │   ├── MigrationRunner.cs      # Automated SQL migration runner
│   │   └── Scripts/
│   │       ├── 001_CreateIdentityTables.sql
│   │       └── 002_CreateProblemsTables.sql
│   └── Seed/
│       └── DatabaseSeeder.cs       # Real user & problem seeder
├── Repositories/
│   ├── Interfaces/
│   │   ├── IProblemRepository.cs   # Problem repository interface
│   │   └── IUserRepository.cs      # User repository interface
│   ├── ProblemRepository.cs        # Dapper problem repository
│   └── UserRepository.cs           # Dapper user repository
├── Identity/
│   ├── DapperUserStore.cs          # Custom Dapper UserStore
│   └── DapperRoleStore.cs          # Custom Dapper RoleStore
├── Views/
│   ├── _ViewImports.cshtml
│   ├── _ViewStart.cshtml
│   ├── Shared/
│   │   ├── _Layout.cshtml          # Master layout with Sections
│   │   ├── _Header.cshtml          # Site navigation header
│   │   ├── _Hero.cshtml            # Hero artwork & intro
│   │   ├── _PromiseStrip.cshtml    # Value proposition strip
│   │   ├── _HowItWorks.cshtml      # Step-by-step workflow
│   │   ├── _YourWorld.cshtml       # Category cards
│   │   ├── _WhyPulse.cshtml        # Pattern switch & analysis demo
│   │   ├── _Features.cshtml        # Feature overview
│   │   ├── _Progress.cshtml        # Progress tracker
│   │   ├── _Privacy.cshtml         # Trust & security
│   │   ├── _Closing.cshtml         # Final CTA section
│   │   ├── _Footer.cshtml          # Site footer
│   │   ├── _LoginModal.cshtml      # Identity Login modal
│   │   ├── _SignupModal.cshtml     # Identity Signup modal
│   │   ├── _PulseApp.cshtml        # Problem tracker dashboard view
│   │   └── _Toast.cshtml           # Notification container
│   ├── Home/
│   │   └── Index.cshtml            # Clean Index page
│   └── Account/
│       ├── Login.cshtml            # Dedicated login view
│       └── Register.cshtml         # Dedicated signup view
├── wwwroot/
│   ├── css/
│   │   ├── pulse-base.css          # Base layout & embedded DM Sans fonts
│   │   ├── pulse-polish.css        # Auth & landing polish styles
│   │   └── pulse-app.css           # Problem tracker app styles
│   └── js/
│       ├── pattern-switch.js       # Interactive pattern switcher
│       ├── auth.js                 # AJAX Identity login & signup
│       └── pulse-app.js            # Problem tracker & Dapper backend sync
├── Program.cs                      # Application entrypoint & DI setup
├── appsettings.json                # Database connection string
├── PulseMvc.csproj                 # Project manifest
└── pulse.db                        # Pre-seeded SQLite database
```

---

## How to Run

1. Open a terminal in the `PulseMvc` folder:
   ```bash
   cd PulseMvc
   ```
2. Run the application:
   ```bash
   dotnet run
   ```
3. Open your browser and navigate to the displayed URL (e.g. `https://localhost:5001` or `http://localhost:5000`).

### Pre-seeded Credentials
- **Email**: `alex@pulse.app`
- **Password**: `Password123!`
