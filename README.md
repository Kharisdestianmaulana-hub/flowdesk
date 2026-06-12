# FlowDesk

FlowDesk is a clean, native desktop productivity application built with Avalonia UI. It is designed to be a personal, local-first workspace to manage your projects, tasks, documents, and files seamlessly without requiring an internet connection.

## Current Version: v1.0.0 (Public Release)
FlowDesk is currently in its v1.0 Public Release stage (Private Workspace only).

### Features (Private Workspace)
- **Projects**: Organize your work into distinct projects, track status and priority.
- **Tasks**: Quick add tasks, track to-dos, and link them to projects.
- **Docs**: A clean markdown-based document editor with auto-save functionality.
- **Files**: Securely import and organize reference files locally within your workspace.
- **Requests**: Track incoming feature requests, bugs, and feedback.
- **Dark/Light Mode**: Full support for system-integrated dark and light themes.
- **Local-First Architecture**: Your data never leaves your machine. Everything is stored locally via SQLite.
- **Automatic Backups**: Daily automatic backups of your database to ensure data safety.

## Installation & Usage
Currently, FlowDesk is built primarily for macOS, but it is 100% compatible with Windows and Linux via .NET 10.0.

### Requirements
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Running Locally
```bash
git clone <repository-url>
cd flowdesk/apps/FlowDesk.Desktop
dotnet run
```

### Building for Release
To build a standalone executable for macOS:
```bash
dotnet publish -c Release -r osx-x64 --self-contained
```

## Data Storage & Privacy
**FlowDesk is a Local-First application.**
- **Database**: All your workspace data, projects, and tasks are stored in `FlowDeskData/flowdesk.db` (SQLite).
- **Files**: Any imported files are securely copied and stored in the `FlowDeskData/files/` directory.
- **Logs & Backups**: Application logs are stored in `FlowDeskData/logs/`, and automatic database backups are kept in `FlowDeskData/backups/auto/`.

Your data is entirely private and belongs to you. We do not collect analytics, telemetry, or upload your data to any cloud servers.

## Known Limitations
- The current v1.0 version is strictly a "Private Workspace". 
- Local Workspace, Server Workspace, Realtime Collaboration, Advanced Permissions, Comments/Mentions, and Team Sync are planned future features and are not available in this release.
- Rich text features in Docs are currently limited to basic markdown representation.

## Roadmap Summary
- **v1.0**: First public-ready release (Private Workspace)
- **v1.5**: Local Workspace (LAN/Offline collaboration support)
- **v2.0**: Server Workspace (Self-hosted ASP.NET Core backend)

---
*FlowDesk — Your clean, offline productivity haven.*
