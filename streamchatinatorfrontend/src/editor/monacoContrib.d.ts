// Ambient typing for the typescript language contribution that ships with
// monaco-editor. The package publishes JS without companion .d.ts for this
// entrypoint, so declare the pieces of it we use.
declare module "monaco-editor/language/typescript/monaco.contribution.js" {
    export const javascriptDefaults: {
        setDiagnosticsOptions(options: { noSemanticValidation?: boolean; noSyntaxValidation?: boolean }): void;
        setCompilerOptions(options: { target?: number; allowNonTsExtensions?: boolean }): void;
        addExtraLib(content: string, filePath?: string): { dispose(): void };
    };
    export const ScriptTarget: { ESNext: number };
}