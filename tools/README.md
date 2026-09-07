# Windows Service Manager

Agent tool for `services.msc` — inspect, monitor, manage Windows services like `SQL Server (MAROUEN)`.

Built for the screenshot you sent (context menu on `SQL Server (MAROUEN)`). The agent can now check why `DESKTOP-7VIA6NI\MAROUEN` was unreachable (`error 26`) and fix it programmatically.

## Why this exists

`DESKTOP-7VIA6NI\MAROUEN` failed with `Error Locating Server/Instance Specified (26)` because on your machine:
```
MSSQL$MAROUEN  -> Stopped / Disabled
SQLBrowser     -> Stopped / Disabled
```
Named instances require `SQL Server Browser` running, and a `Disabled` service cannot be `Start`ed until its StartType is set to `Automatic`.

This tool handles that, and requires **Admin** for any Start/Stop/Restart.

## Quick start

```bash
# list all SQL services (status now correctly shows Running/Stopped)
python tools/windows_service_manager.py list --filter SQL

# check one
python tools/windows_service_manager.py status --name "SQL Server (MAROUEN)"
python tools/windows_service_manager.py status --name MSSQL$MAROUEN

# are you Admin? (needed to start)
python tools/windows_service_manager.py is-admin

# inspect MAROUEN without changing (dry-run)
python tools/windows_service_manager.py ensure-sql --instance MAROUEN --no-auto-start --json

# fix: enable Disabled -> Automatic and start instance + Browser (run terminal as Administrator)
python tools/windows_service_manager.py ensure-sql --instance MAROUEN --json
```

## How it works

- Primary backend: **PowerShell** `Get-Service` / `Set-Service` / `Start-Service` / `Get-CimInstance Win32_Service` via `subprocess` (no extra deps).
- Fallback: `sc.exe config` if `Set-Service` is blocked; `psutil` optional for listing: `python tools/windows_service_manager.py list --filter SQL --psutil` (needs `pip install psutil` — already installed here: `psutil 7.0.0`).

## As a Python module

```python
from tools.windows_service_manager import (
    list_services,
    get_service_status,
    ensure_sql_server_running,
    is_admin,
)

# inspect
for svc in list_services("SQL"):
    print(svc.name, svc.status, svc.start_type)

# one-shot fix for gestion-commerciale DB
result = ensure_sql_server_running("MAROUEN", include_browser=True, auto_start=True)
print(result)  # {instance_status, browser_status, admin, actions_taken, errors}
# if not admin, result["errors"] tells you to re-run as Administrator
```

## Integration in gestion-commerciale

`backend/src/GestionCommerciale.Api/appsettings.Development.json` points at `Server=DESKTOP-7VIA6NI\MAROUEN`. After `ensure-sql --instance MAROUEN` reports both `Running`, restart the API:

```bash
cd backend/src/GestionCommerciale.Api
dotnet run   # now logs "Database migrated and seeded." instead of error 26
```

See `backend/database/seed.sql` for manual seeding if you prefer SSMS.
