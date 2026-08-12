export async function purgeEvents(): Promise<number> {
    const res = await fetch("/api/events", { method: "DELETE" });
    if (!res.ok) throw new Error("Failed to purge events");
    const body = (await res.json()) as { deleted: number };
    return body.deleted;
}
