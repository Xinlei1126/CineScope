# CineScope — Levels 1–4

A dark, responsive movie discovery and community review app built with ASP.NET Core 10 MVC, Razor Views, SQL Server, Entity Framework Core, and ASP.NET Core Identity.

## Included

- **Level 1:** Movie CRUD (admin), title search, genre filtering, details, server-side validation, SQL Server persistence.
- **Level 2:** Responsive dark discovery home, featured hero, movie cards, hover states, category filters, and mobile layouts.
- **Level 3:** Registration and sign in, cookie authentication, Member/Admin roles, protected admin catalog, user listing, and review moderation.
- **Level 4:** Member favorites, one editable rating/review per movie, community rating averages, and review display.

Guests can browse, search, filter, and open movie details. Registered accounts automatically receive the `Member` role. Admins can create, edit, and delete movies and moderate user reviews.

## Requirements

- .NET 10 SDK
- SQL Server (or LocalDB on Windows)

The default development connection is LocalDB in `appsettings.json`. Set `ConnectionStrings__DefaultConnection` to a SQL Server connection string in other environments.

## Run locally

```bash
dotnet restore
dotnet tool install --global dotnet-ef --version 10.0.0
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Create the initial migration before the first run. The application applies available migrations on startup.

### Fixing an existing `AspNetRoles` table conflict

If you already ran an earlier version that used `EnsureCreated`, SQL Server may have the CineScope tables but no `InitialCreate` row in `__EFMigrationsHistory`. The initializer now checks for the complete legacy CineScope schema and baselines that initial migration without deleting data.

For that existing database, replace the old `Data/DatabaseInitializer.cs` with this package version, stop the app, then run:

```powershell
dotnet run
```

The startup initializer records `InitialCreate` as applied only when all expected tables exist, then migration startup continues. Once the app starts successfully, `dotnet ef database update` can be used for later migrations. For a brand-new database, `dotnet ef database update` can run before `dotnet run` as usual.

If the existing database has only some of the expected tables, back it up and use a fresh database name in `ConnectionStrings:DefaultConnection` instead; the migration will create the schema there without touching the old database.

Set admin credentials using environment variables before starting the app:

```bash
# bash
export CineScope__AdminEmail='admin@example.com'
export CineScope__AdminPassword='Choose-A-Strong-Password-123'
dotnet run
```

On Windows PowerShell use `$env:CineScope__AdminEmail` and `$env:CineScope__AdminPassword`.

The configured administrator is created and assigned the `Admin` role on startup. If admin credentials are not configured, the app still seeds its sample movie catalog; create an account and use the admin setup for catalog management. Do not commit production credentials or connection strings to source control. Configure a production-grade SQL Server connection and HTTPS before deployment.

## Data model

- `Movie`: catalog metadata and poster URL.
- `ApplicationUser`: Identity account and display name.
- `MovieReview`: one rating/review per member and movie; unique `(UserId, MovieId)` index.
- `MovieFavorite`: member-to-movie join table with composite key.
- Identity role tables: `Member` and `Admin`.

## Routes

- `/` — browse, search, and filter.
- `/Movies/Details/{id}` — movie details, reviews, and rating.
- `/Movies/Favorites` — signed-in member watchlist.
- `/Account/Register`, `/Account/Login` — membership.
- `/Admin` — admin catalog CRUD.
- `/Admin/Reviews`, `/Admin/Users` — moderation and account listing.

## Notes

Poster URLs in the demo catalog point to TMDB image assets; the app does not call TMDB or require an API key. Levels 5–7 and deployment deliverables from the supplied brief are outside this Level 1–4 implementation. The source was updated to address the legacy schema conflict, but build and database migration execution could not be verified in this environment because the .NET SDK is not installed.
