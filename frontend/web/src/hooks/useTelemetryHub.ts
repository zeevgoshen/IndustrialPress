import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useCallback, useEffect, useRef, useState } from "react";

export interface TelemetryPayload {
  sensorId: number;
  value: number;
  unit: string;
  timestamp: string;
  status: string;
}

/** Same-origin /hubs works with Vite proxy (dev) and nginx (docker). Override for direct API URL. */
function resolveHubUrl(): string {
  const base = import.meta.env.VITE_API_URL as string | undefined;
  if (base) return `${base.replace(/\/$/, "")}/hubs/telemetry`;
  return "/hubs/telemetry";
}

export function useTelemetryHub() {
  const connectionRef = useRef<HubConnection | null>(null);
  const [connected, setConnected] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [readings, setReadings] = useState<Record<number, TelemetryPayload>>({});

  const applyPayload = useCallback((payload: TelemetryPayload) => {
    setReadings((prev) => ({ ...prev, [payload.sensorId]: payload }));
  }, []);

  useEffect(() => {
    const hubUrl = resolveHubUrl();
    const connection = new HubConnectionBuilder()
      .withUrl(hubUrl, { withCredentials: true })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    connection.on("TelemetrySnapshot", (snapshot: TelemetryPayload[]) => {
      setError(null);
      const map: Record<number, TelemetryPayload> = {};
      for (const item of snapshot) map[item.sensorId] = item;
      setReadings(map);
    });

    connection.on("TelemetryUpdated", (payload: TelemetryPayload) => {
      applyPayload(payload);
    });

    connection
      .start()
      .then(() => {
        setConnected(true);
        setError(null);
      })
      .catch((err: Error) => {
        console.error("SignalR connect failed", hubUrl, err);
        setConnected(false);
        setError(
          `Cannot reach ${hubUrl}. Is RestApi running on port 5101? ` +
            `(Docker: rebuild frontend. VS: npm run dev with proxy, or set VITE_API_URL=http://localhost:5101)`
        );
      });

    connection.onreconnected(() => {
      setConnected(true);
      setError(null);
    });
    connection.onclose(() => setConnected(false));

    connectionRef.current = connection;

    return () => {
      void connection.stop();
    };
  }, [applyPayload]);

  return { connected, error, readings };
}
