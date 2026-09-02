# StreamChatInator

A desktop/server app that records Twitch chat events in real time and lets you view them through user-defined filters. Backend is ASP.NET Core 10 + SQLite + SignalR; frontend is a React 19 / TypeScript / Vite SPA, packaged into a single deployable app.

## Features

- Connects to Twitch chat via IRC and records every event type (messages, subs, gifts, raids, timeouts, bans, announcements, …).
- **Filters**: define a TypeScript filter that decides which events to show. Filters run in the browser (for live views) and on the server (Jint, for history pagination).
- Real-time event streaming over SignalR with an auto-reconnecting client.
- Emote and badge rendering for supported providers (7TV / BTTV / FFZ, Twitch badges).
- LAN access: reachable from any device on your network; a PIN gates the UI (can be disabled).
- Optional Twitch login (OAuth device flow) for the broadcaster channel.

## Solution layout

```
StreamChatInator/            ASP.NET Core Web API (C#) — entry point Program.cs
streamchatinatorfrontend/    React 19 / TypeScript / Vite SPA
StreamChatInator.Tests/      xUnit tests (.NET 10)
StreamChatInator.slnx        Solution file linking all three projects
publish/                     Publish/build output (gitignored)
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/) (LTS recommended)

## Run in development

From the repo root, one command launches both the backend and the Vite dev server:

```
dotnet run --project StreamChatInator
```

This starts the backend on **port 17455** and launches Vite (via the SPA proxy) on **port 53401**. Open `http://localhost:17455`.

Frontend-only dev (from `streamchatinatorfrontend/`):

```
npm install
npm run dev
```

`npm run dev` regenerates the Monaco editor type definitions before starting — required, don't skip it if you run Vite some other way.

## Configuration

Configuration comes from `StreamChatInator/appsettings.json` (and `appsettings.Development.json`) plus environment variables:

| Setting | Env var | Default | Purpose |
| --- | --- | --- | --- |
| `Port` | `Port` | `17455` | HTTP port the backend listens on |
| `Auth:Enabled` | `Auth__Enabled` | `true` | Toggle the LAN PIN gate |
| `Auth:Pin` | `Auth__Pin` | auto-generated | Fixed LAN PIN (shown on the console panel) |
| `Twitch:ClientId` | `Twitch__ClientId` | built-in test client id | Twitch OAuth app client id |
| `Twitch:JoinChannel` | `Twitch__JoinChannel` | current user | Channel to join for recording |

## Tests

```
dotnet test StreamChatInator.Tests
```

## Lint / typecheck (frontend)

From `streamchatinatorfrontend/`:

```
npm run lint        # oxlint (note: this project uses oxlint, not ESLint)
npx tsc -b          # TypeScript typecheck
```

## Database & migrations

Data is stored in SQLite at `LocalApplicationData/StreamChatInator/db.sqlite` (per-user), not in the project folder. **Pending migrations are applied automatically on app startup** — nothing to do when running or deploying.

During development, add a new migration with `dotnet-ef` (a local tool in `StreamChatInator/dotnet-tools.json`):

```
dotnet ef migrations add <Name> --project StreamChatInator
```

No need to run `dotnet ef database update` — the migration applies itself on next startup.

## Publish

`publish.ps1` builds the backend *and* the React frontend for one or all platforms:

```powershell
./publish.ps1                          # framework-dependent, all platforms
./publish.ps1 -Platform win-x64        # single platform
./publish.ps1 -Mode self-contained     # self-contained
./publish.ps1 -SkipZip                 # don't zip output
```

Output goes to `publish/<rid>/<mode>/`, with a zip alongside. `publish/` is git-ignored.
