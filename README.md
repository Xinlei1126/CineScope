# CineScope

CineScope is a movie discovery and community review website built with ASP.NET Core MVC. Visitors can explore and search a movie catalog. Registered members can save movies and share ratings and reviews. Administrators can manage the catalog, review accounts, and moderate community reviews.

## Website features

- **Movie discovery:** Responsive home page with a featured movie, movie cards, genre filters, and title search.
- **Movie details:** Poster, genre, release year, runtime, catalog rating, description, member review average, and reviews.
- **Member accounts:** Register, sign in, sign out, and role-based access using ASP.NET Core Identity.
- **Personal list:** Members can save and remove favorite movies.
- **Reviews and ratings:** Members can rate a movie from 1 to 10 and post a written review. Each member has one review per movie; submitting again updates it.
- **Administration:** Admins can create, edit, and delete catalog entries, list accounts, lock or unlock accounts, and hide, approve, or remove reviews.
- **Responsive dark interface:** Razor views, Bootstrap layout utilities, and custom CSS support desktop and mobile screens.

Guests can browse, search, filter, and view movie details. Sign-in is required for member actions. Admin routes require the `Admin` role.

## Technologies

| Area | Technology |
|---|---|
| Application | ASP.NET Core 10 MVC, C# |
| Views | Razor, Bootstrap 5.3, custom CSS |
| Database | SQL Server or Windows LocalDB |
| Data access | Entity Framework Core 10, SQL Server provider |
| Authentication | ASP.NET Core Identity, cookie authentication, role authorization |
| Static assets | Local CSS, Bootstrap CDN, Google Fonts CDN, TMDB image URLs |

NuGet dependencies are declared in `CineScope.csproj`:

- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Design` (development tooling)

The site uses TMDB-hosted poster image URLs for sample records. It does not call the TMDB API and does not require a TMDB API key.

## How the application works

1. **Startup and configuration:** `Program.cs` registers MVC, the EF Core database context, Identity, roles, authentication, and authorization middleware.
2. **Database initialization:** `DatabaseInitializer` applies available EF Core migrations. For an older CineScope database created with `EnsureCreated`, it can record the initial migration as applied when all expected CineScope tables exist and the migration history is empty. It then creates the `Member` and `Admin` roles if needed.
3. **Initial data:** If the movie catalog is empty, four sample movies are inserted. This runs independently of whether admin credentials are configured.
4. **Identity:** New registrations receive the `Member` role. An administrator account is created or promoted when both `CineScope:AdminEmail` and `CineScope:AdminPassword` are configured.
5. **Requests:** MVC controllers handle page requests and form submissions. Controllers query or update the database through `ApplicationDbContext`, then return Razor views or redirects.
6. **Authorization:** Guests can browse. Member actions such as favorites and reviews require authentication and the `Member` or `Admin` role. Catalog management and moderation require the `Admin` role.

## Project structure

```text
CineScope/
├── Controllers/
│   ├── AccountController.cs       # Registration, sign-in, sign-out
│   ├── AdminController.cs         # Catalog, account, and review administration
│   ├── HomeController.cs          # Home page, search, genre filtering
│   └── MoviesController.cs        # Details, favorites, reviews
├── Data/
│   ├── ApplicationDbContext.cs    # EF Core and Identity database model
│   └── DatabaseInitializer.cs     # Migrations, roles, admin, sample catalog
├── Models/
│   ├── ApplicationUser.cs         # Identity user profile
│   ├── Movie.cs                   # Movie catalog record
│   ├── MovieFavorite.cs           # Member-to-movie favorites
│   └── MovieReview.cs             # Member ratings and reviews
├── ViewModels/                    # View-specific data and account forms
├── Views/                         # Razor MVC pages and shared components
├── wwwroot/css/site.css           # Site styling and responsive layout
├── Program.cs                     # Dependency injection and HTTP pipeline
├── appsettings.json               # Default development configuration
└── CineScope.csproj               # .NET target and NuGet dependencies
```

## Requirements

- .NET 10 SDK
- SQL Server, Azure SQL, or Windows LocalDB
- `dotnet-ef` tool version 10 for creating and applying migrations from the command line

The default connection string in `appsettings.json` uses Windows LocalDB. For another SQL Server, set `ConnectionStrings__DefaultConnection` in the environment or update the configuration for local development.

## Run locally

From the project directory, restore packages and create the initial migration before the first run:

```powershell
dotnet restore
dotnet tool install --global dotnet-ef --version 10.0.0
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

