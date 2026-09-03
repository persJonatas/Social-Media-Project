# CIS Project

Modern API developed in **Java with Spring Boot** that integrates with a **legacy Java CLI system** and a **MySQL database**.

---

# Project Overview

The CIS project introduces a **modern REST API** that interacts with a **legacy command-line Java application** and a **shared MySQL database**.

Instead of replacing the legacy system immediately, the new API is designed to **coexist with the existing application**, ensuring that all operations maintain **data consistency** and **system stability**.

This architecture allows the system to evolve incrementally without disrupting current functionality.

---

# Phase 1 Objective – Legacy System Support

The main goal of **Phase 1** is to support and integrate with the legacy system.

During this phase, the new API must:

- Connect to the **existing MySQL legacy database**
- Maintain **data consistency with the legacy CLI**
- Allow both systems to operate **simultaneously**
- Provide modern API endpoints without breaking existing functionality

This approach ensures a **safe transition toward modernization** while preserving the current operational system.

---

# Predictable Structure

The repository follows a **clear and predictable structure** so developers can quickly understand how the legacy system, the new API, and the infrastructure components interact.


```
repo-java-api/

├── legacy-system/
│   ├── src/
│   ├── pom.xml
│   └── cis_config.xml
│
├── src/
│   ├── main/java/
│   └── main/resources/
│       ├── migrations/
│       └── application.yaml
│
├── mvnw
├── mvnw.cmd
├── pom.xml
│
├── mysql-init/
│   ├── 01-schema.sql
│   └── 02-cis-schema.sql
│
├── docker-compose.yml
├── .gitignore
└── README.md
```

This structure allows both the **legacy application** and the **modern API** to coexist while sharing the same database environment.

---

# Technologies Used

Main technologies used in the project:

- **Java 19** (Legacy CLI) & **Java 21** (Spring Boot API)
- **Spring Boot**
- **MySQL**
- **Docker & Docker Compose**
- **Maven**
- **Git & GitLab**

---

# Prerequisites

Before running the project, ensure the following tools are installed:

- Java (JDK 19 and JDK 21)
- Maven
- Docker
- Git

---

# Running the Database (Automated Setup)

The project uses **Docker Compose** to run the MySQL database. The database schema and initial tables are automatically created using the files inside the `mysql-init/` folder.

In the root of the repository, run:

```bash
docker compose up -d
```

This will start the MySQL container (`cis_mysql`) on port `3306` with the database `cis_db` ready to be used by both the legacy system and the modern API.

---

# Running the Modern API (Spring Boot)

1. Start the database:

   docker-compose up -d

2. Run Application.java from your IDE
   or navigate to the api-java directory and run:

   mvn spring-boot:run

The API will be available at: http://localhost:8080/api/v1

## API Documentation

Swagger UI is available at: http://localhost:8080/swagger-ui.html

## Authentication

1. POST /api/v1/auth/login with body: { "login": "...", "password": "..." }
2. Copy the token from the response
3. Click Authorize in Swagger UI and paste the token

## Available Endpoints

| Method | Path                  | Auth | Description        |
|--------|-----------------------|------|--------------------|
| POST   | /api/v1/auth/login    | No   | Authenticate user  |
| POST   | /api/v1/auth/validate | Yes  | Validate JWT token |
| GET    | /api/v1/users         | Yes  | List all users     |
| POST   | /api/v1/users         | Yes  | Create user        |
| GET    | /api/v1/users/{id}    | Yes  | Get user by ID     |
| PUT    | /api/v1/users/{id}    | Yes  | Update user        |
| DELETE | /api/v1/users/{id}    | Yes  | Delete user        |

## Running Tests

Set JAVA_HOME to JDK 21, then run: mvn test

---

# Running the Legacy CLI - Coming Soon

Note: The legacy source code is currently being migrated to this repository. Once available, follow the instructions below.

## DB Configuration File

To connect the legacy system to the Docker database, you will need an XML configuration file (e.g., `cis_config.xml`). Ensure the credentials match the `docker-compose.yml`.

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<!DOCTYPE configuration
  PUBLIC "-//mybatis.org//DTD Config 3.0//EN"
  "http://mybatis.org/dtd/mybatis-3-config.dtd">
<configuration>
    <environments default="development">
        <environment id="development">
            <transactionManager type="JDBC" />
            <dataSource type="POOLED">
                <property name="driver" value="com.mysql.cj.jdbc.Driver" />
                <property name="url" value="jdbc:mysql://localhost:3306/cis_db" />
                <property name="username" value="root" />
                <property name="password" value="root" />
            </dataSource>
        </environment>
    </environments>
</configuration>
```

## Usage

Compile the legacy project and run the CLI program.

```
Users CLI

Usage: users -config=<configuration> [COMMAND]
CRUD on a Users DB
      -config=<configuration>
         Configuration File (xml)

Commands:
  -read    Read Users
  -delete  Delete a User by ID
  -create  Create a new user
  -update  Update an existing user
```

### Common parameters examples

Assuming your config file is named `cis_config.xml`.

**Read users**

```bash
-config=cis_config.xml -read
```

**Create user**

```bash
-config=cis_config.xml -create -n javier -l jroca -p pass123
```

**Delete existing user**

Example ID: `aab5d5fd-70c1-11e5-a4fb-b026b977eb28`

```bash
-config=cis_config.xml -delete aab5d5fd-70c1-11e5-a4fb-b026b977eb28
```

**Update existing user**

Example ID: `3bf71036-e7ef-4890-b79b-91496c14160f`

```bash
-config=cis_config.xml -update -i 3bf71036-e7ef-4890-b79b-91496c14160f -n javier2 -l jroca2 -p pwd321
```