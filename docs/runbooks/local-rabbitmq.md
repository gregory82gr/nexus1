# Runbook: running RabbitMQ locally for development

This machine has no admin rights available to Claude Code sessions, so
RabbitMQ is installed as **portable binaries**, not as a Windows service —
started as a regular background process, stopped by killing that process.
Reproduce this setup on another machine with the same constraint, or adapt
step 2 to a normal service install if admin rights are available there.

## Why Erlang OTP 27, not the latest release

RabbitMQ 4.3.4 fails to boot under Erlang/OTP 29.0.5 (installed via
`winget install Erlang.ErlangOTP`) with:

```
BOOT FAILED
{incompatible_feature_flags,{horus,extraction_denied,
  #{error => {unknown_instruction,{line,[{location,[],1690}]}}}}}
```

OTP 29 was too new for this RabbitMQ release at the time of writing. Erlang
OTP **27.3.4.16** (portable zip, not the winget package) works. Both
Erlang installs coexist on this machine; RabbitMQ is pointed at OTP 27
specifically via `ERLANG_HOME`.

## Install locations

- Erlang OTP 27 (portable): `%LOCALAPPDATA%\erlang-otp27\`
- Erlang OTP 29 (winget, unused by RabbitMQ): `C:\Program Files\Erlang OTP\`
  — left in place, harmless, just not what RabbitMQ is pointed at.
- RabbitMQ 4.3.4 (portable): `%LOCALAPPDATA%\rabbitmq\rabbitmq_server-4.3.4\`
- RabbitMQ data/logs: `%LOCALAPPDATA%\rabbitmq\data\`

## Persistent environment (already set, user-level)

- `ERLANG_HOME` = `%LOCALAPPDATA%\erlang-otp27`
- `RABBITMQ_BASE` = `%LOCALAPPDATA%\rabbitmq\data`

These are set at the Windows user level (`[System.Environment]::
SetEnvironmentVariable(..., "User")`), so they persist across sessions once
set, but a **new shell must be opened** (or re-set them for the current
process) to pick them up — PowerShell/Bash tool calls in this harness don't
inherit env vars set in a previous call.

## Start

```powershell
$env:ERLANG_HOME = "$env:LOCALAPPDATA\erlang-otp27"
$env:RABBITMQ_BASE = "$env:LOCALAPPDATA\rabbitmq\data"
& "$env:LOCALAPPDATA\rabbitmq\rabbitmq_server-4.3.4\sbin\rabbitmq-server.bat"
```

Run this as a background process (it blocks in the foreground otherwise).
Takes several seconds to boot; poll with `rabbitmqctl status` (below) rather
than assuming it's ready immediately.

## Check status

```powershell
$env:ERLANG_HOME = "$env:LOCALAPPDATA\erlang-otp27"
$env:RABBITMQ_BASE = "$env:LOCALAPPDATA\rabbitmq\data"
& "$env:LOCALAPPDATA\rabbitmq\rabbitmq_server-4.3.4\sbin\rabbitmqctl.bat" status
```

Exit code 0 and a populated status block (node name, listeners, memory)
means it's up. AMQP listens on `5672` (both `[::]` and `0.0.0.0`).

## Stop

Find the OS PID from `rabbitmqctl status`'s "Runtime" section (or via
`netstat -ano` on port 5672) and `taskkill /F /PID <pid>`. There is no
Windows service to stop via `services.msc` — this is a plain process.

## Management UI / HTTP API

Enabled (`rabbitmq-plugins.bat enable rabbitmq_management`):

- UI: http://localhost:15672 — default credentials `guest`/`guest` (RabbitMQ
  restricts the `guest` user to loopback connections by default, which is
  fine since everything here is local).
- HTTP API: http://localhost:15672/api/... , same credentials, e.g.
  `GET /api/overview`, `GET /api/exchanges`, `GET /api/queues` — useful for
  proving topology (exchanges/queues/bindings) exists as evidence, rather
  than relying only on `rabbitmqctl` text output.

## Connection string for the .NET client (once wired, §5 step 7)

`amqp://guest:guest@localhost:5672/` — default vhost `/`, default
credentials. Revisit before anything beyond local development touches this
broker (real credentials, non-default vhost, TLS).
