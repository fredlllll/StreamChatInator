import { Routes, Route, Link, Navigate } from "react-router-dom";
import FiltersPage from "./pages/FiltersPage";
import ViewPage from "./pages/ViewPage";
import DashboardPage from "./pages/DashboardPage";
import "./App.css";

function App() {
    return (
        <div>
            <nav>
                <Link to="/filters">Filters</Link>
                {" | "}
                <Link to="/dashboard">Dashboard</Link>
            </nav>

            <Routes>
                <Route path="/" element={<Navigate to="/dashboard" replace />} />
                <Route path="/filters" element={<FiltersPage />} />
                <Route path="/view/:filterId" element={<ViewPage />} />
                <Route path="/dashboard" element={<DashboardPage />} />
            </Routes>
        </div>
    );
}

export default App;