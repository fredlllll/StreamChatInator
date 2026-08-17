export interface AuthStatus {
    authenticated: boolean;
    authenticationEnabled: boolean;
}

export type DeviceStartResponse = {
    id: string;
    userCode: string;
    verificationUri: string;
    expiresIn: number;
    interval: number;
};

export type DeviceStatusResponse =
    | { status: "pending" }
    | { status: "ok"; username: string }
    | { status: "expired" }
    | { status: "failed" };

export async function getAuthStatus(): Promise<AuthStatus> {
    const res = await fetch("/api/auth/status");
    if (!res.ok) throw new Error("Failed to check auth status");
    return res.json();
}

export async function loginWithPin(pin: string): Promise<void> {
    const res = await fetch("/api/auth/pin-login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ pin }),
    });
    if (res.status === 429) throw new Error("too_many_attempts");
    if (!res.ok) throw new Error("invalid_pin");
}

export async function logout(): Promise<void> {
    await fetch("/api/auth/logout", { method: "POST" });
}

export async function getDeviceStatus(id: string): Promise<DeviceStatusResponse> {
    const res = await fetch(`/api/auth/device-status?id=${encodeURIComponent(id)}`);
    return (await res.json()) as DeviceStatusResponse;
}

export async function beginDeviceLogin(): Promise<DeviceStartResponse> {
    const res = await fetch("/api/auth/begin_device_login", { method: "POST" });
    if (!res.ok) {
        throw new Error(`Login could not be started (HTTP ${res.status}).`);
    }
    return (await res.json()) as DeviceStartResponse;
}