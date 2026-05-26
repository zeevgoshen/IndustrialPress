import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useCallback, useEffect, useRef, useState } from "react";

export interface TelemetryPayload {
  sensorId: number;
  value: number;
  unit: string;
  timestamp: string;
  status: string;
}

const hubUrl = "/hubs/telemetry";

export function useTelemetryHub() {
  const connectionRef = useRef<HubConnection | null>(null);
  const [connected, setConnected] = useState(false);
  const [readings, setReadings] = useState<Record<number, TelemetryPayload>>({});

  const applyPayload = useCallback((payload: TelemetryPayload) => {
    setReadings((prev) => ({ ...prev, [payload.sensorId]: payload }));
  }, []);

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    connection.on("TelemetrySnapshot", (snapshot: TelemetryPayload[]) => {
      const map: Record<number, TelemetryPayload> = {};
      for (const item of snapshot) map[item.sensorId] = item;
      setReadings(map);
    });

    connection.on("TelemetryUpdated", (payload: TelemetryPayload) => {
      applyPayload(payload);
    });

    connection
      .start()
      .then(() => setConnected(true))
      .catch((err) => console.error("SignalR connect failed", err));

    connection.onreconnected(() => setConnected(true));
    connection.onclose(() => setConnected(false));

    connectionRef.current = connection;

    return () => {
      connection.stop();
    };
  }, [applyPayload]);

  return { connected, readings };
}
