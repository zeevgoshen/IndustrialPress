# IndustrialPress — Real-Time Industrial Telemetry

Distributed home assignment: **20 sensors @ 1 Hz**, Redis as telemetry source of truth, RabbitMQ + gRPC between backends, REST + SignalR to the UI. Full stack runs with **docker-compose**; backends are **C# / .NET 8**.

## Quick start (Docker — full stack)

```powershell
git clone <your-repo-url>
cd IndustrialPress
docker compose up --build
```

| Service | URL |
|---------|-----|
| UI | http://localhost:5173 |
| REST API / Swagger | http://localhost:5101/swagger |
| RabbitMQ management | http://localhost:15672 (user `industrial`, password `industrial`) |
| SQL Server | `localhost:1433` (user `sa`, password `Your_password123`) |

**Pages:** Live Dashboard (`/`), Sensor Detail (`/sensor/:id`), System Overview (`/overview`).

## Local development (Visual Studio 2022)

See [`docs/GETTING-STARTED-VS2022.md`](docs/GETTING-STARTED-VS2022.md).

1. Infrastructure only: `docker compose up -d sqlserver redis rabbitmq`
2. Multiple startup projects: **RestApi**, **SqlData**, **IotTelemetry**
3. Frontend: `cd frontend\web` → `npm install` → `npm run dev` → http://localhost:5173

Live telemetry uses **SignalR only** (no polling).

## Build & test

```powershell
dotnet build IndustrialPress.sln -c Release
dotnet test IndustrialPress.sln -c Release
```

| Test type | Projects | Notes |
|-----------|----------|--------|
| Unit | `IotTelemetry.UnitTests`, `SqlData.UnitTests`, `RestApi.UnitTests` | No Docker |
| Integration | `RestApi.IntegrationTests` | **Docker required** (Testcontainers Redis + RabbitMQ); proves all **20 sensors** through Redis → RabbitMQ → SignalR |

```powershell
dotnet test services\rest-api\tests\RestApi.IntegrationTests\RestApi.IntegrationTests.csproj -c Release
```

## CI/CD

GitHub Actions workflow [`.github/workflows/ci.yml`](.github/workflows/ci.yml):

1. Restore, build, and test the .NET solution (integration tests use Testcontainers on the runner’s Docker).
2. Install and build the React frontend.
3. Validate `docker-compose.yml` and build application Docker images.

Triggers: push/PR to `main` or `master`.

## Repository layout

```text
IndustrialPress.sln
contracts/protos/          # gRPC contracts
services/rest-api/         # REST + SignalR + RMQ consumer + Redis read
services/sql-data/         # EF Core + SQL + gRPC metadata
services/iot-telemetry/    # 20×1 Hz simulator → Redis + RabbitMQ
frontend/web/              # React + TypeScript (3 pages)
docs/architecture.md       # Detailed diagrams, R1–R7 retries, scaling
prompts/                   # AI prompt log (mandatory)
docker-compose.yml
```

Each application service has its own **Dockerfile**; infrastructure (SQL Server, Redis, RabbitMQ) uses official images in compose.

---

## Architecture (mandatory)

> **Deep dive:** [`docs/architecture.md`](docs/architecture.md) (mermaid diagrams, communication matrix, retry rules R1–R7).

### High-level design and service boundaries

Hub-and-spoke: the **REST API** is the only backend the UI talks to. **IoT Telemetry** owns generation and writes **Redis** first. **SQL Data** owns relational **metadata** (20 seeded sensors). The UI never touches Redis, RabbitMQ, SQL, or peer services.

```text
┌─────────────┐  REST + SignalR   ┌──────────────────┐
│ React UI    │◄─────────────────►│ REST API         │
│ (3 pages)   │                   │ REST, Hub, RMQ   │
└─────────────┘                   └────────┬─────────┘
                                           │
              ┌────────────────────────────┼────────────────────────┐
              │ gRPC (metadata)            │ RabbitMQ (notify)       │ Redis (read)
              ▼                            ▼                         ▼
      ┌──────────────┐              ┌──────────────┐           ┌─────────┐
      │ SQL Data     │              │ IoT Telemetry│──────────►│ Redis   │
      │ EF + SQL     │              │ simulator    │  write    │ latest  │
      └──────┬───────┘              └──────────────┘           └─────────┘
             ▼
      ┌──────────────┐
      │ SQL Server   │  Sensors table (metadata only)
      └──────────────┘
```

