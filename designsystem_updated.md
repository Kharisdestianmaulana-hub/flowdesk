# FlowDesk Design System

## Overview

FlowDesk is a professional desktop workspace for teams, small companies, agencies, studios, schools, organizations, and internal business groups to manage projects, tasks, documents, files, requests, and collaboration in one place.

The interface should feel like a **serious native desktop application**: calm, precise, trustworthy, and built for real work. FlowDesk is not a playful productivity app, not a colorful SaaS dashboard, and not a web page wrapped inside a desktop window. It should feel closer to polished company-grade software: clean sidebars, quiet panels, readable tables, focused dialogs, and predictable navigation.

The visual language is based on **near-invisible UI around meaningful work content**. The app should not compete with the user's projects, documents, tasks, and files. It should frame them with structure, hierarchy, and restraint.

FlowDesk uses neutral surfaces, subtle borders, professional typography, compact spacing, and a single primary accent color. Color should communicate interaction, status, ownership, or priority. It should never be used as decoration.

The application should look modern, but not trendy. It should age well.

**Key Characteristics:**

- Native desktop-first workspace, not a webview-style dashboard.
- Calm neutral color system with one primary blue accent.
- Low visual noise: subtle dividers, soft surfaces, minimal shadows.
- Serious professional tone suitable for companies and teams.
- Modular workspace layout: sidebar, toolbar, main content, optional inspector panel.
- Dense enough for work, spacious enough to avoid feeling cramped.
- Tables, boards, forms, documents, and file views share one consistent design language.
- No decorative gradients, no glowing cards, no cartoon illustrations, no AI-template look.
- Interaction states are clear but quiet.
- UI hierarchy comes from spacing, type, and structure before color.

---

## Product Design Direction

FlowDesk is a **modular team workspace**.

It must support different types of teams without feeling specific to one industry.

Example team types:

- Software teams
- Game studios
- Web agencies
- Creative agencies
- Marketing teams
- Event organizers
- Schools and student teams
- Small companies
- Internal business departments
- Freelancers with clients
- Community teams

FlowDesk should not visually depend on any single use case. Game studios can use it, but the design must also feel natural for a small company managing internal requests, files, docs, and project tasks.

### Core Product Objects

The interface revolves around these objects:

```txt
Workspace
Project
Task
Document
File / Resource
Request
Member
Team
Comment
Activity
Template
```

The UI should always make the active object clear.

Example:

```txt
Workspace → Project → Task
Workspace → Docs → Document
Workspace → Requests → Request Detail
Workspace → Files → File Preview
```

---

---

## Product Modes

FlowDesk is one native desktop application with three workspace modes. The design must make these modes easy to understand without making the product feel technical or fragmented.

```txt
FlowDesk Desktop
├─ Private Workspace
├─ Local Workspace
└─ Server Workspace
```

The visual system, navigation, and core workspace experience stay consistent across all modes. The mode changes where data lives, how collaboration works, and how users join.

### Private Workspace

Private Workspace is for users who want to use FlowDesk only on one device.

Use cases:
```txt
Solo work
Freelance work
Personal business
Student project
Private company planning
```

Behavior:
```txt
Data is stored on this device.
No server is required.
No internet is required.
No member invite is required.
User creates a local profile, not an online account.
```

UI copy:
```txt
Use FlowDesk only on this device.
Best for private work and solo projects.
```

### Local Workspace

Local Workspace is for teams working in the same office, studio, school, lab, building, or local network. One desktop device becomes the host. Other devices join through the same LAN/Wi-Fi network.

Use cases:
```txt
Small office
School lab
Studio room
Company floor
Local agency team
Team in the same building
```

Behavior:
```txt
One device hosts the workspace.
Other FlowDesk desktop apps connect to the host.
No VPS or cloud server is required.
Devices must be able to reach each other on the same local network.
The host device must stay on while others use the workspace.
```

UI copy:
```txt
Share FlowDesk with people on the same office network.
Best for teams working in one place.
```

### Server Workspace

Server Workspace is for companies and teams that install FlowDesk Server on a VPS, cloud server, or internal company server.

Use cases:
```txt
Remote teams
Companies
Organizations
Multiple branches
Self-hosted business workspace
```

Behavior:
```txt
FlowDesk Server stores workspace data.
Users connect from FlowDesk Desktop.
PostgreSQL and server file storage are used.
Server admin controls accounts, roles, backup, and workspace settings.
Can be accessed outside the office depending on company network rules.
```

UI copy:
```txt
Connect to a self-hosted FlowDesk server.
Best for companies and remote teams.
```

### Mode Comparison

| Capability | Private | Local | Server |
|---|---:|---:|---:|
| Use on one device | Yes | Yes | Yes |
| Multi-user | No | Yes | Yes |
| Same-network collaboration | No | Yes | Yes |
| Remote collaboration | No | No | Yes |
| Requires VPS/cloud | No | No | Yes |
| Requires internet | No | No, LAN only | Usually yes |
| Online member status | No | Yes | Yes |
| Invite members | No | Yes | Yes |
| Role permissions | No/basic | Basic | Advanced |
| File sharing | Local only | Local network | Server storage |
| Backup | Local backup | Host backup | Server backup |
| Guest/client access | No | Limited later | Yes later |

---

## Onboarding Flow

FlowDesk should not force every user to create an online account first. The correct onboarding order is:

```txt
Open FlowDesk
↓
Choose workspace mode
↓
Create local profile or login depending on mode
↓
Create or join workspace
↓
Enter dashboard
```

This matters because Private Workspace and Local Workspace do not need a cloud account.

### First Launch Screen

