# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```sh
dotnet build MyMovieScore.sln                       # build (CI uses --configuration Release)
dotnet run --project MyMovieScore.Api               # run the API (needs SQL Server, see below)
docker compose up -d sqlserver                      # start only the database for dotnet run
docker compose up -d --build                        # full stack; API on http://localhost:5000, Swagger at /swagger
```

There are no test projects yet. CI ([.github/workflows/ci.yml](.github/workflows/ci.yml)) runs restore, a Release build and `dotnet test` on the solution, so a new test project must be added to `MyMovieScore.sln` to be picked up.

EF Core migrations live in the Infrastructure project, with the Api project as startup:

```sh
dotnet ef migrations add <Name> --project MyMovieScore.Infrastructure --startup-project MyMovieScore.Api --output-dir Persistence/Migrations
```

Pending migrations are applied automatically at startup in `Program.cs` when `Database:ApplyMigrationsOnStartup` is true (the default), so there is no need to run `dotnet ef database update`.

## Configuration

- `ConnectionStrings:MyMovieScoreCs` in `appsettings.json` points to `localhost,1433`; Docker Compose overrides it to reach the `sqlserver` service by name.
- Settings are bound to options classes in Infrastructure: `Jwt` → `JwtOptions`, `ExternalService` → `ExternalServiceOptions` (OMDb `BaseUrl`, `Key`, `Plot`). Inject `IOptions<T>` rather than reading `IConfiguration` by string.
- `ExternalService:Key` (OMDb API key) is intentionally empty in `appsettings.json`. Compose fills it from `OMDB_API_KEY` in `.env` (copy `.env.example`); with `dotnet run`, use `dotnet user-secrets set "ExternalService:Key" "<key>" --project MyMovieScore.Api`. Creating a movie throws if the key is missing.
- `JwtOptions` is used both by the JWT bearer setup in `Program.cs` and by `AuthService` when issuing tokens (`Jwt:ExpirationHours` sets token lifetime).
- `IMDbExternalService` is a typed `HttpClient` registered with `AddHttpClient`; its `BaseAddress` comes from `ExternalService:BaseUrl`.
- `Swagger:Enabled` (false in `appsettings.json`, true in `appsettings.Development.json`) and `Database:ApplyMigrationsOnStartup` are flags read in `Program.cs`.

## Architecture

.NET 8 ASP.NET Core API using clean architecture + CQRS (MediatR 10). Dependencies point inward: Api → Application → Core ← Infrastructure (Api also references Infrastructure for DI wiring).

- **Core**: entities (`User`, `Movie`, `ExternalRatings`, all deriving `BaseEntity`), repository interfaces, and service interfaces (`IAuthService`, `IIMDbExternalService`). Entities use private setters and constructors; state changes go through methods like `Movie.Update(...)`.
- **Application**: one folder per command/query under `Commands/` and `Queries/`, each holding the request record and its `IRequestHandler`. Handlers depend only on Core interfaces. FluentValidation validators in `Validators/` validate the command objects; view models in `ViewModels/` are what queries return.
- **Infrastructure**: `MyMovieScoreDbContext` (entity configs are picked up via `ApplyConfigurationsFromAssembly` from `Persistence/Configurations`), repositories, `AuthService` (JWT generation + `PasswordHasher` PBKDF2 hashing), and `IMDbExternalService` (OMDb HTTP client).
- **Api**: thin controllers that build a command/query and `_mediator.Send` it. All DI registration is in `Program.cs`; MediatR handlers are registered from the Application assembly via `typeof(CreateMovieCommand)` and validators via `CreateUserCommandValidator`. Movie endpoints require a JWT.

Cross-cutting behavior worth knowing:

- Validation runs through FluentValidation's MVC integration; `ValidationFilter` turns invalid `ModelState` into a 400 with a list of error messages. Handlers do not validate.
- Creating a movie: `CreateMovieCommandHandler` calls OMDb by IMDb id, which returns a `Movie` populated with title/plot/release/genre and `ExternalRatings`; the handler then builds the persisted `Movie` from those fields plus the user's `UserId`/`Watched`/`UserScore`.
- Repositories call `SaveChangesAsync` themselves (no unit of work). `UpdateAsync` relies on EF change tracking: load the entity via the repository, mutate it, then call `UpdateAsync`.

A Postman collection is in `MyMovieScore.postman_collection.json`; the README lists all endpoints.