| Service | Responsibility | Does not |
|---------|----------------|----------|
| **IoT Telemetry** | 20 sensors, ~1 Hz; Redis write (R1); RMQ publish (R2) | Serve UI; read SQL |
| **REST API** | REST metadata proxy; consume RMQ; read Redis; SignalR push | Store telemetry in SQL |
| **SQL Data** | EF migrations; `Sensors` metadata; gRPC queries | Live telemetry path |
| **UI** | 3 pages; REST for metadata; SignalR for live values | Poll telemetry; direct infra access |

### Data flows

**Live telemetry (hot path)**

```text
IoT generates sample
  → [sync] Redis SET telemetry:sensor:{id}     (authoritative latest)
  → [async] RabbitMQ sensor.updated { sensorId }
  → REST API consumer
  → [sync] Redis GET
  → SignalR TelemetryUpdated → UI
```

On SignalR **connect/reconnect**: server sends `TelemetrySnapshot` (all 20 keys from Redis) — **no polling**.

**Metadata (cold path)**

```text
UI → REST GET /api/sensors[/{id}]
  → REST API → gRPC → SQL Data → EF → SQL (Sensors)
```

Live samples are **not** written to SQL in this MVP.

### Why gRPC, RabbitMQ, and SignalR here

| Technology | Where | Why |
|------------|-------|-----|
| **gRPC** | REST API ↔ SQL Data | Typed, low-latency request/response for **metadata** queries; fits internal service-to-service RPC; keeps SQL behind one service boundary. |
| **RabbitMQ** | IoT → REST API | **Decouples** producers from consumers; buffers bursts; survives API restarts; notification payload stays tiny (`sensorId` only) while Redis holds full state. |
| **SignalR** | REST API ↔ UI | Native **server push** over WebSockets in ASP.NET; reconnect + snapshot matches assignment; avoids forbidden telemetry polling. |

**Why not gRPC for telemetry?** The spec requires RabbitMQ between backends; Redis already holds the payload. Sending full samples over both Redis and gRPC would duplicate data without benefit at 20 Hz.

**Why not RabbitMQ for metadata?** Metadata changes rarely; synchronous gRPC + REST is simpler and gives immediate HTTP status codes to the UI.

### Trade-offs and constraints

| Choice | Benefit | Cost |
|--------|---------|------|
| Redis latest-value | Fast reads; clear source of truth | No built-in history |
| Notify-then-read (RMQ + Redis GET) | Small messages; idempotent consumer | Extra Redis read per event |
| SQL metadata only | SQL off hot path | Two stores to reason about |
| CQRS-lite (not full CQRS) | Right store per job | No event replay / audit log |
| Single REST API instance (MVP) | Simple SignalR | Scale-out needs Redis backplane |
| Bounded retries R1–R7 | Predictable failure behavior | Missed ticks possible under prolonged outage |

**Spec constraints honored:** UI ↔ REST only; SignalR only on REST API; backends ↔ gRPC + RabbitMQ only; telemetry originates in Redis; no telemetry polling.

### Evolution: more sensors or higher rates

| Stage | Change |
|-------|--------|
| **20 sensors (now)** | ~20 writes/s, ~20 RMQ msgs/s, ~20 SignalR pushes/s — trivial |
| **100s of sensors** | Batch RMQ notifications (100–250 ms); batch SignalR `TelemetryBatchUpdated`; Redis `MGET` or hash snapshot |
| **1000s** | Partition IoT by sensor range; multiple API consumers; SignalR groups / backplane |
| **Higher Hz** | Reduce per-tick RMQ chatter; consider Redis Streams; avoid per-sample SQL writes |
| **History** | Async worker: RMQ or Redis Stream → Timescale/SQL batch insert; Redis stays hot path |

First bottleneck at scale: **REST API + SignalR fan-out**, not Redis or metadata SQL.

### Failure scenarios

| Failure | System behavior |
|---------|-----------------|
| **Redis down** | IoT R1 fails → no publish (R2 skipped); API cannot read; UI stale/disconnected; recovers on next successful write + events |
| **RabbitMQ delayed/down** | IoT may have Redis data but UI lags; queue backs up when broker returns; consumer R3/R4 nack + requeue; DLQ after 5 redeliveries (R5) |
| **REST API restart** | RMQ queues messages; consumer resumes; UI SignalR reconnects → `TelemetrySnapshot` (R7) |
| **SQL Data / gRPC down** | Metadata REST returns 503 (R6); **live SignalR unaffected** |
| **Missed RMQ message** | Redis still correct; next sensor tick or reconnect snapshot heals |
| **Duplicate RMQ delivery** | Idempotent: re-read Redis, push latest again |

