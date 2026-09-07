"""
Windows Service Manager — agent tool for inspecting/monitoring/managing Windows services.

Designed for SQL Server instances like SQL Server (MAROUEN) seen in services.msc,
but generic for any Windows service.

Uses PowerShell Get-Service/Start-Service/Stop-Service/Restart-Service via subprocess
(primary, always available on Windows). Falls back to `sc.exe` and optionally `psutil`
if installed.

Usage as script:
  python tools/windows_service_manager.py list --filter SQL
  python tools/windows_service_manager.py status --name "SQL Server (MAROUEN)"
  python tools/windows_service_manager.py start --name "SQL Server (MAROUEN)"
  python tools/windows_service_manager.py ensure-sql --instance MAROUEN

Usage as module:
  from tools.windows_service_manager import get_service_status, ensure_sql_server_running
"""

from __future__ import annotations

import argparse
import json
import platform
import re
import subprocess
import sys
from dataclasses import asdict, dataclass
from typing import List, Optional

IS_WINDOWS = platform.system() == "Windows"


@dataclass
class ServiceInfo:
    name: str
    display_name: str
    status: str  # Running, Stopped, Paused, etc.
    start_type: Optional[str] = None  # Automatic, Manual, Disabled


def _run_powershell(cmd: str, timeout: int = 15) -> str:
    """Run a PowerShell command and return stdout. Raises on error."""
    # Use powershell.exe (Windows PowerShell 5) or pwsh (PowerShell 7) — try both
    for exe in ("powershell", "pwsh"):
        try:
            result = subprocess.run(
                [exe, "-NoProfile", "-NonInteractive", "-Command", cmd],
                capture_output=True,
                text=True,
                timeout=timeout,
            )
            if result.returncode == 0:
                return result.stdout.strip()
            # If command produced error but exe exists, surface stderr
            if result.stderr:
                raise RuntimeError(result.stderr.strip())
            raise RuntimeError(f"{exe} exited {result.returncode}: {result.stdout}")
        except FileNotFoundError:
            continue
    raise RuntimeError("No PowerShell executable found (tried powershell, pwsh)")


def _run_sc(query: str, timeout: int = 10) -> str:
    result = subprocess.run(
        ["sc", "query", query],
        capture_output=True,
        text=True,
        timeout=timeout,
    )
    return result.stdout.strip()


# ---------------------------------------------------------------------------
# Core helpers
# ---------------------------------------------------------------------------

def list_services(name_filter: Optional[str] = None) -> List[ServiceInfo]:
    """
    List Windows services. Optionally filter by substring (case-insensitive)
    on Name or DisplayName.

    Example: list_services("SQL") returns SQL Server (MAROUEN), SQL Server Browser, etc.
    """
    if not IS_WINDOWS:
        raise OSError("list_services is Windows-only (requires PowerShell)")

    # PowerShell: Get-Service | Select Name, DisplayName, Status
    # Add StartType via Get-CimInstance Win32_Service for richer info
    ps = r"""
    $filter = $env:SVC_FILTER
    $svcs = Get-Service | Where-Object { -not $filter -or $_.Name -like "*$filter*" -or $_.DisplayName -like "*$filter*" }
    $cim = @{}
    try { Get-CimInstance Win32_Service | ForEach-Object { $cim[$_.Name] = $_.StartMode } } catch {}
    $svcs | Select-Object Name, DisplayName, @{N='Status';E={ $_.Status.ToString() }}, @{N='StartType';E={ $cim[$_.Name] }} | ConvertTo-Json -Depth 2
    """
    env = None
    if name_filter:
        import os

        env = {**__import__("os").environ, "SVC_FILTER": name_filter}

    # Inline env handling — set via powershell $env:
    if name_filter:
        ps = f"$env:SVC_FILTER='{name_filter}'; " + ps

    out = _run_powershell(ps)
    if not out:
        return []
    try:
        data = json.loads(out)
    except json.JSONDecodeError:
        # Single object case: PowerShell returns object not array
        data = json.loads(f"[{out}]") if out.strip().startswith("{") else []
    if isinstance(data, dict):
        data = [data]
    services = []
    for row in data:
        if not row or not row.get("Name"):
            continue
        services.append(
            ServiceInfo(
                name=row["Name"],
                display_name=row.get("DisplayName", ""),
                status=str(row.get("Status", "")),
                start_type=row.get("StartType"),
            )
        )
    return services


