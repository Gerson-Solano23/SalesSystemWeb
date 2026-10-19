# Sales Web System

## Introduction

Full-stack point-of-sale and sales management system. The backend is a layered ASP.NET Core 8 Web API
(API → BLL → DAL → Entity/DTO) using Entity Framework Core 8 and SQL Server; the frontend is an Angular 22
application with Angular Material. Reports are exported to Excel, a weekly report is uploaded to AWS S3,
and the dashboard shows weekly, monthly, yearly and custom-range statistics.

## Technologies Used

- **Backend**: C#, .NET 8, ASP.NET Core Web API
- **Frontend**: Angular 22, Angular Material, Chart.js
- **Database**: SQL Server, Entity Framework Core 8
- **Cloud**: AWS S3 (report storage)
- **Caching**: Output Caching in ASP.NET Core
- **Background jobs**: hosted service that uploads the weekly Excel report
- **Authentication**: JWT bearer tokens, role-based authorization (Admin, Supervisor, Employee)
- **Tests**: xUnit with an in-memory SQLite database

## Features

- **Sales**: register sales with stock validation, server-side prices and consecutive document numbers.
- **Catalog and users**: manage products, categories and users (passwords stored as PBKDF2 hashes).
- **Sales history and reports**: search by document number or date range, export reports to Excel.
- **Dashboard**: totals, sales charts and top products for the last 7 days, a month, a year or a date range.
- **Files**: upload, list and download documents stored in S3.
- **Weekly report**: the previous week's Excel report is uploaded to S3 automatically.
- **Sales forecast**: monthly revenue forecast (Holt linear trend) with an approximate 95% range,
  refit from the database on every request (`GET /api/Prediction/MonthlyRevenue?months=3`).

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server with the `MySalesCompany` database (tables and the `CSP_TopProducts_*` stored procedures)
- Node.js 22.22.3+ or 24 LTS and Angular CLI 22
- Optional: AWS account/bucket for the files module and the weekly report

### New database

To start from an empty SQL Server, run `backend/database/000_create_database.sql`. It creates the
`MySalesCompany` database, its tables and the dashboard stored procedures, plus initial data: roles, menus,
a `General` category and the user **admin@greateshop.com / Admin123!** (change this password after the first
login). It does nothing if the tables already exist.

```sh
sqlcmd -S <server> -U sa -P <password> -C -i backend/database/000_create_database.sql
```

### Database update for an existing database (required once)

Passwords are now stored as hashes, which need a wider column. Run before deploying this version:

```sh
sqlcmd -S <server> -d MySalesCompany -i backend/database/001_password_hash.sql
```

Existing plain-text passwords keep working: each one is re-hashed automatically the first time that user logs in.

### Backend configuration

Settings are read from `appsettings.json`, `appsettings.Development.json` and environment variables
(`Section__Key` maps to `Section:Key`).

| Setting | Environment variable | Description |
|---|---|---|
| `ConnectionStrings:StringConnection` | `ConnectionStrings__StringConnection` | SQL Server connection string (the development one is in `appsettings.Development.json`) |
| `Jwt:Key` | `CERTIFICATE_KEY` or `Jwt__Key` | **Required.** JWT signing key, at least 32 characters |
| `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpirationHours` | `Jwt__...` | Token settings (defaults in `appsettings.json`) |
| `Cors:AllowedOrigins` | `Cors__AllowedOrigins__0`, ... | Origins allowed to call the API (`http://localhost:4200` in development) |
| `Email:User`, `Email:Password`, `Email:NotificationEmail` | `PASSGREATEINDUSTRIES`, `MYOWNEMAIL` | SMTP account (server side only) and stock-alert recipient |
| — | `AWSKEY`, `AWSKEYSECRET`, `BUCKETNAME` | S3 credentials and bucket |
| `WeeklyReport:Enabled` | `WeeklyReport__Enabled` | `false` disables the weekly S3 upload |
| `ReverseProxy:Enabled` | `ReverseProxy__Enabled` | `true` only when the API is behind a reverse proxy (uses `X-Forwarded-For`) |

### Run locally

```sh
# API (http://localhost:5038, Swagger at /swagger in Development)
cd backend/SalesSystem
export CERTIFICATE_KEY="a-long-random-secret-of-at-least-32-chars"
dotnet run --project SalesSystem.API

# Web app (http://localhost:4200)
cd frontend/SalesSystemWeb
npm ci
npm start
```

The development build calls `http://localhost:5038/api/` (`src/environments/environment.development.ts`);
the production build calls `/api/` on the same origin (`src/environments/environment.ts`).

### Tests

```sh
cd backend/SalesSystem
dotnet test
```

The tests cover password hashing, date parsing, the weekly report schedule and, against an in-memory
SQLite database, sale registration (stock, prices, document numbers, rollback) and login.

### Docker

```sh
cp .env.example .env   # fill in the connection string, JWT key, AWS and SMTP values
docker compose up --build
```

The web app is served on http://localhost:8080 and nginx forwards `/api/` to the API container.
The database is not part of the compose file: point `DB_CONNECTION_STRING` to an existing SQL Server.

### Continuous integration

`.github/workflows/ci.yml` builds and tests the backend and builds the frontend on every push to `main`
and on pull requests.

## API notes

- `POST /api/Authenticator/Login` with `{ "email", "password" }` returns `{ token, session }`.
- Send the token as `Authorization: Bearer <token>`; the user id is always taken from the token.
- Errors use the same envelope `{ status: false, message }`. Only business-rule messages are returned;
  unexpected errors are logged on the server and answered with a generic message.
- `GET /health` reports whether the API is running.
