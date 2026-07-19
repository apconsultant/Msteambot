# Teams Live Transcript → SharePoint Bot

A Microsoft Teams meeting bot that joins a meeting, reads the live transcript via
Microsoft Graph, and writes it incrementally (every 1–2 minutes) to a `.docx`
document in a per-meeting SharePoint folder.

This repository has been scaffolded as a .NET 8 C# ASP.NET Core application.

Full design spec: [`docs/teams-transcript-bot-prompt.md`](./docs/teams-transcript-bot-prompt.md)

---

## How it works

```
User adds bot to a Teams meeting
        │
        ▼
Bot joins meeting (Graph API) → subscribes to onlineMeetings/{id}/transcripts
        │
        ▼
Transcript chunks arrive via Graph change notifications (webhook)
        │
        ▼
Buffer service accumulates chunks in memory, keyed by meeting ID
        │
        ▼
Timer flush (every 60–120s) → appends buffered text to the meeting's .docx
        │
        ▼
Microsoft Graph upload → per-meeting folder in SharePoint Document Library
```

Key decisions already locked in (see full spec for rationale):
- Transcript source: **Microsoft Graph transcript API**, not raw audio/Speech-to-Text.
- Bot join model: **manual** — user adds the bot to their own meeting.
- Storage: **one folder per meeting**, named `{MeetingTitle}-{Date}-{MeetingId}`.
- Retention: **none** — documents are kept indefinitely.

---

## Prerequisites

- Microsoft 365 tenant with Teams, and a SharePoint site to write to.
- Tenant admin access (to register the app and grant admin consent).
- Teams meeting **Recording and Transcription** policy enabled for the org/users who'll use the bot.
- Azure subscription (for hosting the bot + Azure Bot Service registration).
- Node.js 18+ **or** .NET 8 SDK, depending on chosen stack (see below).
- [Microsoft 365 Agents Toolkit](https://learn.microsoft.com/microsoftteams/platform/toolkit/teams-toolkit-fundamentals) or Bot Framework SDK/CLI, for local scaffolding and sideloading.
- ngrok or Dev Tunnels, for local webhook testing (Graph change notifications need a public HTTPS endpoint).

---

## Project setup

### 1. Register the Azure AD app

In the [Azure Portal](https://portal.azure.com) → App registrations → New registration.

Grant these **application** permissions (admin consent required):

| Permission | Why |
|---|---|
| `Calls.JoinGroupCall.All` | Bot joins the meeting as a participant |
| `OnlineMeetings.Read.All` | Read meeting metadata |
| `OnlineMeetingTranscript.Read.All` | Read live transcript content |
| `Sites.Selected` (preferred) or `Sites.ReadWrite.All` | Write documents to the target SharePoint library |

> If using `Sites.Selected`, also grant the app write access to the specific
> SharePoint site via `POST /sites/{site-id}/permissions` — see Microsoft Graph docs.

Save the **Application (client) ID**, **Directory (tenant) ID**, and create a
**client secret** (or certificate, preferred for production).

### 2. Register the bot in Azure Bot Service

- Create an Azure Bot resource, link it to the App Registration above.
- Enable the **Teams** channel.
- Enable **Calling** on the bot resource and set the webhook URL to your app's
  `/api/calling` (or equivalent) endpoint.

### 3. Clone and configure

```bash
git clone <this-repo-url>
cd teams-transcript-bot
cp .env.example .env
```

Fill in `.env`:

```
AAD_APP_CLIENT_ID=
AAD_APP_CLIENT_SECRET=
AAD_APP_TENANT_ID=
SHAREPOINT_SITE_ID=
SHAREPOINT_DRIVE_ID=
SHAREPOINT_ROOT_FOLDER=Meeting Transcripts
GRAPH_WEBHOOK_NOTIFICATION_URL=https://<your-public-url>/api/notifications
FLUSH_INTERVAL_SECONDS=90
PORT=3978
```

### 4. Install dependencies

**Node.js stack:**
```bash
npm install
```

**.NET stack:**
```bash
dotnet restore
```

### 5. Run locally

Expose a public HTTPS tunnel (Graph webhooks require this):

```bash
ngrok http 3978
```

Update `GRAPH_WEBHOOK_NOTIFICATION_URL` in `.env` with the ngrok URL, then:

```bash
npm run dev        # Node
# or
dotnet run          # .NET
```

### 6. Sideload the bot into Teams

- Package the Teams app manifest (`/appPackage`) with your bot ID and permissions.
- Upload via Teams Admin Center or **Apps → Manage your apps → Upload a custom app**
  in the Teams client (for personal testing).
- Add the bot to a test meeting and confirm it appears as a participant.

---

## Project structure

```
/src
  /bot              # Bot Framework calling/meeting event handlers
  /graph            # Microsoft Graph API client (transcripts, SharePoint upload)
  /buffer           # Per-meeting transcript buffer + flush timer
  /docgen           # .docx generation/append logic
  /webhooks         # Graph change-notification receiver
/appPackage         # Teams app manifest, icons
/docs
  teams-transcript-bot-prompt.md   # Full design spec
.env.example
README.md
```

---

## Testing checklist

- [ ] Bot joins a test meeting and is visible as a participant.
- [ ] Live transcription is enabled and Graph webhook fires on new chunks.
- [ ] Buffer flush creates a new folder + `.docx` on first write.
- [ ] Subsequent flushes append rather than overwrite.
- [ ] Document is readable and correctly formatted (speaker, timestamp, text) after a full meeting.
- [ ] Bot handles `callEnded` — performs final flush and finalizes the document.
- [ ] Errors during a flush don't drop buffered transcript data (retry logic).

---

## Known constraints / things to verify against current docs

- `OnlineMeetingTranscript.Read.All` and live transcript webhook availability are
  gated by tenant licensing and admin policy — confirm current requirements in
  [Microsoft Graph docs](https://learn.microsoft.com/graph/api/resources/calltranscript)
  before relying on this in production, as this API surface changes.
- Graph delivers transcript **chunks**, not a strict timer — the 1–2 min cadence
  is enforced by this app's flush timer, not by Graph itself.

---

## License

TBD.