Title:
```txt
Welcome to FlowDesk
```

Subtitle:
```txt
Choose how you want to use FlowDesk.
```

Mode cards:
```txt
Private Workspace
Use FlowDesk only on this device.

Local Workspace
Share FlowDesk with people on the same office network.

Server Workspace
Connect to a self-hosted company server.
```

Design rules:
- Present the three modes as equal choices.
- Do not make Server Workspace look like the only professional option.
- Avoid technical words like SQLite, LAN host, or PostgreSQL on the first screen.
- Use short explanatory copy.
- Use calm cards, not colorful marketing tiles.

### Private Workspace Flow

```txt
Open FlowDesk
↓
Choose Private Workspace
↓
Create local profile
↓
Create workspace
↓
Enter dashboard
```

Fields:
```txt
Your name
Workspace name
Workspace type optional
```

No email or password is required by default.

### Local Workspace Flow

Local Workspace has two paths.

#### Start Local Workspace

```txt
Open FlowDesk
↓
Choose Local Workspace
↓
Start a Local Workspace
↓
Create local profile
↓
Create workspace
↓
Start sharing
↓
Enter dashboard
```

After sharing starts, show:
```txt
Workspace address
Invite code
QR code
Connected devices
Stop sharing button
```

#### Join Local Workspace

```txt
Open FlowDesk
↓
Choose Local Workspace
↓
Join a Local Workspace
↓
Enter invite code / IP address / scan QR
↓
Create display name
↓
Wait for host approval
↓
Enter dashboard
```

Waiting state copy:
```txt
Waiting for workspace owner approval.
Keep this window open.
```

### Server Workspace Flow

```txt
Open FlowDesk
↓
Choose Server Workspace
↓
Enter server URL
↓
Login or accept invite
↓
Choose workspace
↓
Enter dashboard
```

Fields:
```txt
Server URL
Email
Password
```

---

## Workspace Setup Screens

FlowDesk needs dedicated setup screens for each mode. These screens should be simple, quiet, and confidence-building.

### Choose Mode Screen

Purpose: help the user understand the three ways to use FlowDesk.

Layout:
```txt
Header
Mode cards
Secondary help link
```

Rules:
- Use three equal cards.
- Do not overload with technical details.
- Keep icons monochrome.
- Use one primary blue action only after a card is selected.

### Create Private Workspace Screen

Fields:
```txt
Your name
Workspace name
Workspace type
```

Optional workspace types:
```txt
General Team
Company
Agency
Studio
School
Software Team
Custom
```

Primary action:
```txt
Create Workspace
```

### Start Local Workspace Screen

Fields:
```txt
Your name
Workspace name
Workspace type
Host device name
```

After creation:
```txt
Local Workspace is running
Address: 192.168.x.x:port
Invite Code: FD-0000-0000
Show QR Code
Copy Invite
Stop Sharing
```

Rules:
- Explain that the host device must stay on.
- Keep this explanation calm and short.
- Do not show scary network warnings unless connection fails.

### Join Local Workspace Screen

Methods:
```txt
Auto-detected local workspaces
Invite code
Manual address
QR code
```

States:
```txt
Searching local network
Workspace found
Waiting for approval
Access denied
Could not connect
```

### Connect to Server Screen

Fields:
```txt
Server URL
Email
Password
```

Actions:
```txt
Connect
Use invite link
Test connection
```

States:
```txt
Checking server
Server found
Invalid server URL
Cannot reach server
Login failed
```

---

## Mode Indicator

Users should always know which type of workspace they are currently using. The mode indicator should be visible but quiet.

Recommended placement:
```txt
Workspace switcher area in sidebar
or bottom-left status area
```

Examples:
```txt
Lokara Studio
Private

Lokara Studio
Local · 3 online

PT Maju Digital
Server · flowdesk.company.com
```

Rules:
- Use small text.
- Use muted color.
- Do not use large warning badges.
- Show connection status only when relevant.
- Do not make the mode visually dominate the workspace.

### Mode Indicator States

| Mode | Example |
|---|---|
| Private | `Private` |
| Local host | `Local Host · 3 online` |
| Local client | `Local · Connected` |
| Local reconnecting | `Local · Reconnecting…` |
| Server connected | `Server · Connected` |
| Server offline | `Server · Offline` |
| Server syncing | `Server · Syncing…` |

---

## Presence & Collaboration UI

Presence appears in Local Workspace and Server Workspace. Presence tells users who is currently in the workspace and what they are doing without becoming intrusive.

### Member Status

Statuses:
```txt
Online
Away
Busy
Do Not Disturb
Offline
Last seen
```

Recommended avatar indicators:

| Status | Treatment |
|---|---|
| Online | small green dot |
| Away | small yellow dot |
| Busy | small orange/red dot |
| Do Not Disturb | small red dot with minus |
| Offline | gray dot |
| Last seen | muted timestamp text |

### Current Activity

FlowDesk may show lightweight current activity.

Examples:
```txt
Kharis is viewing Tasks
Raka is editing a task
Sinta is uploading a file
Dimas is viewing Project Settings
```

Privacy rule:

Users must be able to disable detailed activity.

If detailed activity is off, show only:
```txt
Kharis is online
```

Do not show sensitive views like billing, private docs, or admin settings as detailed activity unless explicitly allowed.

### Presence Placement

Presence can appear in:
```txt
Workspace header
Project header
Member sidebar section
Inspector panel
Command palette search results later
```

Rules:
- Show only a small number of active members in the header.
- Use overflow for more members.
- Do not clutter the toolbar with many avatars.
- Clicking the presence area can open the member popover.

