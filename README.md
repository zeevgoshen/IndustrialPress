# IndustrialPress — Real-Time Industrial Telemetry (Phase 0)

Phase 0 repository skeleton for the industrial digital press home assignment.  
**Architecture, retries, scaling, and CQRS-lite** are documented in [`docs/architecture.md`](docs/architecture.md).

## Prerequisites

- [Visual Studio 2022](https://visualstudio.microsoft.com/) with **ASP.NET and web development** and **.NET desktop development** workloads
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20 LTS](https://nodejs.org/) (for React UI)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for full stack via compose)

## Open in Visual Studio 2022

1. Clone or extract this repository.
2. Open `IndustrialPress.sln`.
3. Set multiple startup projects: **RestApi**, **SqlData**, **IotTelemetry** (right-click solution → *Set Startup Projects* → *Multiple*).
4. See [`docs/GETTING-STARTED-VS2022.md`](docs/GETTING-STARTED-VS2022.md) for ports and debugging.

## Solution structure

```
IndustrialPress.sln
├── contracts/Grpc.Contracts/          # Shared gRPC proto
├── services/rest-api/                 # REST + SignalR hub (Phase 1+)
├── services/sql-data/                 # EF + SQL + gRPC server
├── services/iot-telemetry/            # 20 sensors → Redis + RabbitMQ
├── frontend/web/                      # React + TypeScript (3 pages)
├── docs/architecture.md               # Diagrams, retries, scaling
├── prompts/                           # AI prompt log (mandatory)
└── docker-compose.yml                 # Full stack (Phase 1+)
```

## Build (CLI)

```bash
dotnet restore IndustrialPress.sln
dotnet build IndustrialPress.sln
dotnet test IndustrialPress.sln
```

## Frontend (Phase 1+)

```bash
cd frontend/web
npm install
npm run dev
```

## Docker (Phase 1+)

```bash
docker compose up --build
```

## Phase 0 status

| Item | Status |
|------|--------|
| Solution + projects | ✅ Skeleton builds |
| Architecture doc | ✅ `docs/architecture.md` |
| IoT / Redis / RMQ / SignalR | ⏳ Phase 1+ |
| Integration tests (20 sensors) | ⏳ Phase 7 |
| CI pipeline | ⏳ Skeleton in `.github/workflows/ci.yml` |
| `/prompts` documentation | ⏳ Add as you use AI |

## GitHub

```bash
git init
git add .
git commit -m "Phase 0: solution skeleton and architecture"
git remote add origin https://github.com/YOUR_USER/IndustrialPress.git
git push -u origin main
```

## License

Private assignment repository — add license as required by your institution.
