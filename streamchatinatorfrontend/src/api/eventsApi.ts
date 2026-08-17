export async function purgeEvents(): Promise<number> {
    const res = await fetch("/api/chatEvents", { method: "DELETE" });
    if (!res.ok) throw new Error("Failed to purge events");
    const body = (await res.json()) as { deleted: number };
    return body.deleted;
}

export async function generateTestEvents(): Promise<number> {
    const res = await fetch("/api/chatEvents/testdata", { method: "POST" });
    if (!res.ok) throw new Error("Failed to generate test events");
    const body = (await res.json()) as { created: number };
    return body.created;
}
