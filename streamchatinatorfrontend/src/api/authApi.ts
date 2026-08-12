export interface AuthStatus {
    authenticated: boolean;
    authenticationEnabled: boolean;
}

export async function getAuthStatus(): Promise<AuthStatus> {
    const res = await fetch("/api/auth/me");
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
