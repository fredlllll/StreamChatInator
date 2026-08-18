import { useEffect, useState } from "react";
import { Routes, Route, Navigate, NavLink } from "react-router-dom";
import FiltersPage from "./pages/FiltersPage";
import FilterEditorPage from "./pages/FilterEditorPage";
import ViewPage from "./pages/ViewPage";
import DashboardPage from "./pages/DashboardPage";
import TwitchLoginButton from "./components/TwitchLoginButton";
import LanLogin from "./components/Login";
import { useChatConnection } from "./ChatContext";
import { getAuthStatus, logout } from "./api/authApi";
import { purgeEvents, generateTestEvents } from "./api/eventsApi";
import { useTheme } from "./theme";
import "./App.css";

function App() {
    const { theme, toggleTheme } = useTheme();
    const { tracking, setTracking } = useChatConnection();
    const [authChecking, setAuthChecking] = useState(true);
    const [authenticated, setAuthenticated] = useState(false);
    const [authenticationEnabled, setAuthenticationEnabled] = useState(false);

    useEffect(() => {
        let cancelled = false;
        getAuthStatus()
            .then((status) => {
                if (!cancelled) {
                    setAuthenticated(status.authenticated);
                    setAuthenticationEnabled(status.authenticationEnabled);
                }
            })
            .finally(() => {
                if (!cancelled) setAuthChecking(false);
            });
        return () => {
            cancelled = true;
        };
    }, []);

    function handleLogout() {
        logout().finally(() => window.location.reload());
    }

    async function handlePurgeEvents() {
        const confirmed = window.confirm(
            "Clear all chat events?\n\nThis permanently deletes every recorded event for all filters. Filters themselves are kept."
        );
        if (!confirmed) return;
        try {
            await purgeEvents();
        } catch {
            window.alert("Failed to clear events. Please try again.");
        }
    }

    async function handleTestEvents() {
        try {
            const created = await generateTestEvents();
            window.alert(`Generated ${created} test events (one of each type).`);
        } catch {
            window.alert("Failed to generate test events. Please try again.");
        }
    }

    if (authChecking) {
        return <div className="auth-loading">Loading…</div>;
    }

    if (!authenticated) {
        return <LanLogin />;
    }

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
                        <button
                            type="button"
                            className={`btn btn-ghost${tracking ? "" : " tracking-paused"}`}
                            onClick={() => setTracking(!tracking)}
                            title={tracking ? "Pause recording chat events" : "Resume recording chat events"}
                        >
                            {tracking ? "Pause" : "Play"}
                        </button>
                        <button type="button" className="btn btn-ghost" onClick={handlePurgeEvents}>
                            Clear all events
                        </button>
                        <button
                            type="button"
                            className="btn btn-ghost"
                            onClick={handleTestEvents}
                            title="Creates one event of each type as if it arrived from Twitch, so you can inspect the visuals"
                        >
                            Test events
                        </button>
                        {authenticationEnabled && (
                            <button type="button" className="btn btn-ghost" onClick={handleLogout}>
                                Lock
                            </button>
                        )}
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