# E-Commerce Platform

A production-oriented, multi-tenant e-commerce platform built with .NET and Clean Architecture, designed to simulate real-world SaaS architecture patterns.

## 🚀 Project Overview

This project demonstrates:

- Multi-tenant SaaS architecture
- Clean Architecture + CQRS
- Permission-based authorization model
- Redis-backed distributed caching
- Soft delete with auditing
- Rich domain modeling with value objects
- Dockerized infrastructure for seamless developer experience
- Automated CI/CD pipelines via GitHub Actions
- Comprehensive Unit Testing

Designed to simulate a real-world production backend system.

## 🏗️ Architecture Highlights

- Multi-tenant global query filters for data isolation
- ASP.NET Core Identity integrated with JWT authentication
- Role-based and dynamic permission-based authorization using Identity RoleClaims
- Redis-backed basket system with sliding expiration
- Unit of Work pattern for transactional consistency
- Soft delete strategy with global query filters
- Domain entities encapsulating business behaviors
- Rate limiting applied to authentication endpoints
- Unit tests for Application layer (Commands, Validators and business rules)
- Containerized backend services (MSSQL, Redis, Smtp4Dev, WebAPI, MVC Admin)
- Continuous Integration (CI)** pipeline testing backend and frontend on every push

## 🧩 Design Patterns Used

- Clean Architecture
- CQRS
- Pipeline Behaviors (Decorator pattern)
- Result Pattern
- Repository + Unit of Work
- Domain Driven Design

## 🛠 Tech Stack

- Backend: .NET 10, EF Core, ASP.NET Core Identity, CQRS, Minimal API
- Database: MSSQL (Docker)
- Caching: Redis (Docker)
- Authentication: JWT
- Frontend: Angular 21, Angular Signals, OnPush Change Detection
- Containerization: Docker, Docker Compose
- Testing: xUnit, Moq, FluentAssertions, MockQueryable
- CI/CD: GitHub Actions

## 🎯 Core Features

### Domain Driven Design (DDD)

Entities encapsulate their own behaviors and business rules.

**Value Objects** such as Money and Address strengthen domain consistency and prevent primitive obsession.

### Multi-Tenancy

- A user can belong to multiple companies with different roles
- CompanyUser entity manages company-based authorization
- Identity users associated with multiple companies via CompanyUser entity
- Tenant isolation enforced via Global Query Filters

### Permission-Based Authentication
- Built on top of ASP.NET Core Identity
- Identity roles and RoleClaims used for dynamic permission model
- Every request passes through **PermissionBehavior**
- Permissions are cached in Redis (1 hour TTL)
- Tokens include role and company context

### CQRS & Pipeline Behaviors

Using **TS.MediatR**
- **ValidationBehavior** - automatic validation via FluentValidation
- **PermissionBehavior** - centralized authorization control
- Clear separation of Command and Query responsibilities

### Result Pattern

Centralized error handling using **TS.Result**, ensuring consistent API responses across the application.

### Soft Delete & Auditing

- Auditing fields managed automatically via SaveChangesAsync override
- **Global Query Filters** enforce soft delete and tenant isolation

### Authentication & Token Management

- Access / Refresh token strategy
- Expired tokens cleaned via background job

### Exception Handling

Centralized error handling using a global ExceptionHandler middleware combined with the Result pattern.

### Service Registration Pattern

Each layer manages its own service registrations:
```csharp
services.AddApplicationServices();
services.AddInfrastructureServices();
```

## 🚀 Performance & Scalability

### Redis Caching

Redis is used to reduce database load on frequently accessed data.

Caching scenarios include:

- Shopping basket storage
- Permission caching for authorization checks

This significantly reduces database queries for frequently accessed authorization and user-specific data.

### Rate Limiting

Authentication endpoints are protected using ASP.NET Core Rate Limiting middleware.

Example policy:

- Fixed window limiter
- 3 requests per second

This prevents brute-force attacks on authentication endpoints.

## 🧪 Testing

The **Application layer** is covered with unit tests.

Tests focus on:

- Command handlers
- Validation rules
- Business logic
- Domain constraints

Libraries used:

- **xUnit** – test framework
- **Moq + MockQueryable** – mocking dependencies
- **FluentAssertions** – expressive assertions
- **FluentValidation.TestHelper** – validator testing

## ⚙️ CI/CD Pipeline (GitHub Actions)

The project uses **GitHub Actions** for Continuous Integration.

On every push or pull request:

- Backend solution is restored
- Backend is built
- Unit tests are executed
- Angular frontend build is validated

This ensures that new commits do not break the build or existing functionality.

## 🐳 Docker Support

The backend services are fully **containerized using Docker**.

Services included in Docker environment:

- Web API
- MSSQL Database
- Redis Cache
- Smtp4Dev
- MVC Admin

### Why Frontend Runs Locally

Initially the Angular application was also containerized and served via Nginx.

However, the application requires changing the **CompanyId environment variable** when running different tenants locally.

