import { useTelemetryHub, TelemetryPayload } from "../hooks/useTelemetryHub";

function isStale(timestamp: string): boolean {
  const age = Date.now() - new Date(timestamp).getTime();
  return age > 3000;
}

export default function LiveDashboard() {
  const { connected, error, readings } = useTelemetryHub();
  const list = Object.values(readings).sort((a, b) => a.sensorId - b.sensorId);

  return (
    <section>
      <h1>Live Dashboard</h1>
      <p>SignalR: {connected ? "connected" : "disconnected"}</p>
      {error && <p style={{ color: "crimson" }}>{error}</p>}
      <table>
        <thead>
          <tr>
            <th>ID</th>
            <th>Value</th>
            <th>Unit</th>
            <th>Status</th>
            <th>Updated</th>
          </tr>
        </thead>
        <tbody>
          {connected && list.length === 0 && (
            <tr>
              <td colSpan={5}>
                Connected — waiting for telemetry. Ensure IoT Telemetry service is running.
              </td>
            </tr>
          )}
          {!connected && list.length === 0 && (
            <tr>
              <td colSpan={5}>Not connected to telemetry hub.</td>
            </tr>
          )}
          {list.map((r: TelemetryPayload) => (
            <tr key={r.sensorId} className={isStale(r.timestamp) ? "stale" : ""}>
              <td>{r.sensorId}</td>
              <td>{r.value.toFixed(2)}</td>
              <td>{r.unit}</td>
              <td>{r.status}</td>
              <td>{new Date(r.timestamp).toLocaleTimeString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </section>
  );
}