def get_service_status(service_name: str) -> ServiceInfo:
    """
    Get status for a single service by Name or DisplayName.

    Raises RuntimeError if not found. DisplayName with parentheses (e.g. "SQL Server (MAROUEN)")
    is resolved via Get-Service -DisplayName.
    """
    if not IS_WINDOWS:
        raise OSError("get_service_status is Windows-only")

    # Try Name first, then DisplayName. PowerShell handles both via two attempts.
    ps = r"""
    param($n)
    $svc = $null
    try { $svc = Get-Service -Name $n -ErrorAction Stop } catch {}
    if (-not $svc) {
        try { $svc = Get-Service -DisplayName $n -ErrorAction Stop } catch {}
    }
    if (-not $svc) { Write-Error "Service not found: $n"; exit 1 }
    $startType = $null
    try { $startType = (Get-CimInstance Win32_Service -Filter "Name='$($svc.Name)'").StartMode } catch {}
    @{ Name=$svc.Name; DisplayName=$svc.DisplayName; Status=$svc.Status.ToString(); StartType=$startType } | ConvertTo-Json
    """
    # Escape single quotes for PS string
    safe = service_name.replace("'", "''")
    out = _run_powershell(f"& {{ {ps} }} '{safe}'")
    info = json.loads(out)
    return ServiceInfo(
        name=info["Name"],
        display_name=info["DisplayName"],
        status=info["Status"],
        start_type=info.get("StartType"),
    )


def _ensure_startable(service_name: str):
    """If StartType is Disabled, set to Automatic (requires Admin)."""
    ps = r"""
    param($n)
    $cim = Get-CimInstance Win32_Service -Filter "Name='$n'" -ErrorAction SilentlyContinue
    if ($cim -and $cim.StartMode -eq 'Disabled') {
        try { Set-Service -Name $n -StartupType Automatic -ErrorAction Stop; Write-Output "Enabled" }
        catch { sc.exe config $n start= auto | Out-Null; Write-Output "EnabledViaSc" }
    } else { Write-Output "AlreadyEnabled" }
    """
    safe = service_name.replace("'", "''")
    try:
        return _run_powershell(f"& {{ {ps} }} '{safe}'")
    except Exception:
        return ""


def _service_action(service_name: str, action: str) -> str:
    """Start/Stop/Restart a service. Requires Admin. Returns PowerShell stdout."""
    if action not in ("Start", "Stop", "Restart"):
        raise ValueError(action)
    ps = r"""
    param($n, $a)
    $svc = $null
    try { $svc = Get-Service -Name $n -ErrorAction Stop } catch {}
    if (-not $svc) { try { $svc = Get-Service -DisplayName $n -ErrorAction Stop } catch {} }
    if (-not $svc) { Write-Error "Service not found: $n"; exit 1 }
    # If Disabled, enable first (Start would fail)
    if ($a -eq 'Start' -or $a -eq 'Restart') {
        $cim = Get-CimInstance Win32_Service -Filter "Name='$($svc.Name)'" -ErrorAction SilentlyContinue
        if ($cim -and $cim.StartMode -eq 'Disabled') {
            try { Set-Service -Name $svc.Name -StartupType Automatic } catch { sc.exe config $svc.Name start= auto | Out-Null }
            Start-Sleep -Seconds 1
        }
    }
    if ($a -eq 'Start') { Start-Service -InputObject $svc; $svc.WaitForStatus('Running','00:00:30') }
    elseif ($a -eq 'Stop') { Stop-Service -InputObject $svc -Force; $svc.WaitForStatus('Stopped','00:00:30') }
    else { Restart-Service -InputObject $svc -Force; $svc.WaitForStatus('Running','00:00:30') }
    # Re-query for final status
    $final = Get-Service -Name $svc.Name
    @{ Name=$final.Name; DisplayName=$final.DisplayName; Status=$final.Status.ToString() } | ConvertTo-Json
    """
    safe = service_name.replace("'", "''")
    return _run_powershell(f"& {{ {ps} }} '{safe}' '{action}'", timeout=35)


def start_service(service_name: str) -> ServiceInfo:
    out = _service_action(service_name, "Start")
    info = json.loads(out)
    return ServiceInfo(info["Name"], info["DisplayName"], info["Status"])


