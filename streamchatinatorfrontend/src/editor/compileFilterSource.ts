// The filter editor edits a TypeScript script; the script must define a
// `__matches(eventData)` function (and may add any helpers it likes). Saving
// compiles it down to plain JS and stores the whole script (`codeJs`); at
// runtime we just run it and call `__matches`, so nothing needs to know the
// function signature anymore. This module owns the default template and does
// the TS → JS compile.

export const FILTER_TEMPLATE = `function __matches(eventData: ChatEventEnvelope): boolean {
    return eventData.chatEventType === "ChatMessage" && eventData.chatEventData.username === "someviewer";
}`;

// `ts.transpileModule` prepends a "use strict" pragma and, when the source
// uses `export`, ESM → CJS boilerplate (Object.defineProperty(exports, ...)).
// The stored script is run bare (new Function / Jint), so drop all of that.
function stripModuleBoilerplate(js: string): string {
    return js
        .split("\n")
        .filter((line) => {
            const l = line.trim();
            return !(
                l === "" ||
                l === '"use strict";' ||
                l.startsWith("Object.defineProperty(exports,") ||
                /^exports\.[\w$]+\s*=/.test(l)
            );
        })
        .join("\n");
}

// Compiles the TypeScript filter script down to the plain JS stored in `codeJs`
// and executed at runtime. Returns the source (stored in `code`).
export async function compileFilterSource(source: string): Promise<{ source: string; codeJs: string }> {
    const ts = await import("typescript");
    const result = ts.transpileModule(source, {
        compilerOptions: {
            target: ts.ScriptTarget.ESNext,
            module: ts.ModuleKind.None,
        },
    });
    const codeJs = stripModuleBoilerplate(result.outputText);
    return { source, codeJs };
}