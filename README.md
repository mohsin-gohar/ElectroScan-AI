# ElectroScan AI

AI-Powered Electrical Intelligence Platform — for electricians, companies, clients, and administrators.

## Tech Stack
- ASP.NET Core 8 MVC
- Entity Framework Core with SQLite (auto-fallback) / SQL Server
- Cookie Authentication + Role-based Authorization
- Bootstrap-free custom modern UI

## Default Seeded Accounts

| Role          | Email                        | Password          |
|---------------|------------------------------|-------------------|
| Admin         | admin@electroscann.ai        | Admin123!         |
| Electrician   | electrician@electroscann.ai  | Electrician123!   |
| Company       | company@electroscann.ai      | Company123!       |
| Client        | client@electroscann.ai       | Client123!        |

## Run Locally

```bash
cd electroscannai
dotnet restore
dotnet run
```

The app listens on `http://localhost:5000` (or the port configured in launchSettings).

On Linux / environments without SQL Server, it automatically uses SQLite (`electroscann.db`). The database is created and seeded on first run.

## Features
- Public marketing site (Home, Services, Pricing, AI Scanner, Calculator, Contact, FAQ, etc.)
- Role-based dashboards (Admin, Electrician, Company, Client)
- Job posting & application system
- AI Scan reports, Wire/Load Calculator
- Subscriptions, Payments, Messages, Notifications
- User management & activity

## Notes
- SQLite decimal aggregate issue fixed in Dashboard revenue queries.
- LoginViewModel was missing and has been added.
- All major pages verified to load without server errors.
