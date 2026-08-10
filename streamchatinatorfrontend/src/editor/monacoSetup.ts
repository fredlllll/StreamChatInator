import * as monaco from "monaco-editor/editor/editor.api.js";
import {
    javascriptDefaults,
    ScriptTarget,
} from "monaco-editor/language/typescript/monaco.contribution.js";
import "monaco-editor/languages/definitions/javascript/register.js";
import editorWorker from "monaco-editor/editor/editor.worker?worker";
import tsWorker from "monaco-editor/language/typescript/ts.worker?worker";
import filterGlobals from "./filterGlobals.generated.d.ts?raw";

(self as { MonacoEnvironment?: unknown }).MonacoEnvironment = {
    getWorker(_: string, label: string) {
        if (label === "typescript" || label === "javascript") return new tsWorker();
        return new editorWorker();
    },
};

javascriptDefaults.setDiagnosticsOptions({
    noSemanticValidation: false,
    noSyntaxValidation: false,
});

javascriptDefaults.setCompilerOptions({
    target: ScriptTarget.ESNext,
    allowNonTsExtensions: true,
});

// Makes the `eventData` global (and every field on it) available for
// autocomplete / hover in the filter code editor.
javascriptDefaults.addExtraLib(filterGlobals, "ts:streamchatinator/filterGlobals.d.ts");

export { monaco };