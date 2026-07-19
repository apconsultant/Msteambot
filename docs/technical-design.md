# Technical Design Document: Teams Live Transcript → SharePoint Bot

## 1. Overview

This solution implements a Microsoft Teams meeting bot that can be added to a Teams meeting, participate in the meeting context, receive live transcript events from Microsoft Graph, and write transcript content into a SharePoint-backed document store.

The current repository is scaffolded as a .NET 8 C# ASP.NET Core application that exposes:
- a Bot Framework webhook for Teams activities,
- a Graph-based meeting service stub,
- an API endpoint for join-meeting orchestration,
- and a notification endpoint for transcript webhook payloads.

The design is intended to support the following business flow:
1. A user adds the bot to a Teams meeting.
2. The bot joins the meeting context and prepares to receive transcript updates.
3. Microsoft Graph delivers transcript chunks through webhook notifications.
4. The application buffers transcript chunks and writes them into a per-meeting document in SharePoint.

---

## 2. Business Goals

- Capture live meeting transcripts in a structured, persistent format.
- Store transcript documents in SharePoint for later retrieval and review.
- Provide a Teams-integrated user experience where the bot can be added to meetings.
- Keep the implementation modular so the routing, Graph integration, buffering, and document generation can evolve independently.

---

## 3. Solution Architecture

### 3.1 High-Level Components

The solution is composed of the following layers:

- Teams Bot Layer
  - Hosts a Bot Framework activity handler.
  - Accepts user and conversation events from Microsoft Teams.
  - Provides the bot entry point via the /api/messages endpoint.

- Application Layer
  - Contains orchestration services and HTTP endpoints for bot actions and webhook events.
  - Handles meeting join requests and incoming notifications.

- Microsoft Graph Integration Layer
  - Authenticates to Microsoft Graph using application credentials.
  - Prepares meeting join actions and interfaces with online meeting/transcript capabilities.

- Buffering and Document Layer
  - Collects transcript chunks by meeting ID.
  - Implements a flush strategy so content can be persisted incrementally.
  - Generates a document representation for the transcript output.

- Storage Layer
  - Intended to write transcript documents into SharePoint Document Libraries using Microsoft Graph.

### 3.2 Runtime Topology

The application runs as an ASP.NET Core web service exposed over HTTPS. In production it should be hosted behind a public endpoint so that:
- Teams can send bot activity callbacks,
- Microsoft Graph webhooks can POST transcript notifications,
- and SharePoint document operations can complete successfully.

A typical deployment model is:
- Azure App Service, Azure Container Apps, or Azure Kubernetes Service,
- with a public HTTPS endpoint and environment variables for secrets.

---

## 4. Key Components

### 4.1 Bot Entry Point

File: [src/Bot/MeetingBot.cs](../src/Bot/MeetingBot.cs)

Responsibilities:
- Handle chat messages sent to the bot.
- Respond to member-added events when the bot is installed into a conversation.
- Provide a simple user-facing interaction model for Teams.

Current implementation status:
- It is a scaffold that sends greeting and help text.
- It is not yet connected to the full meeting participation flow.

### 4.2 Bot Webhook and HTTP Surface

File: [Program.cs](../Program.cs)

Responsibilities:
- Configure services and middleware for the ASP.NET Core host.
- Expose the Teams bot webhook at /api/messages.
- Expose a notification endpoint at /api/notifications for graph-driven event delivery.
- Register the controller-based API surface.

### 4.3 Teams Meeting Service

File: [src/Graph/TeamsMeetingService.cs](../src/Graph/TeamsMeetingService.cs)

Responsibilities:
- Construct a Microsoft Graph client using Azure AD application credentials.
- Provide an entry point for initiating a meeting join flow.
- Prepare a scaffold for future integration with Graph online meeting APIs.

Current implementation status:
- The service compiles successfully.
- It returns a safe message until real app credentials and Graph permissions are configured.

### 4.4 Controller Layer

File: [Controllers/TeamsController.cs](../Controllers/TeamsController.cs)

Responsibilities:
- Expose REST-style actions for meeting-related workflows.
- Accept meeting identifiers and route the request into the Graph meeting service.

### 4.5 Buffering Layer

