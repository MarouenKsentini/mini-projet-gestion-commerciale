# AGENTS.md

## Stack
- .NET 8 layered API (Domain / Application / Infrastructure / Api) + EF Core SQL Server. Angular 18 standalone (no NgModules). No `opencode.json`, no CI (`.github/workflows/` empty), no lint/pre-commit; frontend tests are Karma+Jasmine boilerplate only.

## Layout
- `backend/GestionCommerciale.sln` — 4 projects in `backend/src/`: `Domain` (entities/enums/exceptions), `Application` (DTOs/interfaces/services), `Infrastructure` (`Persistence/AppDbContext.cs:7`, `Persistence/DependencyInjection.cs:11`), `Api` (`Program.cs:4`, `Middleware/ExceptionHandlingMiddleware.cs:11`, `Controllers/`).
- `backend/database/schema.sql:1` — fallback DDL when `dotnet ef` cannot run. Initial migration already committed at `Infrastructure/Migrations/20260905132612_InitialCreate.cs`.
- `frontend/gestion-commerciale-app/` — Angular app. Routing `src/app/app.routes.ts:3` (French paths `clients`/`produits`/`commandes`, lazy `loadComponent`). Config `src/app/app.config.ts:8` wires `errorInterceptor`. API URL `src/environments/environment.ts:3` (`https://localhost:5001/api`, prod `/api`).

## Backend — Run & DB
```bash
cd backend/src/GestionCommerciale.Api
dotnet restore
dotnet tool install --global dotnet-ef   # if missing
dotnet ef database update -p ../GestionCommerciale.Infrastructure -s .  # InitialCreate already exists; only update on first run
# new migration: dotnet ef migrations add <Name> -p ../GestionCommerciale.Infrastructure -s .
dotnet run  # Swagger always at Program.cs:42; check launchSettings.json:9 for actual port
```
- Connection string `appsettings.json:10` = `(localdb)\mssqllocaldb` / `GestionCommercialeDb`; change `Server=` for full SQL Server.
- Port gotcha: `Properties/launchSettings.json:9` is `https://localhost:30095` but `environment.ts:3` expects `https://localhost:5001`. Keep `environment.ts` + `Cors:AllowedOrigin` (`appsettings.json:13`, read at `Program.cs:28`, default `http://localhost:4200`) in sync or browser blocks calls.
- DI: all registrations via `AddInfrastructure()` in `DependencyInjection.cs:11` (`AppDbContext`, `IAppDbContext`, `IClientService`, `IProductService`, `IOrderService`). Don't register elsewhere.
- If `dotnet ef` fails, execute `backend/database/schema.sql` manually on SQL Server.

## Frontend — Run
```bash
cd frontend/gestion-commerciale-app
npm install
npm start   # ng serve -> http://localhost:4200
npm test    # ng test (Karma, largely untested — needs Chrome)
ng build    # dist/gestion-commerciale-app (angular.json:23)
```
- Services `src/app/core/services/{client,product,order}.service.ts` use `environment.apiUrl`; `core/interceptors/error.interceptor.ts:11` surfaces `error.error.message` via `ToastService`.

## Gotchas
- Controllers thin — all business rules in `Application/Services/` (`OrderService.cs:10` grading-critical): `TotalHT = sum(Qty*PrixUnitaire)`, `TotalTTC = round(TotalHT*1.19,2)` (`OrderService.cs:152`), `TauxTVA = 0.19m`. `UpdateAsync`/`DeleteAsync` reject non-`Brouillon`; `ValidateAsync` re-checks stock then decrements `Product.QuantiteStock` (`OrderService.cs:114`) and sets `Validee`. `DeleteAsync` only blocks `Validee` (allows `Annulee` per `OrderStatus.cs:6`).
- `BusinessException -> 400`, `NotFoundException -> 404` via `ExceptionHandlingMiddleware.cs:28`; don't add per-controller try/catch. `Program.cs:7` `SuppressModelStateInvalidFilter = false` gives auto 400 for DTO validation (e.g. `[Required]`/`[Range]` in `OrderDtos.cs:30`).
- `AppDbContext.cs:55` `Ignore(l => l.TotalLigne)` — computed in-memory (`OrderLine.cs:16` / `OrderService.cs:178`), not a column. `OrderLine->Product` is `WithMany()` no navigation; `Order->Client` is `Restrict`, `OrderLine->Order` is `Cascade` (`AppDbContext.cs:49,60`).
- `Product.Reference` and `Order.NumeroCommande` have unique indexes (`AppDbContext.cs:32,41`); `NumeroCommande` = `CMD-yyyyMMddHHmmssfff` (`OrderService.cs:166`).
- Middleware order `Program.cs:40`: `ExceptionHandling -> Swagger -> HttpsRedirection -> Cors("AllowAngularApp") -> Authorization -> MapControllers`.

## Conventions
- DTOs never expose entities (`Application/DTOs/` → `frontend/src/app/core/models/` mirrors them). Order form uses `FormArray`; authoritative totals from backend (frontend live preview uses same 19% TVA).
- Angular `scss` default (`angular.json:13`), `strict` TS (`tsconfig.json:7`), `nullable` enabled (`GestionCommerciale.Api.csproj:6`). Auth/JWT not implemented — leave `[Authorize]` out unless adding full flow.
