import * as monaco from "monaco-editor/editor/editor.api.js";
import {
    javascriptDefaults,
    typescriptDefaults,
    ScriptTarget,
    getJavaScriptWorker,
} from "monaco-editor/language/typescript/monaco.contribution.js";
import "monaco-editor/languages/definitions/javascript/register.js";
import "monaco-editor/languages/definitions/typescript/register.js";
import editorWorker from "monaco-editor/editor/editor.worker?worker";
import tsWorker from "monaco-editor/language/typescript/ts.worker?worker";
import filterGlobals from "./filterGlobals.generated.d.ts?raw";

(self as { MonacoEnvironment?: unknown }).MonacoEnvironment = {
    getWorker(_: string, label: string) {
        if (label === "typescript" || label === "javascript") return new tsWorker();
        return new editorWorker();
    },
};

for (const defaults of [javascriptDefaults, typescriptDefaults]) {
    defaults.setDiagnosticsOptions({
        noSemanticValidation: false,
        noSyntaxValidation: false,
    });

    defaults.setCompilerOptions({
        target: ScriptTarget.ESNext,
        allowNonTsExtensions: true,
    });

    // Makes the `eventData` global and the `ChatEventEnvelope` type (and every
    // field on them) available for autocomplete / hover in the filter editor.
    defaults.addExtraLib(filterGlobals, "ts:streamchatinator/filterGlobals.d.ts");
}

// The TS language-service mode (and with it the editor contributions it pulls
// in — suggest, find, …) is loaded lazily on the first javascript model. If an
// editor is created first, it snapshots an empty contribution list and
// autocomplete/find never appear. Warm the module up on purpose and gate the
// editor on it.
void getJavaScriptWorker().catch(() => undefined);

export const monacoReady: Promise<void> = getJavaScriptWorker().then(
    () => undefined,
    () => undefined,
);

export { monaco };