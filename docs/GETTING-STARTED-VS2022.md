# Visual Studio 2022 — Getting Started

## Open the solution

1. **File → Open → Project/Solution**
2. Select `IndustrialPress.sln` at the repository root.

Solution Explorer shows solution folders: **docs**, **contracts**, **services** (rest-api, sql-data, iot-telemetry).

## Configure multiple startup projects

For local debugging of all backends:

1. Right-click the **solution** → **Properties**
2. **Startup Project** → **Multiple startup projects**
3. Set **RestApi**, **SqlData**, and **IotTelemetry** to **Start**
4. Apply

## Default ports (Phase 0 placeholders)

| Service | HTTP | Notes |
|---------|------|--------|
| RestApi | https://localhost:7101, http://localhost:5101 | `launchSettings.json` |
| SqlData (gRPC) | https://localhost:7102 | gRPC + health |
| IotTelemetry | http://localhost:5103 | Worker / health |

Frontend (Vite): `http://localhost:5173` — proxy to RestApi in Phase 1.

## Build & test

- **Build:** `Ctrl+Shift+B` or **Build → Build Solution**
- **Test:** **Test → Test Explorer** → Run All

CLI:

```powershell
dotnet build IndustrialPress.sln
dotnet test IndustrialPress.sln
```

## Recommended VS workloads

- ASP.NET and web development
- .NET desktop development (for worker/console debugging)

## Architecture reference

All diagrams, retry rules (R1–R7), failure matrix, scaling, and CQRS-lite notes:

→ [`architecture.md`](architecture.md)

## Mermaid diagrams in VS

Install extension **Markdown Editor** or view `architecture.md` on GitHub / in Cursor for rendered Mermaid.

## Next phases

| Phase | Focus |
|-------|--------|
| 1 | docker-compose + infra health |
| 2 | SQL Data + EF seed 20 sensors |
| 3 | IoT → Redis + RabbitMQ |
| 4 | REST API consumer + gRPC |
| 5 | SignalR hub |
| 6 | React 3 pages |
| 7–9 | Tests, CI, README final |
