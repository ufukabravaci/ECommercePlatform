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

Designed to simulate a real-world production backend system.

## 🏗️ Architecture Highlights

- Multi-tenant global query filters for data isolation
- JWT authentication with role-based and dynamic permission-based authorization
- Redis-backed basket system with sliding expiration
- Unit of Work pattern for transactional consistency
- Soft delete strategy with global query filters
- Domain entities encapsulating business behaviors
- Rate limiting applied to authentication endpoints

## 🧠 Architectural Decisions

- Multi-tenant architecture to simulate SaaS environment
- Permission-based authorization instead of role-only model
- Redis caching to reduce database load on high-traffic endpoints
- Domain-driven design to encapsulate business logic inside entities

## 🛠 Tech Stack

Backend: .NET 10, EF Core, CQRS, Minimal API
Database: MSSQL
Caching: Redis
Authentication: JWT
Frontend: Angular 21, Angular Signals, OnPush Change Detection

## 🎯 Core Features

### Domain Driven Design (DDD)

Entities encapsulate their own behaviors and business rules.

**Value Objects** such as Money and Address strengthen domain consistency and prevent primitive obsession.

### Multi-Tenancy

- A user can belong to multiple companies with different roles
- CompanyUser entity manages company-based authorization
- Tenant isolation enforced via Global Query Filters

### Permission-Based Authentication

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

Global **ExceptionHandler** middleware ve result pattern ile tutarlı responselarla hata yönetimi

### Service Registration Pattern

Each layer manages its own service registrations:
```csharp
services.AddApplicationServices();
services.AddInfrastructureServices();
```

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

## 📦 Setup

### Backend

```bash
cd ECommercePlatformServer

dotnet restore
dotnet ef database update --project ECommercePlatform.Infrastructure
dotnet run --project ECommercePlatform.WebAPI
# Admin panel
dotnet run --project ECommercePlatform.MvcAdmin
```

### Frontend

```bash
cd ECommercePlatformClient/.angular
npm install
npm start
### environment.ts (Angular)

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  defaultTenantId: 'your-company-guid-here'
};
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
