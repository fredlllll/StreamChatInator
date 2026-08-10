import { Routes, Route, Link, Navigate } from "react-router-dom";
import FiltersPage from "./pages/FiltersPage";
import FilterEditorPage from "./pages/FilterEditorPage";
import ViewPage from "./pages/ViewPage";
import DashboardPage from "./pages/DashboardPage";
import ConnectionIndicator from "./components/ConnectionIndicator";
import "./App.css";

function App() {
    return (
        <div>
            <nav>
                <Link to="/filters">Filters</Link>
                {" | "}
                <Link to="/dashboard">Dashboard</Link>
                {" | "}
                <Link to="/api/auth/login" reloadDocument>Twitch Login<ConnectionIndicator/></Link>
            </nav>

            <Routes>
                <Route path="/" element={<Navigate to="/dashboard" replace />} />
                <Route path="/filters" element={<FiltersPage />} />
                <Route path="/filters/new" element={<FilterEditorPage />} />
                <Route path="/filters/:filterId/edit" element={<FilterEditorPage />} />
                <Route path="/view/:filterId" element={<ViewPage />} />
                <Route path="/dashboard" element={<DashboardPage />} />
            </Routes>
        </div>
    );
}

export default App;