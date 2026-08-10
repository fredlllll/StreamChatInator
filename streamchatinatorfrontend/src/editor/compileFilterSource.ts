// The filter editor edits a TypeScript function; execution runs the compiled JS
// function body. This module owns the default template, upgrades legacy
// body-only filters to the function form, and compiles the source down to the
// JS body used at runtime (frontend `new Function`, backend Jint).

export const FILTER_TEMPLATE = `function __matches(eventData: ChatEventEnvelope): boolean {
    return eventData.chatEventType === "ChatMessage" && eventData.chatEventData.username === "someviewer";
}`;

// Old filters stored only the function body (e.g. `return eventData.seen;`).
// Wrap them so they become a real function again. If the source already looks
// like a full function, leave it alone.
export function ensureFunction(source: string): string {
    const trimmed = source.trim();
    if (trimmed.length === 0) return FILTER_TEMPLATE;
    if (/(^|\n)\s*(export\s+)?function\s+\w+/.test(trimmed)) return source;
    if (/(^|\n)\s*(export\s+)?(const|let|var)\s+\w+\s*=\s*((async\s*)?\(|async\s*\w+\s*\()/.test(trimmed)) return source;
    if (trimmed.endsWith("}") && trimmed.includes("=>")) return source;
    return `function __matches(eventData: ChatEventEnvelope): boolean {\n${source}\n}`;
}

// Extracts the body between the outer braces of the transpiled function.
function extractFunctionBody(js: string): string {
    const start = js.indexOf("{");
    if (start === -1) return js;
    let depth = 0;
    let inString: '"' | "'" | "`" | null = null;
    let escaped = false;
    for (let i = start; i < js.length; i++) {
        const ch = js[i];
        if (inString) {
            if (escaped) {
                escaped = false;
                continue;
            }
            if (ch === "\\") {
                escaped = true;
                continue;
            }
            if (ch === inString) inString = null;
            continue;
        }
        if (ch === '"' || ch === "'" || ch === "`") {
            inString = ch;
            continue;
        }
        if (ch === "{") {
            depth++;
        } else if (ch === "}") {
            depth--;
            if (depth === 0) return js.slice(start + 1, i);
        }
    }
    return js;
}

// Compiles the TypeScript filter function down to the JavaScript body used for
// execution. Returns the normalized function source (stored in `code`) and the
// compiled body (stored in `codeJs`).
export async function compileFilterSource(raw: string): Promise<{ source: string; codeJs: string }> {
    const source = ensureFunction(raw);
    const ts = await import("typescript");
    const result = ts.transpileModule(source, {
        compilerOptions: {
            target: ts.ScriptTarget.ESNext,
            module: ts.ModuleKind.None,
        },
    });
    const codeJs = extractFunctionBody(result.outputText);
    return { source, codeJs };
}