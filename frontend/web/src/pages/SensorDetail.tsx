import { useParams } from "react-router-dom";

/** Phase 6: REST metadata + SignalR for one sensor */
export default function SensorDetail() {
  const { id } = useParams();
  return (
    <section>
      <h1>Sensor Detail</h1>
      <p>Sensor ID: {id}</p>
      <p>Phase 0 placeholder.</p>
    </section>
  );
}