### Editing Awareness

For docs, tasks, and requests, show when another member is editing.

Examples:
```txt
Raka is editing this task.
Sinta is currently viewing this document.
This document was updated by Dimas. Reload changes?
```

Early versions do not need true live document collaboration. Use editing awareness and conflict prevention first.

---

## Connection States

Local and Server modes need clear connection states. Connection states must be calm, specific, and actionable.

### Local Workspace States

#### Local Host Running

```txt
Local Workspace is running.
People on the same network can join using the invite code.
```

Actions:
```txt
Copy Invite
Show QR Code
Stop Sharing
```

#### Searching Local Network

```txt
Searching for local workspaces…
```

Secondary copy:
```txt
Make sure you are connected to the same network.
```

#### Waiting for Approval

```txt
Waiting for workspace owner approval.
```

Secondary copy:
```txt
You will enter the workspace once the owner approves your request.
```

#### Host Offline

```txt
The local workspace host is offline.
```

Secondary copy:
```txt
Ask the host to open FlowDesk and start Local Workspace again.
```

#### Cannot Reach Host

```txt
Cannot reach this local workspace.
```

Suggested actions:
```txt
Check the address.
Make sure both devices are on the same network.
Try entering the address manually.
```

### Server Workspace States

#### Connecting

```txt
Connecting to server…
```

#### Server Connected

```txt
Connected to FlowDesk Server.
```

#### Server Unavailable

```txt
FlowDesk cannot reach the server.
```

Secondary copy:
```txt
Check your connection or contact your workspace admin.
```

#### Session Expired

```txt
Your session has expired.
```

Action:
```txt
Sign in again
```

#### Syncing

```txt
Syncing changes…
```

#### Sync Complete

```txt
All changes are up to date.
```

#### Sync Conflict

```txt
This item was changed somewhere else.
```

Actions:
```txt
Review changes
Keep mine
Use latest
```

### Connection UI Rules

- Do not use aggressive red unless data loss or destructive failure is involved.
- Use warning color for recoverable connection issues.
- Use concise explanations.
- Always provide the next possible action.
- Keep connection banners dismissible when safe.
- Critical connection states should also appear in the workspace mode indicator.

---

## Dashboard Flow

After setup, every mode enters the same core dashboard structure.

```txt
Home
Projects
Tasks
Docs
Files
Requests
Members
Settings
```

Private mode may show Members as a simple profile/settings area.

Local and Server mode show Members as collaborative workspace members.

### Home Dashboard

Purpose: give the user an immediate overview of work.

Contains:
```txt
My Tasks
Active Projects
Pending Requests
Recent Docs
Recent Files
Activity
Online Members if Local/Server
```

Private mode:
```txt
No online members section.
Activity shows only local activity.
```

Local mode:
```txt
Show online members.
Show host status if current device is host.
```

Server mode:
```txt
Show online members.
Show server status only if connection has issues.
```

---

## Host Admin UI

Local Workspace requires a lightweight host admin area.

Located in:
```txt
Settings → Local Workspace
```

Controls:
```txt
Start sharing
Stop sharing
Copy invite
Show QR code
Change invite code
Approve join request
Remove device/member
View connected devices
Backup workspace
```

Connected device fields:
```txt
Device name
Member name
IP address
Role
Connected since
Last activity
```

Rules:
- Keep IP address visible but secondary.
- Show connected devices only to Owner/Admin.
- Removing a device should ask for confirmation.

---

## Server Admin UI

Server Workspace requires a more complete admin area.

Located in:
```txt
Settings → Administration
```

Sections:
```txt
Workspace settings
Members
Roles & permissions
Invites
Storage usage
Backup status
Audit log
Server connection
```

Rules:
- Server administration should feel serious and controlled.
- Dangerous actions must be separated.
- Show audit information clearly.
- Do not hide backup status.

## Design Principles

### 1. Native, Not Webby

FlowDesk must feel like a real desktop application.

Use desktop-native patterns:

- Sidebar navigation
- Split panes
- Toolbar actions
- Context menus
- Keyboard shortcuts
- Command palette
- Modal dialogs
- Inspector panels
- Compact tables
- Status bars
- Resizable columns
- Right-click actions
- Drag-and-drop where useful

Avoid website-like patterns:

- Huge hero sections
- Landing-page cards
- Oversized marketing typography
- Decorative illustrations
- Gradient backgrounds
- Floating glassmorphism everywhere
- Excessive shadows
- Over-animated page transitions

### 2. Quiet Professionalism

The UI should feel calm and reliable.

FlowDesk should look appropriate in a workplace environment. A company should be able to open it during a meeting without it feeling childish, experimental, or visually distracting.

### 3. Work Content First

The user’s work is the product.

The UI should support:

- project clarity
- task ownership
- document readability
- file organization
- request tracking
- team collaboration

Do not make the visual system more important than the workspace content.

### 4. Functional Color

Color is used for:

- primary actions
- selected navigation
- focus states
- status
- priority
- errors
- warnings
- success states
- avatars and project identity, when needed

Color is not used for decoration.

### 5. Structured Density

FlowDesk must handle many items: tasks, files, docs, projects, requests, comments, and members.

The UI should be readable at scale. Tables and lists should be compact, but not cramped.

### 6. Predictable Layout

Users should always know:

- where they are
- what workspace is active
- what project is active
- what item is selected
- what action is primary
- where details appear

No screen should feel like a different application.

---

## Colors

### Brand & Accent

