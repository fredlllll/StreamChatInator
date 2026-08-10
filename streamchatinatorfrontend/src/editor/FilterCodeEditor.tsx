import Editor, { loader } from "@monaco-editor/react";
import { monaco } from "./monacoSetup";

// Use the locally bundled monaco instance instead of the CDN default.
loader.config({ monaco });

type FilterCodeEditorProps = {
    value: string;
    onChange: (value: string) => void;
};

function FilterCodeEditor({ value, onChange }: FilterCodeEditorProps) {
    return (
        <Editor
            height="320px"
            defaultLanguage="javascript"
            value={value}
            onChange={(value) => onChange(value ?? "")}
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
    );
}

export default FilterCodeEditor;