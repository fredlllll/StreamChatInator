import { useEffect, useRef, useState } from "react";
import { Routes, Route, Navigate, NavLink } from "react-router-dom";
import FiltersPage from "./pages/FiltersPage";
import FilterEditorPage from "./pages/FilterEditorPage";
import ViewPage from "./pages/ViewPage";
import DashboardPage from "./pages/DashboardPage";
import TwitchLoginButton from "./components/TwitchLoginButton";
import LanLogin from "./components/Login";
import { useChatActions, useChatState } from "./ChatContext";
import { getAuthStatus, logout } from "./api/authApi";
import { purgeEvents, generateTestEvents } from "./api/eventsApi";
import { useTheme } from "./theme";
import "./App.css";

function App() {
    const { theme, toggleTheme } = useTheme();
    const { tracking } = useChatState();
    const { setTracking } = useChatActions();
    const [authChecking, setAuthChecking] = useState(true);
    const [authCheckFailed, setAuthCheckFailed] = useState(false);
    const [authenticated, setAuthenticated] = useState(false);
    const [authenticationEnabled, setAuthenticationEnabled] = useState(false);
    // Guards against stale auth responses when the check is re-run via Retry.
    const authRunRef = useRef(0);

    function checkAuth() {
        const runId = ++authRunRef.current;
        setAuthChecking(true);
        setAuthCheckFailed(false);
        getAuthStatus()
            .then((status) => {
                if (authRunRef.current !== runId) return;
                setAuthenticated(status.authenticated);
                setAuthenticationEnabled(status.authenticationEnabled);
            })
            .catch(() => {
                // "Not logged in" is a valid answer; failing to *ask* (backend
                // down, network blip) must not kick a signed-in user to the
                // PIN screen. Surface it as its own retryable state instead.
                if (authRunRef.current !== runId) return;
                setAuthCheckFailed(true);
            })
            .finally(() => {
                if (authRunRef.current === runId) setAuthChecking(false);
            });
    }

    useEffect(() => {
        checkAuth();
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

    if (authCheckFailed) {
        return (
            <div className="auth-screen">
                <div className="auth-card">
                    <h1>StreamChatInator</h1>
                    <p>Couldn't reach the server to check your session.</p>
                    <button type="button" className="btn btn-primary" onClick={checkAuth}>
                        Retry
                    </button>
                </div>
            </div>
        );
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