# FlowDesk

FlowDesk is a clean, native desktop productivity application built with Avalonia UI. It is designed to be a personal, local-first workspace to manage your projects, tasks, documents, and files seamlessly without requiring an internet connection.

## Current Version: v1.4 Beta (Local Workspace & LAN Collaboration)
FlowDesk is currently in its v1.4 Beta stage, introducing local networking and team collaboration.

### Features
- **Private & Local Workspaces**: Work solo in a Private Workspace or open your workspace to your local network (LAN).
- **Network Auto-Discovery**: Automatically discover active Local Workspaces on your Wi-Fi or LAN via UDP Broadcast without needing to share IP addresses.
- **Real-Time Collaboration**: Changes to projects, tasks, and documents sync instantly to all connected team members via SignalR.
- **Projects & Tasks**: Organize your work into distinct projects, track status, priority, and link tasks.
- **Docs**: A clean markdown-based document editor with auto-save functionality.
- **Files**: Securely import and organize reference files locally within your workspace.
- **Member Management**: Accept or reject join requests directly from the app. Monitor who is online.
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
- The current v1.4 version supports Private and Local Workspaces over LAN. 
- Server Workspace (Cloud Sync), Advanced Permissions, Comments/Mentions are planned future features and are not available in this release.
- Rich text features in Docs are currently limited to basic markdown representation.

## Roadmap Summary
- **v1.0**: First public-ready release (Private Workspace)
- **v1.5**: Full Local Workspace Polish (Current Stage)
- **v2.0**: Server Workspace (Self-hosted ASP.NET Core backend)

---
*FlowDesk — Your clean, offline productivity haven.*
