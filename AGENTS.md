# AGENTS.md

## Stack
- .NET 8 layered API (`backend/GestionCommerciale.sln`: Domain / Application / Infrastructure / Api) + EF Core. Angular 18 standalone (no NgModules) in `frontend/gestion-commerciale-app/`. No `opencode.json`, no CI (`.github/workflows/` empty), no lint/pre-commit; no backend tests, frontend `npm test` is Karma+Jasmine boilerplate (needs Chrome).

## Backend — run & DB
```bash
cd backend/src/GestionCommerciale.Api
dotnet restore && dotnet run   # Swagger at /swagger; launchSettings.json:9 = https://localhost:5001
# new migration only when model changes:
dotnet ef migrations add <Name> -p ../GestionCommerciale.Infrastructure -s .
```
- No manual `database update` on first run: `DbSeederHostedService` (`Infrastructure/Persistence/DbSeeder.cs:42`) auto-runs `MigrateAsync` + seeds demo clients/products/order on startup. `InitialCreate` migration already committed.
- Dev connection string that actually applies is `appsettings.Development.json:9` (`Server=DESKTOP-7VIA6NI\MAROUEN`, machine-specific), NOT `appsettings.json:10` (LocalDB fallback). Check the Development file first when DB is unreachable.
- DB-down behavior: seeder catches failure and API still starts (Swagger reachable); "error 26" = named instance/Browser stopped. Diagnose with `python tools/windows_service_manager.py ensure-sql --instance MAROUEN --no-auto-start --json` (see `tools/README.md`); starting services needs Admin terminal.
- No SQL Server available? Use a `Data Source=...` connection string without `Server=`: `Infrastructure/Persistence/DependencyInjection.cs:18` auto-switches to SQLite (`EnsureCreated` path). `backend/database/schema.sql` + `seed.sql` are manual fallbacks only.

## Frontend — run
```bash
cd frontend/gestion-commerciale-app
npm install && npm start   # http://localhost:4200
```
- API URL: `src/environments/environment.ts:3` (`https://localhost:5001/api`, matches launchSettings) vs `environment.prod.ts` (`/api`). If backend port changes, update it AND `Cors:AllowedOrigin` (`appsettings.json:13`, read at `Program.cs:28`) together or browser blocks calls.
- Routing `src/app/app.routes.ts:3`: French paths `clients`/`produits`/`commandes`, all lazy `loadComponent`, default + wildcard redirect to `commandes`. HTTP wired once in `src/app/app.config.ts:8` (`errorInterceptor` surfaces backend `error.error.message` via toast — `core/interceptors/error.interceptor.ts:16`).
- Deps must stay on one Angular major (18.x): partial `npm i @angular/...@latest` mixes majors and breaks install/build with ERESOLVE — restore via `git checkout -- package.json package-lock.json && npm ci`.

## Rules that bite
- All business logic lives in `Application/Services/` (`OrderService.cs:10`); controllers are thin, never add per-controller try/catch — `ExceptionHandlingMiddleware.cs:28` maps `BusinessException -> 400`, `NotFoundException -> 404` (`{ message }` JSON). DTO `[Required]`/`[Range]` auto-400s via `Program.cs:12` (`SuppressModelStateInvalidFilter = false`).
- Order lifecycle (`OrderService.cs`): only `Brouillon` can be updated, validated or cancelled (`UpdateAsync:61`, `ValidateAsync:99`, `CancelAsync` → `Annulee`); `DeleteAsync:87` blocks only `Validee`. Endpoints: `POST api/orders/{id}/validate`, `POST api/orders/{id}/cancel`.
- Stock + pricing: line `PrixUnitaire` is snapshotted from `Product.PrixUnitaireHT` (client cannot set price); stock checked at create (`BuildLinesAsync:126`) AND re-checked at validate before decrement (`ValidateAsync:107-115`). `TauxTVA = 0.19m`, `TotalHT = sum(Qty*Prix)`, `TotalTTC = round(TotalHT*1.19,2)` (`OrderService.cs:150-153`); frontend preview must use same 19% (`order-form.component.ts:13`).
- EF mapping (`AppDbContext.cs`): `OrderLine.TotalLigne` is `Ignore`d (computed in-memory `OrderLine.cs:16`, not a column); `Order->Client` and `OrderLine->Product` are `Restrict`, `OrderLine->Order` is `Cascade`. Unique indexes on `Product.Reference` and `Order.NumeroCommande` (`CMD-yyyyMMddHHmmssfff`, `OrderService.cs:165`).
- DI: everything via `AddInfrastructure()` in `Infrastructure/Persistence/DependencyInjection.cs:11` — don't register services elsewhere. No auth/JWT: leave `[Authorize]` out unless adding the full flow.
