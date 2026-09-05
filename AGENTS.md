# AGENTS.md

## Stack
- .NET 8 layered API (Domain / Application / Infrastructure / Api) + EF Core SQL Server. Angular 18 standalone components (no NgModules). No tests, CI, lint, or `opencode.json` currently.

## Repo Layout
- `backend/GestionCommerciale.sln` — 4 projects under `backend/src/`: `GestionCommerciale.Domain` (entities/enums/exceptions), `GestionCommerciale.Application` (DTOs/interfaces/services), `GestionCommerciale.Infrastructure` ( `Persistence/AppDbContext.cs:7`, `Persistence/DependencyInjection.cs:11` ), `GestionCommerciale.Api` (controllers, `Program.cs:4`, `Middleware/ExceptionHandlingMiddleware.cs:11`).
- `backend/database/schema.sql:1` — fallback DDL if `dotnet ef` fails. `backend/.gitignore` + `frontend/gestion-commerciale-app/.gitignore` only — no root `.gitignore`.
- `frontend/gestion-commerciale-app/` — Angular app root. Routing in `src/app/app.routes.ts:3` (French paths, lazy `loadComponent`). App config `src/app/app.config.ts:8` wires `errorInterceptor`. API URL in `src/environments/environment.ts:3` (dev `https://localhost:5001/api`, `environment.prod.ts:3` is `/api`).

## Backend — Run & DB
```bash
cd backend/src/GestionCommerciale.Api
dotnet restore
dotnet tool install --global dotnet-ef   # if missing
dotnet ef database update -p ../GestionCommerciale.Infrastructure -s .  # create on first run; to create new migration: dotnet ef migrations add <Name> -p ../GestionCommerciale.Infrastructure -s .
dotnet run  # Swagger at https://localhost:5001/swagger (always enabled in Program.cs:42)
```
- Connection string: `backend/src/GestionCommerciale.Api/appsettings.json:10` defaults to `Server=(localdb)\mssqllocaldb;Database=GestionCommercialeDb;...`. Change `Server=` for full SQL Server/SQL Express.
- CORS: `Program.cs:28` reads `Cors:AllowedOrigin` (default `http://localhost:4200`). Must match frontend origin or browser blocks calls.
- DI: everything registered via `AddInfrastructure()` in `DependencyInjection.cs:11` (`AppDbContext`, `IAppDbContext`, `IClientService`, `IProductService`, `IOrderService`). Don't add services elsewhere.
- No `Migrations/` folder committed — expected; generate via EF as above or run `backend/database/schema.sql` manually.

## Frontend — Run
```bash
cd frontend/gestion-commerciale-app
npm install
npm start   # == ng serve -> http://localhost:4200
npm test    # karma+jasmine (tsconfig.spec.json), largely untested
ng build    # output dist/gestion-commerciale-app (angular.json:23)
```
- If backend port differs from `https://localhost:5001`, update `src/environments/environment.ts:3` and `appsettings.json` CORS together.
- Services `src/app/core/services/{client,product,order}.service.ts` consume `environment.apiUrl`; `core/interceptors/error.interceptor.ts:11` displays `error.error.message` from the API via `ToastService`.

## Architecture Gotchas
- Keep controllers thin — all business rules live in `Application/Services/` (`OrderService.cs:10` is grading-critical): totals `TotalHT = sum(Qty*Prix)`, `TotalTTC = round(TotalHT*1.19,2)` (`OrderService.cs:152`), TVA 19% (`TauxTVA = 0.19m`). `UpdateAsync`/`DeleteAsync` reject non-`Brouillon` orders; `ValidateAsync` re-checks stock then decrements `Product.QuantiteStock` (`OrderService.cs:114`) and sets `Validee`.
- `BusinessException -> 400`, `NotFoundException -> 404` via `ExceptionHandlingMiddleware.cs:28`; front `errorInterceptor` surfaces `message`. Don't add per-controller try/catch.
- `AppDbContext.cs:55` `Ignore(l => l.TotalLigne)` — computed in-memory (`OrderLine.cs` / `OrderService.cs:178`), not a persisted column despite `schema.sql:42` defining it as persisted. Same divergence: `OrderLine`->`Product` is `WithMany()` with no navigation, `Order`-`Client` is `Restrict` delete.
- `Product.Reference` and `Order.NumeroCommande` have unique indexes (`AppDbContext.cs:32,41`); `Order.NumeroCommande` generated as `CMD-yyyyMMddHHmmssfff` (`OrderService.cs:166`).
- Swagger, `AddControllers` validation (`Program.cs:7`), and `UseCors("AllowAngularApp")` order matters — middleware pipeline is `ExceptionHandling -> Swagger -> HttpsRedirection -> Cors -> Authorization` (`Program.cs:40`).

## Conventions
- DTOs never expose entities directly (`Application/DTOs/`). Frontend models mirror them in `src/app/core/models/`.
- Order form uses `FormArray` for lines; totals displayed live on front but authoritative totals come from backend.
- Angular style `scss` default (`angular.json:13`), `strict` TS, nullable enabled in `GestionCommerciale.Api.csproj:6`. Auth/JWT not implemented (bonus only) — leave `[Authorize]` out unless adding the full flow.
