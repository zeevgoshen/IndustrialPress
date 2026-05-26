# IndustrialPress — Real-Time Industrial Telemetry

## Quick start (Docker — full stack)

```powershell
cd C:\Users\zeev\IndustrialPress
docker compose up --build
```

| Service | URL |
|---------|-----|
| UI | http://localhost:5173 |
| REST API / Swagger | http://localhost:5101/swagger |
| RabbitMQ management | http://localhost:15672 (industrial / industrial) |
| SQL Server | localhost:1433 (sa / Your_password123) |

## Local development (Visual Studio 2022)

1. Start infrastructure only:

```powershell
docker compose up -d sqlserver redis rabbitmq
```

2. Set **multiple startup projects**: RestApi, SqlData, IotTelemetry.

3. Frontend:

```powershell
cd frontend\web
npm install
npm run dev
```

Open http://localhost:5173 — live dashboard uses **SignalR** (no telemetry polling).

## Architecture

See [`docs/architecture.md`](docs/architecture.md) — diagrams, retry rules R1–R7, CQRS-lite, scaling.

## Data flow (implemented)

```text
IoT → Redis → RabbitMQ → REST API → SignalR → UI
REST API → gRPC → SQL Data → SQL (metadata)
```

## Build & test

```powershell
dotnet build IndustrialPress.sln
dotnet test IndustrialPress.sln
```

Integration tests use **Testcontainers** (Docker must be running).

```powershell
dotnet test services\rest-api\tests\RestApi.IntegrationTests\RestApi.IntegrationTests.csproj
```

## Phase status

| Phase | Status |
|-------|--------|
| 0 | Solution + architecture |
| 1 | docker-compose + health |
| 2 | SQL Data + EF + 20 sensors + gRPC |
| 3 | IoT → Redis + RabbitMQ |
| 4–5 | REST API consumer + SignalR |
| 6 | React 3 pages + SignalR |
| 7 | Unit + integration tests (20-sensor pipeline) |
| 8+ | CI hardening, README architecture final |

## AI prompts

Log under [`prompts/`](prompts/) per assignment requirements.
