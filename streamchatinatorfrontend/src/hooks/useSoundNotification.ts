import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useChatState } from "../ChatContext";
import { getFilters } from "../api/filtersApi";
import { compileFilter } from "../filterMatcher";
import { getSound, deleteSound, getAllSoundIds } from "../db/soundStorage";
import type { EventFilter } from "../types";

const CONFIG_KEY = "streamchatinator.soundConfig";

interface SoundConfig {
    /** Per-filter volume, 0-1. */
    volumes: Record<string, number>;
    globalVolume: number;
    muted: boolean;
}

function loadConfig(): SoundConfig {
    try {
        const raw = localStorage.getItem(CONFIG_KEY);
        if (raw) {
            const parsed = JSON.parse(raw);
            return {
                volumes: parsed.volumes ?? {},
                globalVolume: typeof parsed.globalVolume === "number" ? parsed.globalVolume : 0.7,
                muted: !!parsed.muted,
            };
        }
    } catch { /* ignore corrupt data */ }
    return { volumes: {}, globalVolume: 0.7, muted: false };
}

function saveConfig(cfg: SoundConfig): void {
    localStorage.setItem(CONFIG_KEY, JSON.stringify(cfg));
}

export interface SoundNotificationState {
    /** Filter IDs that have a sound configured. */
    filterSounds: Set<string>;
    globalVolume: number;
    muted: boolean;
}

export interface SoundNotificationActions {
    setGlobalVolume: (v: number) => void;
    toggleMute: () => void;
    testSound: (filterId?: string) => void;
}

export type SoundNotification = SoundNotificationState & SoundNotificationActions;