def stop_service(service_name: str) -> ServiceInfo:
    out = _service_action(service_name, "Stop")
    info = json.loads(out)
    return ServiceInfo(info["Name"], info["DisplayName"], info["Status"])


def restart_service(service_name: str) -> ServiceInfo:
    out = _service_action(service_name, "Restart")
    info = json.loads(out)
    return ServiceInfo(info["Name"], info["DisplayName"], info["Status"])


def is_admin() -> bool:
    """Check if current process is elevated (Admin)."""
    if not IS_WINDOWS:
        return False
    try:
        import ctypes

        return ctypes.windll.shell32.IsUserAnAdmin() != 0
    except Exception:
        # Fallback: try a privileged PS call
        try:
            _run_powershell("[Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator) | Out-String")
            return "True" in _run_powershell(
                "(New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)"
            )
        except Exception:
            return False


# ---------------------------------------------------------------------------
# SQL Server specific helpers
# ---------------------------------------------------------------------------

SQL_SERVICE_PATTERNS = [
    "MSSQL${instance}",  # default service name for SQL Server, e.g. MSSQL$MAROUEN or MSSQLSERVER
    "SQL Server ({instance})",  # display name
    "SQLBrowser",  # SQL Server Browser
    "SQLAgent${instance}",  # SQL Agent
]


def _sql_service_names(instance: str = "MAROUEN") -> List[str]:
    inst_upper = instance.upper()
    names = []
    if inst_upper == "MSSQLSERVER":
        names.append("MSSQLSERVER")
    else:
        names.append(f"MSSQL${instance}")
        names.append(f"MSSQL${inst_upper}")
    names.append(f"SQL Server ({instance})")
    names.append(f"SQL Server ({inst_upper})")
    return names


def ensure_sql_server_running(instance: str = "MAROUEN", include_browser: bool = True, auto_start: bool = True) -> dict:
    """
    Inspect SQL Server instance + Browser. Optionally auto-start if stopped.

    Returns dict with keys:
      instance, instance_status, browser_status, admin, actions_taken, errors
    """
    result: dict = {
        "instance": instance,
        "instance_status": None,
        "browser_status": None,
        "admin": is_admin(),
        "actions_taken": [],
        "errors": [],
    }

    # 1. Instance
    instance_info = None
    for candidate in _sql_service_names(instance):
        try:
            instance_info = get_service_status(candidate)
            result["instance_status"] = asdict(instance_info)
            break
        except Exception:
            continue
    if not instance_info:
        # Fallback: search any SQL-related service containing instance name
        try:
            found = [s for s in list_services(instance) if "sql" in s.name.lower() or "sql" in s.display_name.lower()]
            if found:
                instance_info = found[0]
                result["instance_status"] = asdict(instance_info)
            else:
                result["errors"].append(f"SQL Server instance '{instance}' not found (tried {', '.join(_sql_service_names(instance))})")
        except Exception as e:
            result["errors"].append(str(e))

    if instance_info and instance_info.status != "Running" and auto_start:
        if not result["admin"]:
            result["errors"].append(f"Instance is {instance_info.status} but not Admin — cannot Start. Re-run terminal as Administrator.")
        else:
            try:
                after = start_service(instance_info.name)
                result["actions_taken"].append(f"Started {after.display_name} -> {after.status}")
                result["instance_status"] = asdict(after)
            except Exception as e:
                result["errors"].append(f"Failed to start instance: {e}")

    # 2. Browser (required for named instances like MAROUEN via SQL Network Interfaces error 26)
    if include_browser:
        try:
            browser = get_service_status("SQLBrowser")
            result["browser_status"] = asdict(browser)
            if browser.status != "Running" and auto_start:
                if not result["admin"]:
                    result["errors"].append("SQL Server Browser is not Running — needs Admin to start (required for named instances).")
                else:
                    try:
                        after_b = start_service(browser.name)
                        result["actions_taken"].append(f"Started {after_b.display_name} -> {after_b.status}")
                        result["browser_status"] = asdict(after_b)
                    except Exception as e:
                        result["errors"].append(f"Failed to start Browser: {e}")
        except Exception as e:
            result["errors"].append(f"Browser check failed: {e}")

    return result


# ---------------------------------------------------------------------------
# Optional psutil backend (cross-check, no admin actions)
# ---------------------------------------------------------------------------