| Token | Hex | Use |
|---|---:|---|
| `colors.primary` | `#0066CC` | Primary actions, selected states, links |
| `colors.primary.hover` | `#005BB8` | Hover state for primary actions |
| `colors.primary.pressed` | `#004A99` | Pressed primary action |
| `colors.primary.subtle` | `#EAF3FF` | Selected sidebar item, soft active background |
| `colors.primary.soft` | `#F3F8FF` | Very soft accent surface |
| `colors.focus` | `#0071E3` | Keyboard focus ring |

**Accent philosophy:**  
FlowDesk uses one primary blue. Do not introduce extra brand colors. If a second color appears, it must represent status, priority, warning, or semantic meaning.

### Light Surfaces

| Token | Hex | Use |
|---|---:|---|
| `colors.canvas` | `#F5F5F7` | Main app background |
| `colors.window` | `#F8F8FA` | Window chrome, toolbar background |
| `colors.sidebar` | `#F2F2F4` | Sidebar background |
| `colors.surface` | `#FFFFFF` | Main panels, cards, dialogs |
| `colors.surface.subtle` | `#FAFAFB` | Secondary panels, inactive areas |
| `colors.surface.muted` | `#F0F0F2` | Hover surfaces, grouped controls |
| `colors.surface.elevated` | `#FFFFFF` | Popovers, command palette, modal surfaces |

### Text

| Token | Hex | Use |
|---|---:|---|
| `colors.ink` | `#1D1D1F` | Primary headings |
| `colors.text` | `#2C2C2E` | Default text |
| `colors.text.muted` | `#6E6E73` | Metadata, secondary text |
| `colors.text.subtle` | `#8E8E93` | Placeholders, quiet labels |
| `colors.text.disabled` | `#A1A1A6` | Disabled text |
| `colors.text.inverse` | `#FFFFFF` | Text on dark/primary surfaces |

### Borders & Dividers

| Token | Hex | Use |
|---|---:|---|
| `colors.border` | `#D8D8DC` | Standard border |
| `colors.border.soft` | `#E1E1E4` | Card and panel border |
| `colors.border.subtle` | `#ECECEF` | Very soft separator |
| `colors.divider` | `#D1D1D6` | Strong section divider |
| `colors.separator` | `#E5E5EA` | List and table row separators |

### Status Colors

| Token | Hex | Use |
|---|---:|---|
| `colors.success` | `#248A3D` | Done, approved, healthy |
| `colors.success.subtle` | `#EAF7EE` | Success background |
| `colors.warning` | `#B76E00` | Needs attention |
| `colors.warning.subtle` | `#FFF4DE` | Warning background |
| `colors.danger` | `#D70015` | Error, blocked, destructive |
| `colors.danger.subtle` | `#FFE8EA` | Danger background |
| `colors.info` | `#0066CC` | Info state |
| `colors.info.subtle` | `#EAF3FF` | Info background |
| `colors.neutral` | `#6E6E73` | Draft, archived, inactive |
| `colors.neutral.subtle` | `#F0F0F2` | Neutral background |

### Priority Colors

| Priority | Token | Hex |
|---|---|---:|
| Low | `priority.low` | `#6E6E73` |
| Medium | `priority.medium` | `#B76E00` |
| High | `priority.high` | `#D70015` |
| Urgent | `priority.urgent` | `#8E1F1F` |

### Dark Mode

Dark mode should feel professional and low-glare. Avoid pure black except for overlays.

| Token | Hex | Use |
|---|---:|---|
| `dark.canvas` | `#1C1C1E` | App background |
| `dark.window` | `#202022` | Window chrome |
| `dark.sidebar` | `#202022` | Sidebar background |
| `dark.surface` | `#242426` | Main panels |
| `dark.surface.subtle` | `#2C2C2E` | Secondary panels |
| `dark.surface.elevated` | `#303033` | Popovers and dialogs |
| `dark.border` | `#3A3A3C` | Standard border |
| `dark.border.subtle` | `#2F2F32` | Soft separator |
| `dark.text` | `#F5F5F7` | Primary text |
| `dark.text.muted` | `#A1A1A6` | Secondary text |
| `dark.text.subtle` | `#7C7C80` | Metadata |
| `dark.primary` | `#2997FF` | Accent on dark surfaces |
| `dark.primary.subtle` | `#102A43` | Selected item background |

### Color Rules

- Use `colors.primary` for the main action only.
- Use neutral backgrounds for most UI.
- Use status colors only where status matters.
- Do not use random bright colors for cards.
- Do not use gradients.
- Do not use glass effects except for optional platform-native window blur if implemented carefully.
- Do not rely on color alone to communicate status. Use labels/icons too.

---

## Typography

### Font Family

FlowDesk uses system-native fonts.

| Platform | Font |
|---|---|
| macOS | SF Pro / system font |
| Windows | Segoe UI |
| Linux | Inter, Noto Sans, or system font |

Fallback stack:

```txt
system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", Inter, "Noto Sans", sans-serif
```

Do not bundle proprietary Apple fonts.

### Type Scale

| Token | Size | Weight | Line Height | Use |
|---|---:|---:|---:|---|
| `typography.display` | 32px | 600 | 1.15 | Empty states, onboarding title |
| `typography.page-title` | 26px | 600 | 1.2 | Main page title |
| `typography.title` | 22px | 600 | 1.25 | Project/document title |
| `typography.section-title` | 17px | 600 | 1.3 | Section headings |
| `typography.body` | 14px | 400 | 1.45 | Default UI text |
| `typography.body-strong` | 14px | 600 | 1.45 | Emphasized body text |
| `typography.ui` | 13px | 400 | 1.35 | Compact UI text |
| `typography.ui-strong` | 13px | 600 | 1.35 | Compact strong UI |
| `typography.caption` | 12px | 400 | 1.35 | Metadata, helper text |
| `typography.caption-strong` | 12px | 600 | 1.35 | Strong metadata |
| `typography.micro` | 11px | 400 | 1.25 | Small labels and badges |
| `typography.button` | 13px | 500 | 1.2 | Button labels |
| `typography.sidebar` | 13px | 500 | 1.2 | Sidebar navigation |
| `typography.table` | 13px | 400 | 1.35 | Table cells |
| `typography.table-header` | 12px | 600 | 1.25 | Table headers |
| `typography.code` | 13px | 400 | 1.45 | Code blocks and technical text |

