# Devices API

A production-ready RESTful API built with **.NET 10** and **C# 13** for managing device resources.  
This project follows Clean Architecture principles and implements domain-driven design practices to ensure maintainability, scalability, and testability.

---

## 🏗 Architecture

The solution follows a **Clean (Layered) Architecture** approach with clear separation of concerns:

### Devices.API
- Application entry point
- HTTP request handling
- API versioning
- Global exception handling
- OpenAPI configuration

### Devices.Application
- Business logic
- DTOs
- Interfaces
- Validators (FluentValidation)
- Application use cases

### Devices.Domain
- Core business entities
- Enums
- Domain rules
- Independent from external frameworks

### Devices.Infrastructure
- Data persistence layer
- Entity Framework Core configuration
- PostgreSQL integration
- Repository implementations

---

## 🛠 Technologies

- **Runtime:** .NET 10 & C# 13  
- **Database:** PostgreSQL 17  
- **ORM:** Entity Framework Core  
- **Validation:** FluentValidation  
- **Documentation:** OpenAPI (built-in .NET 10) & Swagger UI  
- **Containerization:** Docker & Docker Compose  
- **Testing:** xUnit, Moq, FluentAssertions  

---

## 🚀 Getting Started

### Prerequisites

- Docker Desktop  
- .NET 10 SDK (optional for local development)

---

### Running with Docker

#### 1. Clone the repository

```bash
git clone <repository-url>
cd devices-api/Devices
```

#### 2. Setup environment variables

Create a `.env` file in the root directory (based on `.env.example`):

```
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_password
POSTGRES_DB=devicesdb
```

#### 3. Start the application

```bash
docker-compose up --build
```

The API will be available at:

```
http://localhost:8080
```

---

## 📖 API Documentation

Once the application is running, access Swagger UI:

```
http://localhost:8080/swagger
```

The API is versioned.

**Current version:** `v1.0`

---

## 🧪 Running Tests

The project includes a comprehensive unit test suite covering domain rules and validations.

Run tests locally:

```bash
dotnet test
```

---

## 🔒 Domain Rules Implemented

### Immutable Creation Time
- `CreationTime` is set on creation.
- It cannot be modified afterward.

### In-Use Protection
Devices in the `InUse` state:
- Cannot update `Name`
- Cannot update `Brand`

### Delete Restriction
- Devices in the `InUse` state cannot be deleted.

### Uniqueness Constraint
- A unique constraint is enforced on the combination of:
  - `Name`
  - `Brand`

---

## 📬 Usage Examples

### Create a Device

```bash
curl -X POST http://localhost:8080/api/v1/devices \
-H "Content-Type: application/json" \
-d '{
  "name": "iPhone 15 Pro",
  "brand": "Apple",
  "state": "Available"
}'
```

### Fetch Devices by Brand

```bash
curl -X GET "http://localhost:8080/api/v1/devices?brand=Apple"
```

### Update a Device (PATCH)

```bash
curl -X PATCH http://localhost:8080/api/v1/devices/{id} \
-H "Content-Type: application/json" \
-d '{
  "state": "InUse"
}'
```

---

## 📈 Future Improvements

- Integration tests using Testcontainers with a real PostgreSQL instance
- Authentication & Authorization (JWT / OAuth2)
- Structured logging with Serilog
- Observability with OpenTelemetry
- Caching for frequently accessed queries
- CI/CD pipeline (GitHub Actions / Azure DevOps)
