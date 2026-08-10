import { useRef, useState } from "react";
import Editor, { loader } from "@monaco-editor/react";
import type { editor } from "monaco-editor";
import { monaco } from "./monacoSetup";

// Use the locally bundled monaco instance instead of the CDN default.
loader.config({ monaco });

const WRAP_HEADER = "function __matches(eventData: ChatEventEnvelope) {\n";
const WRAP_FOOTER = "\n}";

type Problem = {
    message: string;
    line: number;
    column: number;
    severity: "error" | "warning";
};

// The runtime evaluates the filter as `new Function("eventData", code)`, so the
// model is wrapped in a real function. The wrapper lines are hidden from view,
// which removes the top-level `return` parse error and gives `eventData` a typed
// parameter (needed for member autocomplete).
function applyHiddenAreas(instance: editor.IStandaloneCodeEditor) {
    const model = instance.getModel();
    const lineCount = model?.getLineCount() ?? 0;
    if (!model || lineCount < 3) return;
    const setter = instance as unknown as {
        setHiddenAreas: (
            ranges: Array<{ startLineNumber: number; startColumn: number; endLineNumber: number; endColumn: number }>,
        ) => void;
    };
    setter.setHiddenAreas([
        { startLineNumber: 1, startColumn: 1, endLineNumber: 1, endColumn: model.getLineMaxColumn(1) },
        { startLineNumber: lineCount, startColumn: 1, endLineNumber: lineCount, endColumn: model.getLineMaxColumn(lineCount) },
    ]);
}

type FilterCodeEditorProps = {
    value: string;
    onChange: (value: string) => void;
};

function FilterCodeEditor({ value, onChange }: FilterCodeEditorProps) {
    const [problems, setProblems] = useState<Problem[]>([]);
    const editorRef = useRef<editor.IStandaloneCodeEditor | null>(null);

    function handleMount(instance: editor.IStandaloneCodeEditor) {
        editorRef.current = instance;
        applyHiddenAreas(instance);
        instance.onDidChangeModelContent(() => applyHiddenAreas(instance));
        instance.onDidChangeModel(() => applyHiddenAreas(instance));
    }

    function handleValidate(markers: editor.IMarker[]) {
        const lineCount = editorRef.current?.getModel()?.getLineCount() ?? 0;
        setProblems(
            markers
                .filter((m) => m.startLineNumber > 1 && m.endLineNumber < lineCount)
                .map((m) => ({
                    message: m.message,
                    line: m.startLineNumber - 1,
                    column: m.startColumn,
                    severity: m.severity === monaco.MarkerSeverity.Error ? ("error" as const) : ("warning" as const),
                })),
        );
    }

    // The editor model holds the wrapped text; swap in/out the wrapper so parent
    // state only ever sees the user's function body.
    const wrapped = WRAP_HEADER + value + WRAP_FOOTER;
    function unwrap(text: string) {
        if (text.startsWith(WRAP_HEADER) && text.endsWith(WRAP_FOOTER)) {
            return text.slice(WRAP_HEADER.length, text.length - WRAP_FOOTER.length);
        }
        return text;
    }

    return (
        <div>
            <Editor
                height="320px"
                defaultLanguage="javascript"
                value={wrapped}
                onChange={(text) => onChange(unwrap(text ?? ""))}
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