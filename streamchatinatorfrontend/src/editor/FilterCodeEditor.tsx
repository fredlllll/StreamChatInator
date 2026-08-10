import { useEffect, useRef, useState } from "react";
import Editor, { loader } from "@monaco-editor/react";
import type { editor } from "monaco-editor";
import { monaco, monacoReady } from "./monacoSetup";
import { useTheme } from "../theme";

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
    const { theme } = useTheme();

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
                theme={theme === "dark" ? "vs-dark" : "vs"}
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
                <ul className="editor-problems">
                    {problems.map((p, i) => (
                        <li key={i}>
                            <span className={`severity ${p.severity}`}>{p.severity}</span>
                            <span className="message">{p.message}</span>{" "}
                            <span className="loc">line {p.line}:{p.column}</span>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}

export default FilterCodeEditor;