Retry summary: **R1–R7** in [`docs/architecture.md`](docs/architecture.md) (bounded backoff, no infinite loops on the 1 Hz timer).

### Operations: logs, metrics, signals

**Today:** structured logs on retry exhaustion, consumer nacks, DLQ routing, gRPC failures.

**Production additions:**

| Signal | Purpose |
|--------|---------|
| `iot_redis_write_failures_total` | R1 health |
| `rmq_publish_failures_total` | R2 health |
| `api_consumer_lag_seconds` | Queue backlog |
| `signalr_connections` / `push_latency_ms` | UI path |
| `redis_command_latency` | Hot path SLO |
| Traces: IoT → Redis → RMQ → API → SignalR | End-to-end latency |
| DLQ depth alert | Poison / persistent failures |

Correlate by `sensorId` and `traceId` on each tick.

### Sensor behavior (fast / slow / constant)

| Behavior | Handling | If performance matters |
|----------|----------|------------------------|
| **Fast-changing** | Each change overwrites Redis; RMQ notifies; UI gets pushes | Throttle/coalesce per sensor before publish |
| **Slow-changing** | Same path; many RMQ messages carry little new info | Suppress publish if value unchanged within epsilon |
| **Constant long periods** | Redis holds last value; few visible UI changes | Heartbeat-only RMQ optional; snapshot on connect still sufficient |

MVP treats all 20 sensors uniformly at 1 Hz. Optimizations above are the first knobs before architectural change.

### Stable vs likely to change

| Stable (assignment core) | Likely to evolve |
|--------------------------|------------------|
| UI ↔ REST API only | UI auth, routing, styling |
| Redis as live source of truth | Key schema versioning |
| 3-page structure | Extra admin/history pages |
| gRPC metadata contract | New RPCs for config/history |
| RMQ `sensor.updated` contract | Batch envelope, routing keys |
| Retry policy shape (bounded) | Backoff tuning from metrics |

### Alternative designs

| Alternative | When | Trade-off |
|-------------|------|-----------|
| **Redis Pub/Sub instead of RMQ** | Smaller deployment | Weaker persistence; harder DLQ/retry story |
| **Redis Streams only** | Unified log + latest | More moving parts for reviewers |
| **gRPC streaming for telemetry** | Low-latency LAN | Violates RMQ requirement; couples IoT to API uptime |
| **SQL for every sample** | Audit/compliance | Breaks hot-path latency; needs async ingest |
| **Full CQRS + event store** | Many consumers, replay | Overkill for 20×1 Hz MVP |

**Would implement differently for production at scale:** batch notifications, SignalR backplane, partitioned IoT workers, async history pipeline — without changing the assignment’s boundary rules.

### CQRS-lite (one paragraph)

We use a **pragmatic CQRS split**: Redis for live telemetry reads and writes, SQL for sensor metadata. The pipeline is **event-notified** via RabbitMQ with a **synchronous Redis write before publish** for consistency. This is **not event-sourced**; we use **latest-value semantics**. The system is **not async end-to-end**; it is async at the broker and push layers, sync at the authoritative Redis write and metadata queries.

---

## AI prompts

All AI-assisted prompts are logged under [`prompts/`](prompts/) per assignment rules. See [`prompts/README.md`](prompts/README.md) for naming and template.

## Submission checklist

- [ ] Git repository URL shared with reviewers
- [ ] `docker compose up --build` runs full stack from clean clone
- [ ] GitHub Actions CI green on `main`/`master` (build, tests, Docker images)
- [ ] `dotnet test IndustrialPress.sln` passes locally (Docker running for integration tests)
- [ ] README architecture section (above) + [`docs/architecture.md`](docs/architecture.md)
- [ ] [`prompts/`](prompts/) complete

## Phase status

| Phase | Status |
|-------|--------|
| 0 | Solution + architecture doc |
| 1 | docker-compose + health |
| 2 | SQL Data + EF + 20 sensors + gRPC |
| 3 | IoT → Redis + RabbitMQ |
| 4–5 | REST API consumer + SignalR |
| 6 | React 3 pages + SignalR |
| 7 | Unit + integration tests (20-sensor pipeline) |
| 8 | CI hardening + README architecture + submission checklist |
