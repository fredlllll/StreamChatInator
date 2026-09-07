import { Suspense, lazy, useCallback, useEffect, useRef, useState } from "react";
import { createFilter, updateFilter, getFilterById, invalidateFilter } from "../api/filtersApi";
import { useNavigate, useParams } from "react-router-dom";
import { FILTER_TEMPLATE, compileFilterSource } from "../editor/compileFilterSource";
import { saveSound, getSound, deleteSound } from "../db/soundStorage";

const FilterCodeEditor = lazy(() => import("../editor/FilterCodeEditor"));

const SOUND_CONFIG_KEY = "streamchatinator.soundConfig";

function readSoundConfig(): Record<string, number> {
    try {
        const raw = localStorage.getItem(SOUND_CONFIG_KEY);
        if (raw) return JSON.parse(raw).volumes ?? {};
    } catch { /* ignore */ }
    return {};
}

function writeSoundConfig(volumes: Record<string, number>): void {
    try {
        const raw = localStorage.getItem(SOUND_CONFIG_KEY);
        const cfg = raw ? JSON.parse(raw) : {};
        cfg.volumes = volumes;
        localStorage.setItem(SOUND_CONFIG_KEY, JSON.stringify(cfg));
    } catch { /* ignore */ }
}

function FilterEditorPage() {
    const { filterId } = useParams<{ filterId: string }>();
    const creating = !filterId;
    const navigate = useNavigate();

    const [name, setName] = useState("");
    const [code, setCode] = useState(FILTER_TEMPLATE);
    const [loading, setLoading] = useState(!creating);
    const [notFound, setNotFound] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // Sound config state.
    const [hasSound, setHasSound] = useState(false);
    const [soundFileName, setSoundFileName] = useState<string | null>(null);
    const [soundVolume, setSoundVolume] = useState(0.7);
    const fileInputRef = useRef<HTMLInputElement>(null);
    const previewAudioRef = useRef<HTMLAudioElement | null>(null);
    const objectUrlRef = useRef<string | null>(null);

    useEffect(() => {
        if (creating) return;
        let alive = true;
        getFilterById(filterId)
            .then((filter) => {
                if (!alive) return;
                setName(filter.name);
                setCode(filter.code);
                setLoading(false);
            })
            .catch(() => {
                if (!alive) return;
                setNotFound(true);
                setLoading(false);
            });
        return () => {
            alive = false;
        };
    }, [filterId, creating]);

    // Load existing sound config for this filter.
    useEffect(() => {
        if (!filterId) return;
        let alive = true;
        const volumes = readSoundConfig();
        if (filterId in volumes) {
            setHasSound(true);
            setSoundVolume(volumes[filterId]);
            getSound(filterId).then((blob) => {
                if (blob && alive) {
                    setSoundFileName("notification-sound");
                }
            });
        }
        return () => { alive = false; };
    }, [filterId]);

    async function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
        e.preventDefault();
        try {
            const { source, codeJs } = await compileFilterSource(code);
            if (creating) {
                await createFilter(name, source, codeJs);
            } else {
                await updateFilter(filterId, name, source, codeJs);
                invalidateFilter(filterId);
            }
            navigate("/filters");
        } catch {
            setError("Failed to save filter.");
        }
    }

    const handleSoundUpload = useCallback(async (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file || !filterId) return;
        try {
            await saveSound(filterId, file);
            const volumes = readSoundConfig();
            volumes[filterId] = soundVolume;
            writeSoundConfig(volumes);
            setHasSound(true);
            setSoundFileName(file.name);
        } catch {
            setError("Failed to save sound file.");
        }
        // Reset input so the same file can be re-selected.
        e.target.value = "";
    }, [filterId, soundVolume]);

    const handleRemoveSound = useCallback(async () => {
        if (!filterId) return;
        try {
            await deleteSound(filterId);
            const volumes = readSoundConfig();
            delete volumes[filterId];
            writeSoundConfig(volumes);
            setHasSound(false);
            setSoundFileName(null);
            if (objectUrlRef.current) {
                URL.revokeObjectURL(objectUrlRef.current);
                objectUrlRef.current = null;
            }
        } catch {
            setError("Failed to remove sound.");
        }
    }, [filterId]);

    const handleSoundVolumeChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
        const vol = Number(e.target.value) / 100;
        setSoundVolume(vol);
        if (filterId) {
            const volumes = readSoundConfig();
            volumes[filterId] = vol;
            writeSoundConfig(volumes);
        }
    }, [filterId]);

    const handlePreviewSound = useCallback(async () => {
        if (!filterId) return;
        const blob = await getSound(filterId);
        if (!blob) return;
        if (objectUrlRef.current) URL.revokeObjectURL(objectUrlRef.current);
        const url = URL.createObjectURL(blob);
        objectUrlRef.current = url;
        const audio = new Audio(url);
        audio.volume = soundVolume;
        previewAudioRef.current = audio;
        audio.play().catch(() => {});
    }, [filterId, soundVolume]);

    // Cleanup object URL on unmount.
    useEffect(() => {
        return () => {
            if (objectUrlRef.current) URL.revokeObjectURL(objectUrlRef.current);
        };
    }, []);

    if (loading) return <div className="page"><p>Loading filter...</p></div>;
    if (notFound) return <div className="page"><p>Filter not found.</p></div>;

    return (
        <div className="page">
            <div className="page-header">
                <h2>{creating ? "New filter" : "Edit filter"}</h2>
            </div>

            <form className="card editor-card" onSubmit={handleSubmit}>
                {error && <p className="error">{error}</p>}
                <div className="editor-field">
                    <label htmlFor="filter-name">Name</label>
                    <input
                        id="filter-name"
                        type="text"
                        className="input"
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                        placeholder="Filter name"
                        required
                    />
                </div>

                {!creating && (
                    <div className="editor-field">
                        <label>Notification Sound</label>
                        {hasSound ? (
                            <div className="sound-config-row">
                                <span className="sound-config-name">{soundFileName}</span>
                                <input
                                    type="range"
                                    min={0}
                                    max={100}
                                    value={Math.round(soundVolume * 100)}
                                    onChange={handleSoundVolumeChange}
                                    title={`Volume: ${Math.round(soundVolume * 100)}%`}
                                />
                                <button type="button" className="btn btn-sm" onClick={handlePreviewSound}>
                                    Play
                                </button>
                                <button type="button" className="btn btn-sm btn-danger" onClick={handleRemoveSound}>
                                    Remove
                                </button>
                            </div>
                        ) : (
                            <div className="sound-config-row">
                                <input
                                    ref={fileInputRef}
                                    type="file"
                                    accept="audio/*"
                                    className="sound-file-input"
                                    onChange={handleSoundUpload}
                                />
                                <button
                                    type="button"
                                    className="btn btn-sm"
                                    onClick={() => fileInputRef.current?.click()}
                                >
                                    Upload sound
                                </button>
                                <span className="sound-config-hint">
                                    Audio file stored locally in your browser
                                </span>
                            </div>
                        )}
                    </div>
                )}

                <Suspense fallback={<div>Loading code editor...</div>}>
                    <FilterCodeEditor value={code} onChange={setCode} />
                </Suspense>
                <p className="editor-hint">
                    The filter is a TypeScript script. It must define{" "}
                    <code>__matches(eventData)</code>, plus any helper functions it likes. Type{" "}
                    <code>eventData.</code> to see the available fields, and{" "}
                    <code>eventData.chatEventType === "</code> for the list of event types.
                </p>

                <div className="editor-actions">
                    <button type="button" className="btn" onClick={() => navigate("/filters")}>Cancel</button>
                    <button type="submit" className="btn btn-primary">{creating ? "Create" : "Save"}</button>
                </div>
            </form>
        </div>
    );
}

export default FilterEditorPage;