export function useSoundNotification(): SoundNotification {
    const { events } = useChatState();

    const [config, setConfig] = useState<SoundConfig>(loadConfig);
    const [filterSounds, setFilterSounds] = useState<Set<string>>(new Set());

    // All loaded filters (for matcher compilation + names).
    const [filters, setFilters] = useState<EventFilter[]>([]);

    // Refs for values needed inside the event-processing effect without
    // adding them to deps (which would re-fire on every delivery).
    const configRef = useRef(config);
    configRef.current = config;
    const filtersRef = useRef(filters);
    filtersRef.current = filters;

    // Audio pool: one Audio per filter ID, keyed by filterId.
    const audioPoolRef = useRef<Map<string, HTMLAudioElement>>(new Map());
    // Object URLs created from blobs, tracked for revocation on unmount.
    const objectUrlsRef = useRef<Map<string, string>>(new Map());
    // Compiled matchers for filters that have a sound.
    const matchersRef = useRef<Map<string, (e: import("../types").FrontEndEventData) => boolean>>(new Map());

    // --- Load filters + re-read config from localStorage ---
    // Re-reads on every filter load so changes made by FilterEditorPage (which
    // writes to localStorage independently) are picked up without a page reload.
    useEffect(() => {
        let cancelled = false;
        getFilters().then(async (all) => {
            if (cancelled) return;
            setFilters(all);
            setConfig(loadConfig());

            // Clean up IndexedDB entries for deleted filters.
            const filterIds = new Set(all.map((f) => f.id));
            const storedIds = await getAllSoundIds();
            for (const id of storedIds) {
                if (!filterIds.has(id)) {
                    await deleteSound(id).catch(() => {});
                }
            }
        }).catch(() => {});
        return () => { cancelled = true; };
    }, []);

    // --- Load blobs from IndexedDB and build audio pool ---
    useEffect(() => {
        let cancelled = false;
        const soundIds = Object.keys(config.volumes).filter((id) => config.volumes[id] > 0 || true);

        // Determine which filters have sounds by checking localStorage config.
        setFilterSounds(new Set(soundIds));

        // Compile matchers for filters that have sounds.
        for (const f of filters) {
            if (soundIds.includes(f.id)) {
                const key = `${f.id}:${f.updated}`;
                if (!matchersRef.current.has(f.id)) {
                    matchersRef.current.set(f.id, compileFilter(f.codeJs, key));
                }
            }
        }

        // Load blobs from IndexedDB and create Audio elements.
        for (const id of soundIds) {
            if (audioPoolRef.current.has(id)) continue;
            getSound(id).then((blob) => {
                if (cancelled || !blob) return;
                const url = URL.createObjectURL(blob);
                objectUrlsRef.current.set(id, url);
                const audio = new Audio(url);
                audioPoolRef.current.set(id, audio);
            }).catch(() => {});
        }

        // Clean up audio for filters that no longer have sounds.
        for (const [id, audio] of audioPoolRef.current) {
            if (!soundIds.includes(id)) {
                audio.pause();
                audioPoolRef.current.delete(id);
                const url = objectUrlsRef.current.get(id);
                if (url) {
                    URL.revokeObjectURL(url);
                    objectUrlsRef.current.delete(id);
                }
                matchersRef.current.delete(id);
            }
        }

        return () => { cancelled = true; };
    }, [config.volumes, filters]);

    // Revoke all object URLs on unmount.
    useEffect(() => {
        const urls = objectUrlsRef.current;
        const pool = audioPoolRef.current;
        return () => {
            for (const url of urls.values()) {
                URL.revokeObjectURL(url);
            }
            urls.clear();
            for (const audio of pool.values()) {
                audio.pause();
            }
            pool.clear();
        };
    }, []);

    // --- Event processing ---
    const prevCountRef = useRef(0);

    useEffect(() => {
        const prevCount = prevCountRef.current;

        // Purge detection: events were cleared.
        if (events.length < prevCount) {
            prevCountRef.current = events.length;
            return;
        }

        if (events.length === prevCount) return;

        const newEvents = events.slice(prevCount, events.length);
        prevCountRef.current = events.length;

        const cfg = configRef.current;
        if (cfg.muted) return;

        const matchers = matchersRef.current;
        const pool = audioPoolRef.current;
        const volumes = cfg.volumes;
        const globalVol = cfg.globalVolume;

        for (const event of newEvents) {
            for (const [filterId, matcher] of matchers) {
                try {
                    if (!matcher(event)) continue;
                } catch {
                    continue;
                }
                const audio = pool.get(filterId);
                if (!audio) continue;
                const vol = (volumes[filterId] ?? 0.7) * globalVol;
                audio.volume = Math.max(0, Math.min(1, vol));
                audio.currentTime = 0;
                audio.play().catch(() => {});
                // Only play the first matching sound per event.
                break;
            }
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [events]);

    // --- Actions ---

    const setGlobalVolume = useCallback((v: number) => {
        setConfig((prev) => {
            const next = { ...prev, globalVolume: v };
            saveConfig(next);
            return next;
        });
    }, []);

    const toggleMute = useCallback(() => {
        setConfig((prev) => {
            const next = { ...prev, muted: !prev.muted };
            saveConfig(next);
            return next;
        });
    }, []);

    const testSound = useCallback((filterId?: string) => {
        const cfg = configRef.current;
        const pool = audioPoolRef.current;
        const volumes = cfg.volumes;

        let targetId = filterId;
        if (!targetId) {
            targetId = Object.keys(volumes)[0];
        }
        if (!targetId) return;

        const audio = pool.get(targetId);
        if (!audio) return;
        const vol = (volumes[targetId] ?? 0.7) * cfg.globalVolume;
        audio.volume = Math.max(0, Math.min(1, cfg.muted ? 0 : vol));
        audio.currentTime = 0;
        audio.play().catch(() => {});
    }, []);

    const state = useMemo(
        () => ({ filterSounds, globalVolume: config.globalVolume, muted: config.muted }),
        [filterSounds, config.globalVolume, config.muted]
    );

    const actions = useMemo(
        () => ({ setGlobalVolume, toggleMute, testSound }),
        [setGlobalVolume, toggleMute, testSound]
    );

    return useMemo(() => ({ ...state, ...actions }), [state, actions]);
}
