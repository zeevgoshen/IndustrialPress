import { Link, Route, Routes } from "react-router-dom";
import LiveDashboard from "./pages/LiveDashboard";
import SensorDetail from "./pages/SensorDetail";
import SystemOverview from "./pages/SystemOverview";

export default function App() {
  return (
    <div className="app">
      <nav>
        <Link to="/">Live Dashboard</Link>
        <Link to="/sensor/1">Sensor Detail</Link>
        <Link to="/system">System Overview</Link>
      </nav>
      <main>
        <Routes>
          <Route path="/" element={<LiveDashboard />} />
          <Route path="/sensor/:id" element={<SensorDetail />} />
          <Route path="/system" element={<SystemOverview />} />
        </Routes>
      </main>
    </div>
  );
}