### Typography Principles

- Use 600 for headings. Avoid 700 unless absolutely necessary.
- Default desktop UI text is 14px.
- Dense controls, sidebars, metadata, and tables may use 13px.
- Use 12px for captions and table headers.
- Use muted text for secondary information.
- Avoid oversized headings in work screens.
- Use clear labels. Do not use vague decorative copy.
- Never use playful display fonts.

### Writing Rhythm

FlowDesk copy should be short and practical.

Good:

```txt
Create project
Invite member
Upload file
Submit request
Archive task
Move to review
```

Avoid:

```txt
Let's build something amazing
Supercharge your workflow
Unlock team magic
Your productivity command center
AI-powered everything
```

---

## Layout

### Spacing System

FlowDesk uses a 4px base spacing system.

| Token | Value | Use |
|---|---:|---|
| `spacing.1` | 4px | Micro spacing |
| `spacing.2` | 8px | Icon-label gap, tight groups |
| `spacing.3` | 12px | Compact padding |
| `spacing.4` | 16px | Standard padding |
| `spacing.5` | 20px | Medium gap |
| `spacing.6` | 24px | Panel padding |
| `spacing.8` | 32px | Large layout spacing |
| `spacing.10` | 40px | Major screen spacing |
| `spacing.12` | 48px | Empty state and onboarding sections |

### Desktop Shell

FlowDesk uses a stable desktop shell.

```txt
┌───────────────────────────────────────────────────────────┐
│ Native Title Bar                                           │
├────────────────┬──────────────────────────────────────────┤
│ Sidebar        │ Toolbar                                  │
│                ├───────────────────────────────┬──────────┤
│ Navigation     │ Main Content                  │ Inspector│
│ Workspace      │                               │ Panel    │
│ Projects       │                               │ Optional │
└────────────────┴───────────────────────────────┴──────────┘
```

### Layout Measurements

| Element | Size |
|---|---:|
| Sidebar width | 240px |
| Collapsed sidebar width | 64px |
| Toolbar height | 48px |
| Inspector panel width | 320px |
| Modal small width | 420px |
| Modal medium width | 560px |
| Modal large width | 720px |
| List row compact height | 40px |
| List row comfortable height | 48px |
| Table row compact height | 36px |
| Table row comfortable height | 44px |
| Standard button height | 32px |
| Compact button height | 28px |
| Large button height | 40px |
| Input height | 32px |
| Card padding | 16px / 20px |
| Panel padding | 20px / 24px |

### Container Widths

| Surface | Width Behavior |
|---|---|
| Work views | Full available width |
| Document editor | Max readable width 760px |
| Settings pages | Max width 860px |
| Dialogs | Fixed width based on complexity |
| Tables | Full width with scroll when needed |
| Boards | Horizontal scroll allowed |

### Whitespace Philosophy

FlowDesk is a work application, so whitespace should be practical.

- Use compact spacing inside tables, lists, and sidebars.
- Use more breathing room in document editors, settings pages, and empty states.
- Avoid huge empty SaaS-style sections.
- Avoid cramped dashboards with too many widgets.

---

## Elevation & Depth

FlowDesk uses very limited elevation.

| Level | Treatment | Use |
|---|---|---|
| Flat | No shadow | Sidebar, toolbar, main panels |
| Hairline | 1px border | Cards, inputs, tables |
| Popover | Border + soft shadow | Dropdowns, context menus, command palette |
| Dialog | Border + stronger soft shadow | Modals, confirmations |

### Shadow Tokens

```txt
shadow.none = none
shadow.popover = 0 8px 24px rgba(0, 0, 0, 0.10)
shadow.dialog = 0 18px 48px rgba(0, 0, 0, 0.16)
shadow.dark.popover = 0 8px 24px rgba(0, 0, 0, 0.32)
shadow.dark.dialog = 0 18px 48px rgba(0, 0, 0, 0.42)
```

### Elevation Rules

- Do not put shadows on every card.
- Use borders for structure.
- Use shadows only for floating surfaces.
- Never use glow effects.
- Never use heavy neumorphism.
- Avoid layered cards inside layered cards.

---

## Shapes

### Border Radius Scale

| Token | Value | Use |
|---|---:|---|
| `radius.none` | 0px | Split panes, window edges |
| `radius.xs` | 4px | Badges, tiny chips |
| `radius.sm` | 6px | Inputs, compact controls |
| `radius.md` | 8px | Buttons, selected list rows |
| `radius.lg` | 12px | Cards, popovers |
| `radius.xl` | 16px | Large dialogs |
| `radius.full` | 999px | Pills, avatars |

### Shape Rules

- Desktop UI should not be overly rounded.
- Avoid 24px+ radius except for full pills.
- Use consistent radius per component type.
- Cards and dialogs can be rounded; split panes should stay square.
- Buttons should feel modern but not bubbly.

---

## Icons

### Icon Style

