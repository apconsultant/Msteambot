# Teams Transcript Bot Design Spec

This file is a placeholder for the full design specification referenced by the project README.

## Planned architecture
- Bot joins Teams meetings through Microsoft Graph calling APIs.
- Live transcript chunks arrive through Graph change notifications.
- App buffers transcript content by meeting ID and flushes to SharePoint periodically.
- Each meeting gets its own folder and `.docx` document.
