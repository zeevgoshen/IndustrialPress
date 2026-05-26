import { useEffect, useState } from "react";

export default function SystemOverview() {
  const [health, setHealth] = useState<Record<string, unknown> | null>(null);

  useEffect(() => {
    fetch("/api/health")
      .then((r) => r.json())
      .then(setHealth)
      .catch(() => setHealth({ status: "unreachable" }));
  }, []);

  return (
    <section>
      <h1>System Overview</h1>
      <h2>REST API health</h2>
      <pre>{JSON.stringify(health, null, 2)}</pre>
      <p>20 sensors configured. Live telemetry via SignalR only (no polling).</p>
    </section>
  );
}
