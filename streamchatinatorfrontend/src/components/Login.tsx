import { useEffect, useState } from "react";
import { loginWithPin } from "../api/authApi";

export default function Login() {
    const [pin, setPin] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);
    const [autoLoggingIn, setAutoLoggingIn] = useState(false);

    // The console shows a link carrying the PIN as a query param, so clicking
    // or copying it unlocks the UI without typing anything. On success (or a
    // bad pin) the param is stripped so it doesn't linger in the address bar
    // or browser history.
    useEffect(() => {
        const urlPin = new URLSearchParams(window.location.search).get("pin");
        if (!urlPin) return;
        setAutoLoggingIn(true);
        loginWithPin(urlPin).then(
            () => {
                stripPinFromUrl();
                window.location.reload();
            },
            (err) => {
                stripPinFromUrl();
                setAutoLoggingIn(false);
                setError(
                    err instanceof Error && err.message === "too_many_attempts"
                        ? "Too many attempts — wait a moment and try again."
                        : "That PIN isn't right."
                );
            }
        );
    }, []);

    function stripPinFromUrl() {
        const url = new URL(window.location.href);
        if (url.searchParams.has("pin")) {
            url.searchParams.delete("pin");
            window.history.replaceState({}, "", url);
        }
    }

    async function handleSubmit(e: React.SubmitEvent) {
        e.preventDefault();
        if (!pin || submitting) return;
        setSubmitting(true);
        setError(null);
        try {
            await loginWithPin(pin);
            // Full reload so the fresh session cookie re-mounts everything
            // (SignalR connection included) cleanly.
            window.location.reload();
        } catch (err) {
            setError(
                err instanceof Error && err.message === "too_many_attempts"
                    ? "Too many attempts — wait a moment and try again."
                    : "That PIN isn't right."
            );
            setPin("");
        } finally {
            setSubmitting(false);
        }
    }

    if (autoLoggingIn) {
        return (
            <div className="auth-screen">
                <div className="auth-card">
                    <h1>StreamChatInator</h1>
                    <p>Logging you in…</p>
                </div>
            </div>
        );
    }

    return (
        <div className="auth-screen">
            <form className="auth-card" onSubmit={handleSubmit}>
                <h1>StreamChatInator</h1>
                <p>
                    Enter the LAN access PIN shown on the machine running this app to unlock the
                    chat UI.
                </p>
                <input
                    className="input"
                    type="password"
                    inputMode="numeric"
                    autoFocus
                    value={pin}
                    onChange={(e) => setPin(e.target.value)}
                    placeholder="PIN"
                    disabled={submitting}
                />
                {error && <p className="auth-error">{error}</p>}
                <button type="submit" className="btn btn-primary" disabled={submitting || pin.length === 0}>
                    Unlock
                </button>
            </form>
        </div>
    );
}
