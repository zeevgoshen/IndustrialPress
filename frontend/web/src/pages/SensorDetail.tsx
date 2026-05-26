import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { useTelemetryHub, TelemetryPayload } from "../hooks/useTelemetryHub";

export default function SensorDetail() {
  const { id } = useParams();
  const sensorId = Number(id);
  const { connected, readings } = useTelemetryHub();
  const [meta, setMeta] = useState<{ name: string; location: string } | null>(null);
  const live = readings[sensorId];

  useEffect(() => {
    fetch(`/api/sensors/${sensorId}`)
      .then((r) => (r.ok ? r.json() : null))
      .then((data) => {
        if (data) setMeta({ name: data.name, location: data.location });
      })
      .catch(() => setMeta(null));
  }, [sensorId]);

  return (
    <section>
      <h1>Sensor Detail</h1>
      <p>SignalR: {connected ? "connected" : "disconnected"}</p>
      {meta && (
        <p>
          {meta.name} — {meta.location}
        </p>
      )}
      {live ? (
        <TelemetryCard payload={live} />
      ) : (
        <p>No live reading yet for sensor {sensorId}</p>
      )}
    </section>
  );
}

function TelemetryCard({ payload }: { payload: TelemetryPayload }) {
  return (
    <div>
      <p>Value: {payload.value.toFixed(2)} {payload.unit}</p>
      <p>Status: {payload.status}</p>
      <p>Updated: {new Date(payload.timestamp).toLocaleString()}</p>
    </div>
  );
}
