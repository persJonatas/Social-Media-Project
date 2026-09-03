# Collaborative Ideation Solution (CIS) — Social Media Project

Solução de Ideação Colaborativa baseada em arquitetura de microsserviços distribuídos para gestão de usuários, tópicos de discussão, ideias e votação.

---

## 🏛️ Arquitetura do Sistema

O sistema é composto por serviços desacoplados e uma interface web:

| Componente | Stack Tecnológica | Porta Padrão | Descrição |
|---|---|---|---|
| **Users API** | Java 17+ / Spring Boot 3 / MySQL | `http://localhost:8080` | Microsserviço de gestão e CRUD de usuários, autenticação via JWT e validação de tokens (`/auth/validate`). |
| **Legacy CLI** | Java / MyBatis / MySQL | — | Sistema CLI legado para administração de usuários, operando simultaneamente na mesma base MySQL. |
| **CIS API** | C# / .NET 9.0 / ASP.NET Core | `http://localhost:8081` | Microsserviço de tópicos, ideias e votos. Valida tokens de usuário através da Users API. |
| **Frontend Web** | React 18 / Vite / Tailwind CSS | `http://localhost:5173` | Interface web para visualização de tópicos, postagem de ideias e votação em tempo real. |

---

## 🛠️ Tecnologias e Infraestrutura

- **Backend**: Spring Boot 3 (Java), ASP.NET Core 9.0 (C#)
- **Persistência**: MySQL 8.0 gerenciado via Docker Compose
- **Documentação de API**: OpenAPI 3.0 / Swagger UI
- **Testes**:
  - Testes Unitários: JUnit 5 / Mockito (Java) e xUnit (C#)
  - Testes Automatizados / Integração: `test-client` (Java) e `CisApi.TestClient` (C#)
  - Postman: Coleções completas e ambientes com automação de testes

---

## 🚀 Como Executar o Projeto

### 1. Iniciar o Banco de Dados MySQL
Execute na pasta `api-java`:
```bash
cd api-java
docker compose up -d
```

### 2. Iniciar a Users API (Java)
```bash
cd api-java
./mvnw spring-boot:run -pl app
```
A API estará acessível em `http://localhost:8080/api/v1`.

### 3. Iniciar a CIS API (.NET)
```bash
cd repo-cis-api
dotnet run --project CisApi.Presentation
```
A API estará acessível em `http://localhost:8081/cis-api/v1`.

### 4. Iniciar o Frontend Web (React)
```bash
cd frontend
npm install
npm run dev
```
Acesse em `http://localhost:5173`.

---

## 🧪 Executando os Testes

### Testes da Users API (Java)
```bash
# Testes Unitários (JUnit 5)
cd api-java
./mvnw test -pl app

# Cliente de Teste Automatizado HTTP
./mvnw compile exec:java -pl test-client
```

### Testes da CIS API (.NET)
```bash
# Testes Unitários (xUnit)
cd repo-cis-api
dotnet test CisApi.Tests

# Teste Automatizado / Simulação Multiusuário
dotnet run --project CisApi.TestClient
```

---

## 📮 Coleções do Postman

As coleções prontas para importação estão localizadas em:
- `api-java/postman/Users_API.postman_collection.json`
- `repo-cis-api/postman/CIS_API.postman_collection.json`