- Monochrome line icons
- 16px for dense UI
- 20px for navigation and toolbar
- 24px only for empty states or large buttons
- Consistent stroke width
- Rounded line caps preferred
- No colorful icons in sidebar navigation

### Icon Color Rules

| Context | Color |
|---|---|
| Default icon | `colors.text.muted` |
| Active icon | `colors.primary` |
| Disabled icon | `colors.text.disabled` |
| Danger icon | `colors.danger` |
| Warning icon | `colors.warning` |
| Success icon | `colors.success` |

Icons should clarify action, not decorate the screen.

---

## Components

### App Shell

The app shell is the persistent layout used across the product.

Parts:

```txt
Native Title Bar
Sidebar
Toolbar
Main Content
Optional Inspector Panel
Status Bar
```

Rules:

- The shell must remain stable between screens.
- The sidebar should not jump or resize unexpectedly.
- Toolbar actions must change based on current screen.
- Inspector panel should be optional and dismissible.
- Main content should be scrollable independently.

---

### Sidebar

The sidebar is the primary navigation layer.

Contains:

- Workspace switcher
- Home
- Projects
- Tasks
- Docs
- Files
- Requests
- Members
- Settings

Style:

| Property | Value |
|---|---|
| Width | 240px |
| Background | `colors.sidebar` |
| Border right | `colors.border.soft` |
| Item height | 32px |
| Item radius | 8px |
| Item padding | 8px 10px |
| Icon size | 18px |
| Text | `typography.sidebar` |

States:

| State | Treatment |
|---|---|
| Default | muted text, transparent background |
| Hover | `colors.surface.muted` |
| Selected | `colors.primary.subtle`, text `colors.primary` |
| Disabled | reduced opacity |

Rules:

- Do not use colorful navigation icons.
- Selected state must be obvious but quiet.
- Workspace switcher should stay at the top.
- Settings should stay near the bottom.
- Collapsed mode may show icons only.

---

### Toolbar

The toolbar contains context-specific actions.

Examples:

- New Project
- New Task
- New Document
- Upload File
- Filter
- Sort
- View Options
- Search

Style:

| Property | Value |
|---|---|
| Height | 48px |
| Background | `colors.window` |
| Border bottom | `colors.border.soft` |
| Padding | 12px 16px |

Rules:

- Use one primary action per screen.
- Place primary action on the right.
- Search can sit left or center depending on module.
- Keep toolbar compact.
- Avoid filling toolbar with too many buttons.

---

### Button

#### Primary Button

Use for the main action.

Examples:

- Create Project
- Save Changes
- Invite Member
- Submit Request

Style:

| Property | Value |
|---|---|
| Height | 32px |
| Padding | 0 14px |
| Radius | 8px |
| Background | `colors.primary` |
| Text | white |
| Font | `typography.button` |

States:

| State | Treatment |
|---|---|
| Hover | `colors.primary.hover` |
| Pressed | `colors.primary.pressed` |
| Focus | 2px focus ring |
| Disabled | muted background, disabled text |

#### Secondary Button

Use for neutral actions.

Style:

- Background: `colors.surface`
- Border: `colors.border`
- Text: `colors.text`
- Height: 32px
- Radius: 8px

#### Ghost Button

Use inside toolbars and panels.

Style:

- Transparent background
- No border
- Hover background: `colors.surface.muted`
- Text: `colors.text`

#### Destructive Button

Use for dangerous actions.

Examples:

- Delete Workspace
- Remove Member
- Delete Project
- Delete File

Rules:

- Use danger color.
- Require confirmation for destructive permanent actions.
- Do not place destructive actions next to common primary actions without spacing.

---

### Input Fields

Inputs should feel compact and native.

Style:

| Property | Value |
|---|---|
| Height | 32px |
| Radius | 6px |
| Background | `colors.surface` |
| Border | `colors.border` |
| Text | `colors.text` |
| Placeholder | `colors.text.subtle` |

States:

| State | Treatment |
|---|---|
| Hover | border slightly stronger |
| Focus | focus border/ring |
| Error | danger border + helper text |
| Disabled | muted surface |

Input types:

- Text field
- Search field
- Text area
- Select
- Date picker
- Tag picker
- Member picker
- File picker

---

### Cards

Cards group related content.

Style:

| Property | Value |
|---|---|
| Background | `colors.surface` |
| Border | `colors.border.subtle` |
| Radius | 12px |
| Padding | 16px or 20px |
| Shadow | none |

Use cards for:

- project overview blocks
- settings groups
- empty states
- workspace summaries
- request summaries

Do not:

- use cards for everything
- nest cards deeply
- apply colorful backgrounds without meaning
- add shadows by default

---

### Tables

Tables are important for company workflows.

Use tables for:

- files
- members
- requests
- tasks list view
- documents list
- project records

Table style:

| Property | Value |
|---|---|
| Header height | 36px |
| Row height | 36px / 44px |
| Header text | `typography.table-header` |
| Cell text | `typography.table` |
| Row separator | `colors.separator` |
| Hover background | `colors.surface.subtle` |
| Selected row | `colors.primary.subtle` |

Rules:

- Keep tables readable.
- Avoid heavy vertical grid lines.
- Allow sorting/filtering later.
- Use muted metadata.
- Use clear empty states.
- Support right-click row actions.

---

### Task Board

The board supports kanban-style workflows.

Default columns:

```txt
Backlog
To Do
In Progress
Review
Done
```

Column style:

| Property | Value |
|---|---|
| Width | 280px |
| Background | `colors.surface.subtle` |
| Border | `colors.border.subtle` |
| Radius | 12px |
| Padding | 12px |

Task card style:

