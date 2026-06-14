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

---

# FlowDesk (Bahasa Indonesia)

FlowDesk adalah aplikasi produktivitas desktop asli (*native*) yang rapi dan bersih, dibangun menggunakan Avalonia UI. Aplikasi ini dirancang sebagai ruang kerja pribadi yang mengutamakan kelokalan (*local-first*) untuk mengelola proyek, tugas, dokumen, dan file Anda dengan mulus tanpa memerlukan koneksi internet.

## Versi Saat Ini: v1.4 Beta (Local Workspace & Kolaborasi LAN)
FlowDesk saat ini berada pada tahap v1.4 Beta, memperkenalkan fitur jaringan lokal dan kolaborasi tim.

### Fitur
- **Private & Local Workspaces**: Bekerja sendirian di ruang kerja pribadi (Private) atau buka ruang kerja Anda untuk jaringan lokal (LAN).
- **Network Auto-Discovery**: Secara otomatis menemukan ruang kerja lokal (Local Workspaces) yang aktif di Wi-Fi atau LAN Anda menggunakan *UDP Broadcast* tanpa perlu membagikan alamat IP secara manual.
- **Kolaborasi Real-Time**: Perubahan pada proyek, tugas, dan dokumen disinkronkan secara instan ke semua anggota tim yang terhubung melalui SignalR.
- **Proyek & Tugas**: Atur pekerjaan Anda ke dalam proyek yang berbeda, lacak status, prioritas, dan tautkan tugas.
- **Dokumen (Docs)**: Editor dokumen berbasis *markdown* yang rapi dengan fungsi simpan otomatis (*auto-save*).
- **File**: Impor dan atur file referensi dengan aman di dalam ruang kerja Anda secara lokal.
- **Manajemen Anggota**: Terima atau tolak permintaan bergabung langsung dari dalam aplikasi. Pantau siapa saja yang sedang *online*.
- **Mode Gelap/Terang**: Dukungan penuh untuk tema gelap dan terang bawaan sistem.
- **Arsitektur Local-First**: Data Anda tidak pernah meninggalkan perangkat Anda. Semuanya disimpan secara lokal menggunakan SQLite.
- **Pencadangan Otomatis**: Pencadangan database otomatis setiap hari untuk memastikan keamanan data Anda.

## Instalasi & Penggunaan
Saat ini, FlowDesk dibangun terutama untuk macOS, tetapi 100% kompatibel dengan Windows dan Linux melalui .NET 10.0.

### Persyaratan
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Menjalankan Secara Lokal
```bash
git clone <repository-url>
cd flowdesk/apps/FlowDesk.Desktop
dotnet run
```

### Membangun (Build) untuk Rilis
Untuk membangun *executable* mandiri khusus macOS:
```bash
dotnet publish -c Release -r osx-x64 --self-contained
```

## Penyimpanan Data & Privasi
**FlowDesk adalah aplikasi Local-First.**
- **Database**: Semua data ruang kerja, proyek, dan tugas Anda disimpan di `FlowDeskData/flowdesk.db` (SQLite).
- **File**: File yang diimpor disalin dengan aman dan disimpan di dalam direktori `FlowDeskData/files/`.
- **Log & Cadangan**: Log aplikasi disimpan di `FlowDeskData/logs/`, dan file cadangan (*backup*) otomatis disimpan di `FlowDeskData/backups/auto/`.

Data Anda sepenuhnya bersifat pribadi dan milik Anda seutuhnya. Kami tidak mengumpulkan analitik, telemetri, atau mengunggah data Anda ke server *cloud* mana pun.

## Batasan Saat Ini
- Versi v1.4 saat ini hanya mendukung *Private Workspace* dan *Local Workspace* melalui LAN. 
- *Server Workspace* (Sinkronisasi *Cloud*), Izin Akses Tingkat Lanjut (*Advanced Permissions*), dan fitur Komentar/Sebutan (*Mentions*) adalah fitur yang direncanakan di masa depan dan belum tersedia pada rilis ini.
- Fitur *rich text* pada Dokumen saat ini terbatas pada representasi *markdown* dasar.

## Ringkasan Peta Jalan (Roadmap)
- **v1.0**: Rilis perdana untuk publik (Private Workspace)
- **v1.5**: Penyempurnaan penuh *Local Workspace* (Tahap Saat Ini)
- **v2.0**: *Server Workspace* (Backend ASP.NET Core yang di-*host* sendiri)

---
*FlowDesk — Tempat produktivitas offline Anda yang bersih dan rapi.*
