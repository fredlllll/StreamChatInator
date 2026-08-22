import { useEffect, useRef, useState } from "react";
import ConnectionIndicator from "./ConnectionIndicator";
import { beginDeviceLogin, type DeviceStartResponse, getDeviceStatus } from "../api/authApi";

function TwitchLoginButton() {
    const [device, setDevice] = useState<DeviceStartResponse | null>(null);
    const [error, setError] = useState<string | null>(null);
    // When set, an in-flight poll loop stops at its next checkpoint. Closing the
    // modal or unmounting sets it, so a late poll completion can't reload the
    // page or show an error toast after the user has cancelled.
    const cancelledRef = useRef(false);

    useEffect(() => {
        return () => {
            cancelledRef.current = true;
        };
    }, []);

    const cancel = () => {
        cancelledRef.current = true;
        setDevice(null);
    };

    const poll = async (d: DeviceStartResponse) => {
        const wait = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));
        const maxAttempts = Math.max(3, Math.ceil(d.expiresIn / d.interval));
        // A single dropped request while polling shouldn't kill the sign-in
        // flow; only give up after several consecutive failures.
        const maxConsecutiveFailures = 5;
        let consecutiveFailures = 0;
        for (let i = 0; i < maxAttempts; i++) {
            await wait(d.interval * 1000);
            if (cancelledRef.current) return;
            try {
                const data = await getDeviceStatus(d.id);
                if (cancelledRef.current) return;
                consecutiveFailures = 0;
                if (data.status === "ok") {
                    cancel();
                    window.location.reload();
                    return;
                }
                if (data.status === "expired" || data.status === "failed") {
                    cancel();
                    setError("Sign-in did not complete. Please try again.");
                    return;
                }
            } catch {
                consecutiveFailures += 1;
                if (consecutiveFailures >= maxConsecutiveFailures) {
                    cancel();
                    setError("Lost connection while waiting for sign-in.");
                    return;
                }
            }
        }
        cancel();
        setError("Sign-in timed out. Please try again.");
    };

    const startLogin = async () => {
        setError(null);
        cancelledRef.current = false;
        try {
            const data = await beginDeviceLogin();
            if (cancelledRef.current) return; // modal closed while the start request was in flight
            setDevice(data);
            void poll(data);
        } catch (err) {
            if (cancelledRef.current) return;
            setError(err instanceof Error ? err.message : "Sign-in failed to start.");
        }
    };

    return (
        <>
            <button type="button" className="btn btn-ghost" onClick={startLogin}>
                Twitch Login
                <ConnectionIndicator />
            </button>

            {device && (
                <div className="login-modal-backdrop" onClick={cancel}>
                    <div className="login-modal" onClick={(e) => e.stopPropagation()}>
                        <h3>Link your Twitch account</h3>
                        <p>
                            Open this link in your browser and enter the code to authorize StreamChatInator:
                        </p>
                        <p className="login-verification">
                            <a href={device.verificationUri} target="_blank" rel="noreferrer">
                                {device.verificationUri}
                            </a>
                        </p>
                        <div className="login-code">{device.userCode}</div>
                        <p className="login-hint">Waiting for authorization&hellip;</p>
                        <div className="login-actions">
                            <button type="button" className="btn" onClick={cancel}>
                                Cancel
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {error && (
                <div className="login-toast" role="alert">
                    {error}
                    <button type="button" className="login-toast-close" onClick={() => setError(null)}>
                        ×
                    </button>
                </div>
            )}
        </>
    );
}

export default TwitchLoginButton;
