import { useCallback, useEffect, useRef, useState } from "react";
import type { SoundNotification } from "../hooks/useSoundNotification";

type Props = SoundNotification;

function SoundNotificationSettings({ globalVolume, muted, setGlobalVolume, toggleMute, testSound, filterSounds }: Props) {
    const [open, setOpen] = useState(false);
    const panelRef = useRef<HTMLDivElement>(null);
    const buttonRef = useRef<HTMLButtonElement>(null);

    const hasSounds = filterSounds.size > 0;

    // Close on outside click.
    useEffect(() => {
        if (!open) return;
        function handleClick(e: MouseEvent) {
            if (
                panelRef.current && !panelRef.current.contains(e.target as Node) &&
                buttonRef.current && !buttonRef.current.contains(e.target as Node)
            ) {
                setOpen(false);
            }
        }
        document.addEventListener("mousedown", handleClick);
        return () => document.removeEventListener("mousedown", handleClick);
    }, [open]);

    const handleVolumeChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
        setGlobalVolume(Number(e.target.value) / 100);
    }, [setGlobalVolume]);

    return (
        <div className="sound-nav-wrapper">
            <button
                ref={buttonRef}
                type="button"
                className={`btn btn-ghost${muted ? " sound-muted" : ""}`}
                onClick={() => setOpen((o) => !o)}
                title="Sound notification settings"
                disabled={!hasSounds}
            >
                {muted ? "🔇" : "🔊"}
            </button>
            {open && (
                <div ref={panelRef} className="sound-nav-panel">
                    <div className="sound-nav-row">
                        <label htmlFor="sound-global-volume">Volume</label>
                        <input
                            id="sound-global-volume"
                            type="range"
                            min={0}
                            max={100}
                            value={Math.round(globalVolume * 100)}
                            onChange={handleVolumeChange}
                        />
                    </div>
                    <div className="sound-nav-row">
                        <button type="button" className="btn btn-sm" onClick={toggleMute}>
                            {muted ? "Unmute" : "Mute"}
                        </button>
                        <button
                            type="button"
                            className="btn btn-sm btn-primary"
                            onClick={() => testSound()}
                            disabled={!hasSounds}
                        >
                            Test
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}

export default SoundNotificationSettings;
