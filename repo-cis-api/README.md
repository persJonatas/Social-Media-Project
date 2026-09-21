# CIS API — Collaborative Ideation Solution

RESTful API for managing topics, ideas, and votes in the Collaborative Ideation Solution (CIS).
Built with C# / ASP.NET Core and integrated with the Users API for authentication via JWT.

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Technologies](#technologies)
- [Prerequisites](#prerequisites)
- [Database Setup](#database-setup)
- [Running the Application](#running-the-application)
- [API Endpoints](#api-endpoints)
- [Authentication](#authentication)
- [Repository Structure](#repository-structure)
- [Related Repositories](#related-repositories)

---

## Overview

The CIS API is responsible for managing the core business logic of the Collaborative Ideation Solution: topics, ideas, and votes. It delegates all authentication and user management to the Users API (`repo-api-java`), validating JWT tokens via `POST /auth/validate` before processing any protected request.

---

## Architecture

The project follows a 3-tier architecture:

| Layer | Project | Responsibility |
|---|---|---|
| Presentation | `CisApi.Presentation` | Controllers, routing, HTTP request/response handling |
| Business | `CisApi.Business` | Business logic and service layer |
| Data | `CisApi.Data` | DbContext, Entity Framework migrations, database access |

---

## Technologies

| Component | Technology |
|---|---|
| Framework | .NET 9.0 / ASP.NET Core 9.0 |
| ORM | Entity Framework Core 9.0 |
| Database | MySQL 8.0 (via Pomelo.EntityFrameworkCore.MySql) |
| API Contract | OpenAPI 3.0 (`cis-api.yaml` in `repo-swagger-api`) |
| Authentication | JWT Bearer Token (validated via Users API) |

---

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- ASP.NET Core 9.0 Runtime
- MySQL 8.0 running on `localhost:3306`

> The database is shared with the Users API (`repo-api-java`). Run the Docker container from that repository before starting this API.

---

## Database Setup

The database is managed by the `repo-api-java` repository. To start it:

```bash
# In the repo-api-java directory
docker-compose up -d
```

> If MySQL is already running locally on port 3306, stop it first:
> ```bash
> net stop mysql80
> ```

The `mysql-init` folder in `repo-api-java` contains the initialization scripts:

- `01-schema.sql` — creates the `users` table
- `02-cis-schema.sql` — creates the `topics`, `ideas`, and `votes` tables

The connection string is configured in `CisApi.Presentation/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=sd3;User=root;Password=sd5;"
  }
}
```

---

## Running the Application

```bash
dotnet run --project CisApi.Presentation
```

The API will be available at:

```
http://localhost:8081/cis-api/v1
```

Health check:

```
GET http://localhost:8081/health
```

---

## API Endpoints

All endpoints require a valid JWT token in the `Authorization: Bearer <token>` header.
Obtain a token via `POST /api/v1/auth/login` on the Users API.

### Topics

| Method | Endpoint | Description | Success |
|---|---|---|---|
| GET | `/cis-api/v1/topics` | List all topics | 200 |
| POST | `/cis-api/v1/topics` | Create a new topic | 201 |
| GET | `/cis-api/v1/topics/{id}` | Get topic by ID | 200 |
| PUT | `/cis-api/v1/topics/{id}` | Update a topic | 200 |
| DELETE | `/cis-api/v1/topics/{id}` | Delete a topic | 204 |

### Ideas

| Method | Endpoint | Description | Success |
|---|---|---|---|
| GET | `/cis-api/v1/ideas` | List all ideas | 200 |
| POST | `/cis-api/v1/ideas` | Create a new idea | 201 |
| GET | `/cis-api/v1/ideas/{id}` | Get idea by ID | 200 |
| PUT | `/cis-api/v1/ideas/{id}` | Update an idea | 200 |
| DELETE | `/cis-api/v1/ideas/{id}` | Delete an idea | 204 |
| GET | `/cis-api/v1/topics/{topicId}/ideas` | List ideas by topic | 200 |

### Votes

| Method | Endpoint | Description | Success |
|---|---|---|---|
| POST | `/cis-api/v1/ideas/{ideaId}/votes` | Cast a vote on an idea | 201 |
| DELETE | `/cis-api/v1/ideas/{ideaId}/votes` | Cancel a vote on an idea | 204 |

> Business rule: a user can only cast one vote per idea. Attempting to vote again returns `409 Conflict`.

---

## Authentication

The CIS API does not manage users directly. For every authenticated request:

1. The client sends the request with `Authorization: Bearer <token>`
2. The CIS API calls `POST /api/v1/auth/validate` on the Users API
3. The Users API validates the token and returns `userId` and `login`
4. The CIS API uses the `userId` to associate the operation with the authenticated user

---

## Repository Structure

```
repo-cis-api/
├── CisApi.Business/          # Business logic and services
│   └── CisApi.Business.csproj
├── CisApi.Data/              # Data layer
│   ├── Migrations/           # EF Core migrations
│   ├── CisDbContext.cs       # Database context
│   └── CisApi.Data.csproj
├── CisApi.Presentation/      # Web API entry point
│   ├── Properties/
│   ├── appsettings.json      # Application configuration
│   ├── Program.cs
│   └── CisApi.Presentation.csproj
├── CisApi.sln                # Solution file
└── README.md
```

---

## Related Repositories

| Repository | Description |
|---|---|
| `repo-api-java` | Users API — user management and JWT authentication |
| `repo-swagger-api` | OpenAPI contracts for all APIs |
| `repo-api-legacy` | Legacy CLI system (Java + MyBatis) |