File: [src/Buffer/TranscriptBuffer.cs](../src/Buffer/TranscriptBuffer.cs)

Responsibilities:
- Accumulate transcript chunks per meeting ID.
- Provide a flush operation so buffered content can be emitted to document generation or persistent storage.

### 4.6 Document Generation Layer

File: [src/DocGen/DocumentGenerator.cs](../src/DocGen/DocumentGenerator.cs)

Responsibilities:
- Generate a text-based document payload for transcript content.
- In the future, this layer can be extended to produce DOCX output or SharePoint-compatible document content.

### 4.7 Webhook Receiver

File: [src/Webhooks/NotificationReceiver.cs](../src/Webhooks/NotificationReceiver.cs)

Responsibilities:
- Receive Graph webhook payloads.
- Route payload data into buffering or downstream processing logic.

---

## 5. Data Flow

### 5.1 Initial Bot Installation

1. A user installs the Teams app into a Teams environment.
2. The bot receives an activity event through the /api/messages endpoint.
3. The bot responds with a welcome or guidance message.

### 5.2 Meeting Join Request

1. The application receives a join-meeting request via the controller endpoint.
2. The TeamsMeetingService prepares to invoke Graph meeting capabilities.
3. The app uses Azure AD credentials to authenticate to Microsoft Graph.

### 5.3 Transcript Event Handling

1. Microsoft Graph sends notification payloads to /api/notifications.
2. The NotificationReceiver receives the payload.
3. The payload is routed into the buffering process.
4. A flush cycle writes the aggregated transcript content to a document target.

---

## 6. Security and Identity Model

The application relies on Microsoft identity and app registration settings:
- MicrosoftAppId and MicrosoftAppPassword for the Teams bot authentication model,
- AAD_APP_CLIENT_ID, AAD_APP_CLIENT_SECRET, and AAD_APP_TENANT_ID for Microsoft Graph access.

Recommended practices:
- Store secrets in Azure Key Vault or environment variables.
- Avoid embedding secrets in source code.
- Restrict app permissions to the minimum required.
- Use certificate-based auth in production where possible.

---

## 7. Dependencies

The current implementation uses the following major SDKs and frameworks:

- ASP.NET Core 8
- Bot Framework SDK for Teams-compatible activity handling
- Microsoft Graph SDK for meeting-related operations
- Azure Identity SDK for token acquisition

These dependencies support the core goals of Teams integration and Graph-based meeting/transcript access.

---

## 8. Deployment Considerations

### 8.1 Prerequisites

To deploy this solution successfully, the following components are required:
- Microsoft 365 tenant with Teams enabled,
- Azure Bot registration,
- Microsoft Entra app registration,
- Microsoft Graph permissions for meeting and transcript access,
- a public HTTPS endpoint for bot and webhook callbacks,
- and a SharePoint site/document library target.

### 8.2 Recommended Hosting Options

- Azure App Service for simple web hosting,
- Azure Container Apps for containerized deployment,
- or AKS if enterprise scale and governance are required.

### 8.3 Public Endpoint Requirement

Because Teams and Graph webhooks require external reachability, the service should be reachable over a public HTTPS endpoint. Tools such as ngrok or Dev Tunnels can be used during development.

---

## 9. Current Maturity and Next Steps

### Current State
- The project is a working C# scaffold.
- It builds successfully.
- It includes Teams bot route handling and placeholder integrations for meeting and transcript flows.

### Recommended Next Steps
1. Replace the current placeholder meeting join logic with production-grade Graph implementation.
2. Implement webhook payload parsing and transcript chunk aggregation.
3. Add a timer-based or event-driven flush mechanism.
4. Implement SharePoint document creation and incremental append behavior.
5. Package and sideload the Teams app manifest using real bot IDs and endpoints.
6. Add logging, retry policies, and monitoring.

---

## 10. Architecture Summary

This project is a C#-based Teams meeting bot scaffold designed to eventually perform the following end-to-end process:
- join or prepare to participate in a Teams meeting,
- receive Microsoft Graph transcript updates,
- buffer transcript content per meeting,
- and persist the transcript into SharePoint.

In its current form, it is an implementation foundation rather than a fully production-ready transcript capture system. It is suitable for demonstrating the overall architecture and for validating the bot and webhook integration path with a solution architect.