| Property | Value |
|---|---|
| Background | `colors.surface` |
| Border | `colors.border.subtle` |
| Radius | 10px |
| Padding | 12px |
| Shadow | none |

Task card content:

- title
- status
- priority
- assignee avatar
- due date
- project label
- comment/attachment indicators

Rules:

- Avoid colorful column backgrounds.
- Keep badges small.
- Drag feedback should be smooth, not flashy.
- Cards should be compact but readable.

---

### Docs / Editor

The document editor should feel focused and calm.

Editor layout:

```txt
Document title
Metadata row
Editor toolbar
Content area
Optional comments/inspector
```

Rules:

- Keep writing surface white.
- Use readable max width around 760px.
- Minimize editor chrome.
- Title should feel prominent but not huge.
- Toolbar should be compact and sticky only when useful.
- Avoid excessive formatting controls in early versions.

Supported early content:

- headings
- paragraphs
- bold/italic
- links
- bullet lists
- numbered lists
- checklists
- code blocks
- attachments

---

### Files & Resources

Files are shared resources inside a workspace or project.

File list columns:

```txt
Name
Type
Project
Owner
Updated
Size
Tags
```

File detail panel:

```txt
Preview
Metadata
Linked project
Owner
Upload date
Tags
Activity
Download/Open actions
```

Rules:

- Use table/list view by default.
- Support preview where possible.
- Keep file actions clear.
- Do not make file cards overly visual unless in gallery view.

---

### Requests

Requests are structured work intake items.

Examples:

- bug report
- design request
- purchase request
- support request
- client feedback
- leave request
- content request

Request status examples:

```txt
New
Triaged
In Progress
Waiting
Resolved
Closed
```

Request detail should include:

- title
- requester
- status
- priority
- assigned person
- form fields
- attachments
- comments
- activity

Rules:

- Request forms must be clear and practical.
- Required fields must be obvious.
- Submission confirmation should be reassuring.
- Submitted requests should become trackable items.

---

### Modal Dialogs

Use modals for focused actions.

Examples:

- Create Workspace
- Create Project
- Invite Member
- Upload File
- Delete Confirmation
- Workspace Settings

Modal style:

| Property | Value |
|---|---|
| Background | `colors.surface.elevated` |
| Radius | 16px |
| Border | `colors.border.soft` |
| Shadow | `shadow.dialog` |
| Padding | 24px |

Rules:

- Keep modals narrow unless necessary.
- Title must be clear.
- Description should explain consequence if needed.
- Primary action bottom-right.
- Cancel next to primary.
- Destructive confirmation must be explicit.

---

### Popovers & Context Menus

Use for secondary actions.

Examples:

- sort menu
- filter menu
- view options
- right-click actions
- member actions
- file actions

Style:

- Background: `colors.surface.elevated`
- Border: `colors.border.soft`
- Radius: 12px
- Shadow: `shadow.popover`
- Item height: 32px
- Item padding: 8px 12px

Rules:

- Menus should be compact.
- Destructive actions should be separated.
- Keyboard navigation should work.

---

### Command Palette

Command palette is the fastest way to navigate and act.

Shortcuts:

```txt
macOS: Cmd + K
Windows/Linux: Ctrl + K
```

Supports:

- Search projects
- Open documents
- Create task
- Upload file
- Switch workspace
- Invite member
- Open settings
- Search requests
- Search files

Style:

- Width: 640px
- Centered overlay
- Radius: 16px
- Shadow: `shadow.dialog`
- Search input at top
- Results grouped by type

Rules:

- It must feel fast.
- Results should be keyboard-selectable.
- Use small icons and muted metadata.
- Avoid visual clutter.

---

### Toast Notifications

Use for short feedback.

Examples:

- Project created
- File uploaded
- Task moved
- Changes saved
- Invite sent

Style:

- Bottom-right
- Surface elevated
- Border
- Radius 12px
- Minimal icon
- Auto dismiss

Rules:

- Do not overuse.
- Errors may persist until dismissed.
- Keep copy short.

---

### Badges & Chips

Use for status, priority, tags, and roles.

Style:

| Property | Value |
|---|---|
| Height | 20px / 22px |
| Radius | full |
| Font | `typography.micro` |
| Padding | 6px horizontal |

Examples:

```txt
Admin
Viewer
High
Done
Blocked
Internal
Client
```

Rules:

- Badges should be small.
- Avoid too many badges on one card.
- Use semantic colors only.

---

### Avatars

Use avatars for members and assignees.

Rules:

- 24px for compact lists.
- 28px for task cards.
- 32px for panels.
- 40px for profile/detail views.
- Use initials when no image exists.
- Avatar colors should be muted, not neon.

---

## Module Templates

### Workspace Home

Purpose: give a quick overview of current work.

Contains:

- active projects
- my tasks
- pending requests
- recent documents
- recent files
- recent activity

Rules:

- Do not overload with charts.
- Show useful operational data.
- Avoid decorative analytics cards.
- Use clear lists and summaries.

---

### Projects

Purpose: manage company/team projects.

Views:

- grid view
- list view
- status grouped view

Project card content:

- project name
- status
- members
- open tasks
- open requests
- latest activity
- due date

---

### Project Detail

Purpose: central hub for one project.

Tabs/sections:

```txt
Overview
Tasks
Docs
Files
Requests
Activity
Settings
```

Optional later:

```txt
Timeline
Calendar
Reports
```

Rules:

- Project title and status must be visible.
- Primary project action should be clear.
- Recent activity should be easy to scan.

---

### Tasks

Purpose: manage work items.

Views:

```txt
Board
List
Table
Calendar later
```

