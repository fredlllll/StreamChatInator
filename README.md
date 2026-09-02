# StreamChatInator

A desktop/server app that records Twitch chat events in real time and lets you view them through user-defined TypeScript filters. Connect to any Twitch channel, see every event — messages, subs, gifts, raids, timeouts, bans, announcements and more — as they happen, with live filterable views.

## Features

- Records every Twitch chat event type via IRC (messages, subs, gifts, raids, timeouts, bans, announcements, …).
- **Filters**: write TypeScript filters to decide which events to show. Filters work both live and on history.
- Real-time event streaming via SignalR with automatic reconnection.
- Emote and badge rendering (7TV / BTTV / FFZ, Twitch badges).
- Reachable from any device on your network; a PIN gates access (can be disabled).

## Download

Grab the latest build from [GitHub Releases](https://github.com/fredlllll/StreamChatInator/releases). Pick the zip for your platform:

| Platform | Zip |
| --- | --- |
| Windows x64 | `StreamChatInator-win-x64.zip` |
| Linux x64 | `StreamChatInator-linux-x64.zip` |
| macOS ARM64 | `StreamChatInator-osx-arm64.zip` |
| Windows x64 | `StreamChatInator-win-x64-self-contained.zip` |
| Linux x64 | `StreamChatInator-linux-x64-self-contained.zip` |
| macOS ARM64 | `StreamChatInator-osx-arm64-self-contained.zip` |

**Self-contained** builds include the .NET runtime — nothing else to install. **Framework-dependent** builds are smaller but require the [.NET 10 runtime](https://dotnet.microsoft.com/download/dotnet/10.0).

## Running

Extract the zip and run the executable. On first launch it creates a database in your user data directory (see [Where is my data?](#where-is-my-data) below).

On startup you'll see a console panel showing the app URL and a LAN access link with a PIN. Open that link on any device on your network to access the UI — clicking the link enters the PIN automatically.

If the console panel can't be shown (piped output, service mode), the PIN is logged instead.

### Twitch login (required)

The app needs a Twitch account to connect to chat. On first launch, click the **Twitch login** button in the UI and follow the device code flow:

1. Click **Twitch login** — a code and link appear.
2. Open the link in your browser, enter the code, and authorize.
3. The app detects authorization automatically and connects.

Login is remembered between restarts. If the token expires, the app prompts you to log in again.

### Set your own PIN

To lock the app to a fixed PIN (useful for headless / remote server setups), set it via environment variable or config:

```bash
Auth__Pin=123456       # environment variable
```

Or in `appsettings.json`:

```json
{ "Auth": { "Pin": "123456" } }
```

### Disable authentication

To disable the PIN gate entirely (e.g. for personal use on a private LAN):

```bash
Auth__Enabled=false
```

## Configuration

Settings are read from `appsettings.json` and environment variables:

| Setting | Env var | Default | Purpose |
| --- | --- | --- | --- |
| `Port` | `Port` | `17455` | HTTP port the backend listens on |
| `Auth:Enabled` | `Auth__Enabled` | `true` | Toggle the LAN PIN gate |
| `Auth:Pin` | `Auth__Pin` | auto-generated | Fixed LAN PIN |
| `Twitch:JoinChannel` | `Twitch__JoinChannel` | current user | Channel to join for recording |

## Where is my data?

The SQLite database lives in:

- **Windows**: `%LOCALAPPDATA%\StreamChatInator\db.sqlite`
- **macOS / Linux**: `~/.local/share/StreamChatInator/db.sqlite`

Migrations are applied automatically on startup — nothing to do when updating.

---

## For developers

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/) (LTS recommended)

### Run in development

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

### Tests

```
dotnet test StreamChatInator.Tests
```

### Lint / typecheck (frontend)

From `streamchatinatorfrontend/`:

```
npm run lint        # oxlint (note: this project uses oxlint, not ESLint)
npx tsc -b          # TypeScript typecheck
```

### Database migrations

During development, add a new migration with `dotnet-ef` (a local tool in `StreamChatInator/dotnet-tools.json`):

```
dotnet ef migrations add <Name> --project StreamChatInator
```

No need to run `dotnet ef database update` — the migration applies itself on next startup.

### Publish

`publish.ps1` builds the backend *and* the React frontend for one or all platforms:

```powershell
./publish.ps1                          # framework-dependent, all platforms
./publish.ps1 -Platform win-x64        # single platform
./publish.ps1 -Mode self-contained     # self-contained
./publish.ps1 -SkipZip                 # don't zip output
```

Output goes to `publish/<rid>/<mode>/`, with a zip alongside. `publish/` is git-ignored.

### Solution layout

```
StreamChatInator/            ASP.NET Core Web API (C#) — entry point Program.cs
streamchatinatorfrontend/    React 19 / TypeScript / Vite SPA
StreamChatInator.Tests/      xUnit tests (.NET 10)
StreamChatInator.slnx        Solution file linking all three projects
publish/                     Publish/build output (gitignored)
```
