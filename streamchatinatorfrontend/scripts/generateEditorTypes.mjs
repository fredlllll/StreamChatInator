import ts from "typescript";
import { writeFileSync, mkdirSync, watch } from "node:fs";
import { basename, dirname, join, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const projectRoot = resolve(__dirname, "..");
const typesFile = join(projectRoot, "src", "chatEventTypes.ts");
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
    const interfaceDecls = [];
    let chatEventTypeNode = null;
    for (const stmt of sourceFile.statements) {
        if (ts.isInterfaceDeclaration(stmt) && stmt.name) {
            interfaceDecls.push(stmt);
            if (stmt.name.text.startsWith("ChatEvent")) {
                payloadDecls.set(stmt.name.text, stmt);
            }
        } else if (ts.isTypeAliasDeclaration(stmt) && stmt.name) {
            const name = stmt.name.text;
            aliases.set(name, stmt.type.getText(sourceFile));
            if (name === "ChatEventType") {
                chatEventTypeNode = stmt.type;
            }
        }
    }

    // Literal member names of ChatEventType (used to build the discriminant map).
    let chatEventTypeMembers = null;
    if (chatEventTypeNode) {
        const type = program.getTypeChecker().getTypeFromTypeNode(chatEventTypeNode);
        const collect = (t) => {
            if (!t) return [];
            const parts = [];
            for (const member of t) {
                if (member.isStringLiteral()) parts.push(member.value);
                else if (member.isUnion()) parts.push(...collect(member.types));
            }
            return parts;
        };
        if (type.isUnion()) chatEventTypeMembers = collect(type.types);
        else if (type.isStringLiteral()) chatEventTypeMembers = [type.value];
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

    const aliasLines = [...aliases.keys()]
        .sort()
        .map((name) => `type ${name} = ${aliases.get(name)};`);

    // Print the full interfaces from chatEventTypes.ts verbatim so the payload
    // types (ChatEvent*), and the bases they extend (Model, ChatUserNoticeBase),
    // are real types in the editor that can be referenced / cast to. Drop the
    // `export` keywords — a d.ts with top-level exports loads as a module in
    // the Monaco TS worker, which would stop `eventData` from being global.
    const printer = ts.createPrinter({ newLine: ts.NewLineKind.LineFeed });
    const interfaceLines = interfaceDecls.map(
        (decl) =>
            printer
                .printNode(ts.EmitHint.Unspecified, decl, sourceFile)
                .replace(/^export\s+/gm, "")
                .trim()
    );

    // One payload type per ChatEventType ("None" carries no payload).
    let byTypeLines = [`    None: unknown;`];
    if (chatEventTypeMembers) {
        byTypeLines = chatEventTypeMembers.map((k) => {
            const payloadName = `ChatEvent${k}`;
            const ref = payloadDecls.has(payloadName) ? payloadName : "unknown";
            return `    ${k}: ${ref};`;
        });
    }
    const byTypeBody = byTypeLines.join("\n");

    const out = `// AUTO-GENERATED from src/chatEventTypes.ts — do not edit.
// Regenerate with \`npm run generate:editor-types\` (also runs automatically on dev/build).
//
// Mirrors the chat event data serialized to JSON (camelCase) and passed to
// the filter code as \`eventData\` — both in the browser
// (new Function("eventData", code)) and on the server (Jint:
// function __matches(eventData) { ... }).

// Type aliases used by the payload fields below, copied verbatim from chatEventTypes.ts.
${aliasLines.join("\n")}

// Chat event payload interfaces (and the base interfaces they extend), copied
// verbatim from chatEventTypes.ts, so you can cast or type against the real
// payload type, e.g. \`eventData.chatEventData as ChatEventAnnouncement\`.
${interfaceLines.join("\n\n")}

/** Maps every ChatEventType to the payload it carries ("None" has no payload). */
type ChatEventDataByType = {
${byTypeBody}
};

/** Payload of any chat event: \`ChatEventAnnouncement | ChatEventAnonGiftPaidUpgrade | ... | ChatEventUserTimedout\`. */
type ChatEventDataUnion = ChatEventDataByType[ChatEventType];

/** Every field that exists on ANY chat event payload (all optional, so they autocomplete regardless of chatEventType). */
interface ChatEventDataMerged {
${fieldLines.join("\n")}
}

/**
 * The event envelope, discriminated on \`chatEventType\`. \`chatEventData\`
 * narrows to the matching payload type automatically:
 *     if (eventData.chatEventType === "ChatMessage") {
 *         eventData.chatEventData.username; // ChatEventChatMessage
 *     }
 * or cast explicitly: \`eventData.chatEventData as ChatEventAnnouncement\`.
 */
type ChatEventEnvelope = {
    [K in ChatEventType]: {
        eventId: string;
        chatEventType: K;
        seen: boolean;
        chatEventData: ChatEventDataByType[K];
    };
}[ChatEventType];

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