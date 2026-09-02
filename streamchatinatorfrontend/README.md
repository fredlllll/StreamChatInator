# StreamChatInator frontend

React 19 / TypeScript / Vite SPA for StreamChatInator. Uses **oxlint** (not ESLint) for linting.

## Scripts

| Command | Description |
| --- | --- |
| `npm run dev` | Start the Vite dev server (port **53401**). Runs `generate:editor-types` first — required. |
| `npm run build` | Typecheck (`tsc -b`) + regenerates editor types + production build to `dist/`. |
| `npm run lint` | Run oxlint. |
| `npm run generate:editor-types` | Regenerate `src/editor/filterGlobals.generated.d.ts` from `src/chatEventTypes.ts`. |
| `npm run preview` | Preview the production build. |
| `npm run perf:harness` | Build + serve the performance harness on port 53401. |
| `npm run perf:test` | Run the Playwright performance tests. |

## Editor type generation

`src/editor/filterGlobals.generated.d.ts` is **auto-generated** from `src/chatEventTypes.ts` by `scripts/generateEditorTypes.mjs`. It mirrors the chat event JSON shape into the Monaco editor's filter code completion. It runs automatically on `dev` and `build`; run `npm run generate:editor-types` manually if you're working outside those scripts.

## Structure

- `src/pages/` — route pages (dashboard, filters, editor, view).
- `src/components/` — shared components.
- `src/components/chatitems/` — one renderer per chat event type.
- `src/editor/` — Monaco-based filter code editor.
- `src/api/` — REST client wrappers.
- `src/badges/`, `src/emoteReplace/` — Twitch badge + emote rendering.
- `src/ChatContext.tsx` — SignalR connection, tracking, and seen-state.
- `src/harness.tsx` — performance test harness used by the Playwright tests.

## Dev behind the backend

In normal development run `dotnet run` from the repo root; it launches this Vite server via the SPA proxy. You can also run `npm run dev` here directly (CORS allows `localhost:53401` in dev), but then you must run the backend separately on `localhost:17455`.

## Dependencies

Notable: `@microsoft/signalr` (real-time), `@monaco-editor/react` + `monaco-editor` (filter editor), `react-virtuoso` (virtualized event list), `flexlayout-react` (dashboard layout), `react-router-dom` (routing).