Because the Angular build produces static bundled assets, Nginx caching could sometimes serve stale frontend bundles.
This caused the application to run with outdated configuration values (such as an incorrect CompanyId).

To avoid stale configuration issues during development and ensure reliable environment changes, the Angular application runs locally using Node.js while backend services run in Docker.


## 🎨 Frontend Architecture (Angular 21)

### Signal-Driven State Management

- **Single Source of Truth** using Angular Signals
- Reactive state stored inside services

### Container / Presentational Component Pattern

- **Container (Smart) Components** - handle business logic and state
- **Presentational (Dumb) Components** - UI-only, driven by @Input/@Output

### HTTP Interceptors

- **authInterceptor** - attaches JWT & handles refresh logic
- **tenantInterceptor** - injects tenant header for multi-tenant requests

## 📸 Screenshots

### Angular Frontend

<details>
<summary>User Interface</summary>

#### Home
![Ana Sayfa](screenshots/angular/home.png)

#### Login
![Giriş](screenshots/angular/login.png)

#### Register
![Kayıt](screenshots/angular/register.png)

#### Products
![Ürünler](screenshots/angular/products.png)

#### Categories
![Kategoriler](screenshots/angular/categories.png)

#### Order Creation
![Sipariş Oluşturma](screenshots/angular/ordercreation.png)

#### Order History
![Sipariş Geçmişi](screenshots/angular/orderhistory.png)

#### Payment
![Ödeme](screenshots/angular/payment.png)

#### Profile
![Profil](screenshots/angular/profile.png)


</details>

### Admin Panel (MVC)

<details>
<summary>Management Panel</summary>

#### Dashboard
![Dashboard](screenshots/admin/dashboard.png)

#### Login
![Admin Giriş](screenshots/admin/login.png)

#### Register
![Admin Kayıt](screenshots/admin/register.png)

#### Product Management
![Ürün Yönetimi](screenshots/admin/products.png)

#### Category Management
![Kategori Yönetimi](screenshots/admin/categories.png)

#### Category Tree
![Kategori Ağacı](screenshots/admin/categorytree.png)

#### Brand Management
![Marka Yönetimi](screenshots/admin/brands.png)

#### Banner Management
![Banner Yönetimi](screenshots/admin/banners.png)

#### Reviews
![Yorumlar](screenshots/admin/comments.png)

#### Order Management
![Sipariş Yönetimi](screenshots/admin/orders.png)

#### Customer Management
![Müşteri Yönetimi](screenshots/admin/customers.png)

#### Employee Management
![Çalışan Yönetimi](screenshots/admin/employees.png)

#### Permission Management
![Çalışan Yönetimi](screenshots/admin/permissions.png)

#### Company Management
![Şirket Yönetimi](screenshots/admin/company.png)

</details>

## 📦 Setup (Local Development)

Backend is fully dockerized (no IDE required).
Frontend is run locally via Node.js for the best hot-reload experience and to avoid Docker/Nginx caching issues.

### Prerequisites
- Docker + Docker Compose
- Node.js (v22+ recommended)

### 1) Start Backend (Docker)

`docker-compose.yml` is located under `ECommercePlatformServer/`.

```bash
cd ECommercePlatformServer
docker-compose up -d --build
```

Useful URLs:
- API (Scalar): https://localhost:8081/scalar/v1
- Mail UI (Smtp4Dev): http://localhost:5002
- Admin Panel (MVC): http://localhost:5010

### 2) Create Company + Get CompanyId

1. Open Scalar: https://localhost:8081/scalar/v1
2. Call `RegisterTenant` and create your company.
3. Open Smtp4Dev UI: http://localhost:5002
4. Copy your **Company ID (Tenant ID)** from the welcome / confirmation email (or use the Admin Panel Company Settings page if available).

### 3) Configure Frontend Tenant

Edit both environment.ts and environment.prod.ts files under `ECommercePlatformClient/src/environments/` and set your Company ID:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:8080/api',
  defaultTenantId: 'YOUR-COMPANY-ID-HERE'
};
```

### 4) Start Frontend (Angular Local)

```bash
cd ECommercePlatformClient
npm install
npm start
```

Open:
- Storefront: http://localhost:4200

### Stop / Reset

Stop containers:
```bash
cd ECommercePlatformServer
docker-compose down
```

Full reset (removes volumes / DB data):
```bash
cd ECommercePlatformServer
docker-compose down -v
```

## 📝 Seed Data

On first run, the system automatically creates:
- **Default Roles**: SuperAdmin, CompanyOwner, Employee, Customer
- Role **permissions** stored in AspNetRoleClaims
- Initial **SuperAdmin user**

## 👤 Author

Ufuk Abravacı
Backend Developer (.NET)

- LinkedIn: https://linkedin.com/in/ufukabravaci
- GitHub: https://github.com/ufukabravaci
- Email: ufukabravaci@gmail.com

## 📌 Project Purpose

This project was built as a **portfolio project** to demonstrate production-grade backend architecture including:

- Multi-tenant SaaS design
- Clean Architecture
- Advanced authorization models
- Scalable infrastructure components