If `dotnet-ef` is already installed, update it with:

```powershell
dotnet tool update --global dotnet-ef --version 10.0.0
```

Open the local URL printed by `dotnet run` in the terminal.

## Configure an administrator

Set both environment variables before starting the application. For PowerShell:

```powershell
$env:CineScope__AdminEmail = "admin@example.com"
$env:CineScope__AdminPassword = "Choose-A-Strong-Password-123"
dotnet run
```

The password must satisfy the Identity password policy. Do not commit real credentials or production connection strings to source control. If these settings are omitted, the site still creates the Member and Admin roles and seeds the sample movie catalog; it simply does not create an administrator account.

## Database model

- **Movie:** title, genre, release year, catalog rating, runtime, poster URL, and description.
- **ApplicationUser:** Identity credentials and optional display name.
- **MovieReview:** movie, member, 1–10 rating, review body, date, and moderation status. A unique index prevents multiple reviews by the same user for the same movie.
- **MovieFavorite:** join table with a composite key of user ID and movie ID.
- **Identity tables:** users, roles, claims, logins, tokens, and role membership.

Movie records are deleted with their associated reviews and favorites. Review averages shown on the details page are calculated from approved member reviews; the movie's catalog rating is stored separately.

## Main routes

| URL | Purpose | Access |
|---|---|---|
| `/` | Browse, search, and filter movies | Guest |
| `/Movies/Details/{id}` | Movie information and reviews | Guest |
| `/Movies/Favorites` | Saved movies | Member or Admin |
| `/Account/Register` | Create a member account | Guest |
| `/Account/Login` | Sign in | Guest |
| `/Admin` | Create, edit, and delete movies | Admin |
| `/Admin/Reviews` | Moderate reviews | Admin |
| `/Admin/Users` | View and lock/unlock accounts | Admin |

## Existing database migration conflict

If an earlier version created the database with `EnsureCreated`, the tables may exist even though `__EFMigrationsHistory` does not record `InitialCreate`. The initializer in this project handles a complete legacy CineScope schema: replace the previous `Data/DatabaseInitializer.cs`, stop the app, and run `dotnet run` once. Startup records the initial migration as applied before applying migrations. This check preserves existing records and only runs when all expected tables exist and the migration history has no applied rows.

If the legacy database is incomplete or differs from the current CineScope schema, back it up and configure a new database name in the connection string. Do not delete individual Identity tables to work around migration errors.

## Troubleshooting

- **“No movies found”:** Confirm the app is connected to the expected database, remove search text and genre filters, then restart the app. The initializer seeds sample movies when the connected database has none.
- **`AspNetRoles` already exists during migration:** Follow the existing database migration steps above. Verify the app is using the intended connection string.
- **Login page rejects a password:** New passwords must be at least eight characters and include a digit.
- **Admin pages show access denied:** Configure the admin email and password, restart the app, then sign in with that account.
- **Poster images do not load:** The sample poster URLs are remote assets and require internet access; movie text and other features remain available.

## Security and deployment notes

- Use HTTPS and a production-grade SQL Server connection in deployed environments.
- Store secrets in environment variables or a managed secret store, never in committed settings files.
- Configure a strong admin password and protect the database connection string.
- The project currently loads Bootstrap and Google Fonts from CDNs; provide local assets if the deployment must work without external content delivery.
- No external movie metadata API, real-time service, or cloud deployment configuration is included.
