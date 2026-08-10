import { useEffect, useRef, useState } from "react";
import Editor, { loader } from "@monaco-editor/react";
import type { editor } from "monaco-editor";
import { monaco, monacoReady } from "./monacoSetup";

// Use the locally bundled monaco instance instead of the CDN default.
loader.config({ monaco });

type Problem = {
    message: string;
    line: number;
    column: number;
    severity: "error" | "warning";
};

type FilterCodeEditorProps = {
    value: string;
    onChange: (value: string) => void;
};

function FilterCodeEditor({ value, onChange }: FilterCodeEditorProps) {
    const [problems, setProblems] = useState<Problem[]>([]);
    const [ready, setReady] = useState(false);
    const editorRef = useRef<editor.IStandaloneCodeEditor | null>(null);

    useEffect(() => {
        let alive = true;
        monacoReady.then(() => {
            if (alive) setReady(true);
        });
        return () => {
            alive = false;
        };
    }, []);

    function handleMount(instance: editor.IStandaloneCodeEditor) {
        editorRef.current = instance;
    }

    // The model is the TS source the user edits; markers map straight through.
    function handleValidate(markers: editor.IMarker[]) {
        setProblems(
            markers.map((m) => ({
                message: m.message,
                line: m.startLineNumber,
                column: m.startColumn,
                severity: m.severity === monaco.MarkerSeverity.Error ? ("error" as const) : ("warning" as const),
            })),
        );
    }

    if (!ready) {
        return <div>Loading code editor...</div>;
    }

    return (
        <div>
            <Editor
                height="320px"
                defaultLanguage="typescript"
                value={value}
                onChange={(text) => onChange(text ?? "")}
                onMount={handleMount}
                onValidate={handleValidate}
                loading="Loading code editor..."
                options={{
                    minimap: { enabled: false },
                    fontSize: 14,
                    tabSize: 4,
                    scrollBeyondLastLine: false,
                    wordWrap: "on",
                    automaticLayout: true,
                    tabCompletion: "on",
                    suggestOnTriggerCharacters: true,
                }}
            />
            {problems.length > 0 && (
                <ul style={{ listStyle: "none", margin: "8px 0 0", padding: 0 }}>
                    {problems.map((p, i) => (
                        <li key={i} style={{ fontSize: 13, fontFamily: "var(--mono)", lineHeight: 1.4 }}>
                            <span style={{ color: p.severity === "error" ? "#f48771" : "#d89b1b" }}>
                                {p.severity === "error" ? "error" : "warning"}
                            </span>{" "}
                            <span style={{ color: "var(--text-h)" }}>{p.message}</span>{" "}
                            <span style={{ color: "var(--text)" }}>
                                line {p.line}:{p.column}
                            </span>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}

export default FilterCodeEditor;