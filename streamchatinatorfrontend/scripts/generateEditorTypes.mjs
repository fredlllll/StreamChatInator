import ts from "typescript";
import { writeFileSync, mkdirSync, watch } from "node:fs";
import { basename, dirname, join, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const projectRoot = resolve(__dirname, "..");
const typesFile = join(projectRoot, "src", "types.ts");
const outputFile = join(projectRoot, "src", "editor", "filterGlobals.generated.d.ts");

function splitTopLevelUnion(text, sep = "|") {
    const parts = [];
    let depth = 0;
    let current = "";
    for (const char of text) {
        if ("([{".includes(char)) depth++;
        else if (")]}".includes(char)) depth--;
        if (char === sep && depth === 0) {
            parts.push(current);
            current = "";
        } else {
            current += char;
        }
    }
    if (current.trim() !== "") parts.push(current);
    return parts;
}

function normalizeMemberType(text) {
    const seen = new Set();
    const parts = [];
    for (const token of splitTopLevelUnion(text)) {
        const trimmed = token.trim();
        if (trimmed !== "" && !seen.has(trimmed)) {
            seen.add(trimmed);
            parts.push(trimmed);
        }
    }
    return parts.join(" | ");
}

export function generateEditorTypes() {
    const program = ts.createProgram([typesFile], {
        target: ts.ScriptTarget.ESNext,
        module: ts.ModuleKind.ESNext,
        moduleResolution: ts.ModuleResolutionKind.Bundler,
        skipLibCheck: true,
    });
    const diagnostics = ts.getPreEmitDiagnostics(program);
    if (diagnostics.length > 0) {
        const messages = diagnostics.map(
            (d) => ts.flattenDiagnosticMessageText(d.messageText, "\n")
        );
        throw new Error(`Cannot generate editor types, ${typesFile} has errors:\n${messages.join("\n")}`);
    }
    const sourceFile = program.getSourceFile(typesFile);

    const payloadDecls = new Map();
    const aliases = new Map();
    let chatEventTypeText = null;
    for (const stmt of sourceFile.statements) {
        if (ts.isInterfaceDeclaration(stmt) && stmt.name) {
            const name = stmt.name.text;
            if (name.startsWith("ChatEvent")) {
                payloadDecls.set(name, stmt);
            }
        } else if (ts.isTypeAliasDeclaration(stmt) && stmt.name) {
            const name = stmt.name.text;
            aliases.set(name, stmt.type.getText(sourceFile));
            if (name === "ChatEventType") {
                const type = program.getTypeChecker().getTypeFromTypeNode(stmt.type);
                chatEventTypeText = program.getTypeChecker().typeToString(type, stmt, ts.TypeFormatFlags.NoTruncation);
            }
        }
    }

    const checker = program.getTypeChecker();
    const typeFlags = ts.TypeFormatFlags.NoTruncation;

    const merged = new Map();
    for (const [payloadName, decl] of payloadDecls) {
        const symbol = checker.getSymbolAtLocation(decl.name);
        const type = checker.getDeclaredTypeOfSymbol(symbol);
        for (const prop of type.getProperties()) {
            const name = prop.getName();
            if (name.startsWith("__")) continue;
            const valueDecl = prop.valueDeclaration ?? decl.name;
            const typeText = normalizeMemberType(
                checker.typeToString(checker.getTypeOfSymbolAtLocation(prop, valueDecl), valueDecl, typeFlags)
            );
            let entry = merged.get(name);
            if (!entry) {
                entry = { types: new Set(), origins: new Set() };
                merged.set(name, entry);
            }
            entry.types.add(typeText);
            entry.origins.add(payloadName);
        }
    }

    const sortedNames = [...merged.keys()].sort();
    const fieldLines = sortedNames.map((name) => {
        const { types, origins } = merged.get(name);
        const typeText = normalizeMemberType([...types].join(" | "));
        const originsText = [...origins].sort().join(", ");
        return `    /** Present on: ${originsText} */\n    ${name}?: ${typeText};`;
    });

    const chatEventType = chatEventTypeText ?? "string";
    const payloadsList = [...payloadDecls.keys()].sort().join(", ");
    const aliasLines = [...aliases.keys()]
        .sort()
        .map((name) => `type ${name} = ${aliases.get(name)};`);

    const out = `// AUTO-GENERATED from src/types.ts — do not edit.
// Regenerate with \`npm run generate:editor-types\` (also runs automatically on dev/build).
//
// Mirrors the chat event data serialized to JSON (camelCase) and passed to
// the filter code as \`eventData\` — both in the browser
// (new Function("eventData", code)) and on the server (Jint:
// function __matches(eventData) { ... }).

// Type aliases used by the payload fields below, copied verbatim from types.ts.
${aliasLines.join("\n")}

interface ChatEventEnvelope {
    eventId: string;
    chatEventType: ${chatEventType};
    seen: boolean;
    /** Payload of the event. Present on one of: ${payloadsList}. */
    chatEventData: ChatEventDataMerged;
}

/** Every field that exists on ANY chat event payload (all optional, so they autocomplete regardless of chatEventType). */
interface ChatEventDataMerged {
${fieldLines.join("\n")}
}

declare const eventData: ChatEventEnvelope;
`;

    mkdirSync(dirname(outputFile), { recursive: true });
    writeFileSync(outputFile, out, "utf8");
    return outputFile;
}

export function editorTypesPlugin() {
    const targetName = basename(typesFile);
    let fsWatcher;
    return {
        name: "streamchatinator-editor-types",
        buildStart() {
            generateEditorTypes();
        },
        configureServer(server) {
            generateEditorTypes();
            fsWatcher?.close();
            fsWatcher = watch(dirname(typesFile), (eventType, filename) => {
                const name = filename ? filename.toString() : "";
                if (!name.endsWith(targetName)) return;
                try {
                    generateEditorTypes();
                    server.ws.send({ type: "full-reload", path: "*" });
                } catch (error) {
                    console.error("Failed to regenerate editor types:", error);
                }
            });
            server.httpServer?.once("close", () => fsWatcher?.close());
        },
        buildEnd() {
            fsWatcher?.close();
        },
    };
}

if (import.meta.url === pathToFileURL(process.argv[1] ?? "").href) {
    try {
        const written = generateEditorTypes();
        console.log(`Editor types written to ${written}`);
    } catch (error) {
        console.error(error);
        process.exit(1);
    }
}