def list_services_psutil(name_filter: Optional[str] = None) -> List[ServiceInfo]:
    """Alternative backend using psutil (if installed). No admin actions, just listing."""
    try:
        import psutil
    except ImportError:
        raise RuntimeError("psutil not installed — pip install psutil")

    out = []
    for svc in psutil.win_service_iter():
        try:
            # psutil Service: name(), display_name(), status()
            name = svc.name()
            disp = svc.display_name()
            status = svc.status()
            if name_filter and name_filter.lower() not in name.lower() and name_filter.lower() not in disp.lower():
                continue
            out.append(ServiceInfo(name=name, display_name=disp, status=status))
        except Exception:
            continue
    return out


# ---------------------------------------------------------------------------
# CLI
# ---------------------------------------------------------------------------

def _print_services(services: List[ServiceInfo]):
    if not services:
        print("No services found.")
        return
    # Simple table
    w1 = max(len(s.name) for s in services)
    w2 = max(len(s.display_name) for s in services)
    print(f"{'Name'.ljust(w1)}  {'DisplayName'.ljust(w2)}  Status      StartType")
    print("-" * (w1 + w2 + 30))
    for s in services:
        print(f"{s.name.ljust(w1)}  {s.display_name.ljust(w2)}  {s.status.ljust(10)}  {s.start_type or ''}")


def main():
    p = argparse.ArgumentParser(description="Windows Service Manager — inspect/manage services (e.g. SQL Server (MAROUEN))")
    sub = p.add_subparsers(dest="cmd", required=True)

    sp_list = sub.add_parser("list", help="List services")
    sp_list.add_argument("--filter", default=None, help="Substring filter on Name/DisplayName, e.g. SQL")
    sp_list.add_argument("--json", action="store_true")
    sp_list.add_argument("--psutil", action="store_true", help="Use psutil backend instead of PowerShell")

    sp_status = sub.add_parser("status", help="Get status for one service")
    sp_status.add_argument("--name", required=True, help="Name or DisplayName, e.g. 'SQL Server (MAROUEN)' or MSSQL$MAROUEN")

    for cmd in ("start", "stop", "restart"):
        sp = sub.add_parser(cmd, help=f"{cmd.capitalize()} a service (requires Admin)")
        sp.add_argument("--name", required=True)

    sp_sql = sub.add_parser("ensure-sql", help="Ensure SQL Server instance + Browser are Running")
    sp_sql.add_argument("--instance", default="MAROUEN")
    sp_sql.add_argument("--no-browser", action="store_true")
    sp_sql.add_argument("--no-auto-start", action="store_true", help="Only inspect, don't start")
    sp_sql.add_argument("--json", action="store_true")

    sp_admin = sub.add_parser("is-admin", help="Check if running as Admin")

    args = p.parse_args()

    try:
        if args.cmd == "list":
            svcs = list_services_psutil(args.filter) if args.psutil else list_services(args.filter)
            if args.json:
                print(json.dumps([asdict(s) for s in svcs], indent=2, ensure_ascii=False))
            else:
                _print_services(svcs)
        elif args.cmd == "status":
            info = get_service_status(args.name)
            print(json.dumps(asdict(info), indent=2, ensure_ascii=False))
        elif args.cmd == "start":
            info = start_service(args.name)
            print(json.dumps(asdict(info), indent=2, ensure_ascii=False))
        elif args.cmd == "stop":
            info = stop_service(args.name)
            print(json.dumps(asdict(info), indent=2, ensure_ascii=False))
        elif args.cmd == "restart":
            info = restart_service(args.name)
            print(json.dumps(asdict(info), indent=2, ensure_ascii=False))
        elif args.cmd == "ensure-sql":
            res = ensure_sql_server_running(
                instance=args.instance,
                include_browser=not args.no_browser,
                auto_start=not args.no_auto_start,
            )
            if args.json:
                print(json.dumps(res, indent=2, ensure_ascii=False))
            else:
                print(json.dumps(res, indent=2, ensure_ascii=False))
                if res["errors"]:
                    print("\nErrors:", file=sys.stderr)
                    for e in res["errors"]:
                        print(f"  - {e}", file=sys.stderr)
        elif args.cmd == "is-admin":
            print(json.dumps({"admin": is_admin()}, indent=2))
    except Exception as e:
        print(f"Error: {e}", file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    main()
