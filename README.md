# 🌙 Walter Life

A GitHub-inspired dark-mode life dashboard built with **C#**, **Blazor Server**, and **JSON file storage**.

## Features
- Overview dashboard with open tasks, upcoming items, active projects and completed items
- Tasks, reminders, projects and notes
- Create, edit, search, filter, complete and delete items
- Priority, category and optional due dates
- Responsive GitHub-style dark UI

## Run

Requires the .NET 8 SDK.

```bash
dotnet restore
dotnet run
```

Open the local URL shown in the terminal.

## Data

Items are stored in `App_Data/life.json` on the server. The directory is created automatically and excluded from Git. Use a persistent writable disk for deployment.

**Security:** This first version does not include authentication or separate user accounts. Anyone who can access the deployed app can read and edit the same data. Run locally or restrict access until authentication is implemented. Do not publicly deploy with private personal data.

## Roadmap

Authentication, recurring reminders, calendar view, backups and notifications.