Task detail contains:

- title
- status
- assignee
- priority
- due date
- description
- attachments
- comments
- activity

Rules:

- Task update flow must be fast.
- Drag-and-drop should not be required for all actions.
- List/table view is important for company workflows.

---

### Docs

Purpose: manage team knowledge.

Views:

```txt
Document list
Editor
Project-linked docs
Recent docs
```

Document metadata:

- title
- owner
- project
- updated date
- tags
- permissions later

---

### Files

Purpose: manage shared resources.

Views:

```txt
List
Table
Gallery later
Preview
```

Rules:

- Files should be searchable.
- Metadata should be visible.
- Linking files to projects/tasks/requests is important.

---

### Requests

Purpose: collect and track structured work intake.

Views:

```txt
Inbox
Board
List
Request detail
Form templates later
```

Common request templates:

- bug report
- design request
- client feedback
- purchase request
- internal support
- general request

---

### Team

Purpose: manage workspace members.

Contains:

- member list
- roles
- invite status
- workspace access
- pending invites

Roles:

```txt
Owner
Admin
Member
Viewer
Guest later
```

---

## Motion

Motion should be fast and functional.

| Token | Duration | Use |
|---|---:|---|
| `motion.fast` | 120ms | Hover, simple opacity |
| `motion.standard` | 180ms | Panel transitions, menus |
| `motion.slow` | 240ms | Dialog entrance |

Easing:

```txt
ease.standard = cubic-bezier(0.2, 0, 0, 1)
ease.out = cubic-bezier(0.16, 1, 0.3, 1)
ease.in = cubic-bezier(0.7, 0, 0.84, 0)
```

Rules:

- Do not use bouncing motion.
- Do not use long transitions.
- Do not animate everything.
- Motion should clarify state change.

---

## Accessibility

Minimum requirements:

- Keyboard navigation
- Visible focus states
- Screen-reader labels where possible
- High contrast text
- No color-only status
- Large enough click targets
- Clear error messages
- Logical tab order
- Support reduced motion

Focus ring:

```txt
2px solid colors.focus
2px offset when possible
```

Minimum interactive target:

```txt
28px compact
32px standard
40px comfortable
```

---

## Platform Adaptation

FlowDesk should keep one design language but respect platform expectations.

### macOS

- Use native title bar behavior.
- Support Command shortcuts.
- UI can feel slightly lighter.
- Sidebar and toolbar should feel calm and integrated.

### Windows

- Use Segoe UI by default.
- Support Ctrl shortcuts.
- Controls should not look overly macOS-only.
- Window chrome should feel natural on Windows.

### Linux

- Use Inter/Noto Sans/system font.
- Keep layout stable.
- Avoid relying on platform-specific blur effects.

---

## Avalonia UI Implementation Notes

FlowDesk will be built with Avalonia UI + .NET.

Recommended project structure:

```txt
FlowDesk/
├─ apps/
│  ├─ FlowDesk.Desktop/
│  └─ FlowDesk.Server/
├─ src/
│  ├─ FlowDesk.Core/
│  ├─ FlowDesk.Infrastructure/
│  ├─ FlowDesk.Shared/
│  └─ FlowDesk.DesignSystem/
├─ docs/
│  └─ designsystem.md
└─ README.md
```

### Resource Naming

Use predictable resource names.

Examples:

```txt
ColorPrimary
ColorCanvas
ColorSurface
ColorText
ColorTextMuted
ColorBorder
ColorDanger
ColorWarning
ColorSuccess
Spacing4
RadiusMedium
ShadowPopover
TypographyBody
```

### Suggested Avalonia Resource Groups

```txt
Resources/
├─ Colors.axaml
├─ Typography.axaml
├─ Spacing.axaml
├─ Controls.axaml
├─ Buttons.axaml
├─ Inputs.axaml
├─ Tables.axaml
├─ Dialogs.axaml
└─ Themes/
   ├─ Light.axaml
   └─ Dark.axaml
```

### Control Styling Priorities

Style these first:

```txt
Button
TextBox
ComboBox
ListBox
TreeView
DataGrid
TabControl
ContextMenu
Flyout
Dialog
SidebarItem
TaskCard
Badge
Avatar
```

### Recommended First Screens

Build these screens first to validate the design system:

```txt
Workspace Home
Project List
Project Detail
Task Board
Task Detail
Docs List
File List
Request Inbox
Settings
```

---

## Do and Don't

### Do

- Use neutral surfaces.
- Keep one primary accent color.
- Make tables and lists readable.
- Use borders before shadows.
- Keep toolbar actions predictable.
- Use compact controls.
- Support keyboard shortcuts.
- Keep copy direct and professional.
- Design for real team workflows.

### Don't

- Do not make it look like a web landing page.
- Do not use flashy gradients.
- Do not use glowing cards.
- Do not make every card colorful.
- Do not use playful mascot illustrations.
- Do not over-round every component.
- Do not use shadows on every surface.
- Do not use fake glassmorphism everywhere.
- Do not make dashboard cards purely decorative.
- Do not make the app feel like an AI-generated template.

---

## Final Direction

FlowDesk should feel like a serious desktop workspace that individuals, local teams, and self-hosted companies can trust every day.

The design language is:

```txt
Native desktop structure
Calm neutral surfaces
Precise spacing
Professional typography
Subtle dividers
Minimal elevation
Functional color
Clear hierarchy
Reliable interaction
```

The intended user reaction is:

```txt
"This app feels organized, serious, fast, and ready for real work."
```

FlowDesk should not impress by being visually loud.  
It should impress by being clear, stable, and useful.
