import { Routes, Route, Navigate, NavLink } from "react-router-dom";
import FiltersPage from "./pages/FiltersPage";
import FilterEditorPage from "./pages/FilterEditorPage";
import ViewPage from "./pages/ViewPage";
import DashboardPage from "./pages/DashboardPage";
import TwitchLoginButton from "./components/TwitchLoginButton";
import { useTheme } from "./theme";
import "./App.css";

function App() {
    const { theme, toggleTheme } = useTheme();

    return (
        <div className="app-shell">
            <nav className="app-nav">
                <div className="app-nav-inner">
                    <span className="app-brand">StreamChatInator</span>
                    <div className="app-nav-links">
                        <NavLink to="/filters" className={({ isActive }) => (isActive ? "active" : "")}>
                            Filters
                        </NavLink>
                        <NavLink to="/dashboard" className={({ isActive }) => (isActive ? "active" : "")}>
                            Dashboard
                        </NavLink>
                    </div>
                    <div className="app-nav-actions">
                        <TwitchLoginButton />
                        <button type="button" className="btn btn-ghost" onClick={toggleTheme}>
                            {theme === "dark" ? "Light mode" : "Dark mode"}
                        </button>
                    </div>
                </div>
            </nav>

            <main className="app-main">
                <Routes>
                    <Route path="/" element={<Navigate to="/dashboard" replace />} />
                    <Route path="/filters" element={<FiltersPage />} />
                    <Route path="/filters/new" element={<FilterEditorPage />} />
                    <Route path="/filters/:filterId/edit" element={<FilterEditorPage />} />
                    <Route path="/view/:filterId" element={<ViewPage />} />
                    <Route path="/dashboard" element={<DashboardPage />} />
                </Routes>
            </main>
        </div>
    );
}

export default App;