# E-Commerce Platform

Modern ve ölçeklenebilir bir e-ticaret platformu. .NET 10 ve Angular 21 kullanılarak Clean Architecture ve Domain Driven Design prensipleriyle geliştirilmiştir.

## 🏗️ Mimari

Proje, **Clean Architecture** ve **Domain Driven Design (DDD)** prensiplerine göre katmanlı bir yapıya sahiptir.

### Backend Mimarisi

```
ECommercePlatformServer/
├── ECommercePlatform.Domain         # Domain katmanı (Entities, Value Objects)
├── ECommercePlatform.Application    # Business Logic (CQRS, Behaviors)
├── ECommercePlatform.Infrastructure # Veri erişimi, External servisler
├── ECommercePlatform.MvcAdmin       # Admin paneli
└── ECommercePlatform.WebAPI         # API katmanı (Minimal API)
```

### Frontend Mimarisi

```
ECommercePlatformClient/
└── .angular/
    ├── src/
    │   ├── core/           # Servisler, Guards, Interceptors
    │   ├── features/       # Feature modülleri (lazy-loaded)
    │   └── shared/         # Reusable componentler
```

## 🚀 Teknolojiler

### Backend

- **.NET 10** - Modern ve performanslı framework
- **Entity Framework Core 10** - ORM ve veritabanı yönetimi
- **MSSQL** - İlişkisel veritabanı
- **Redis** - Dağıtık önbellek (Sepet ve yetki yönetimi)
- **Minimal API** - Hafif ve performanslı API endpoint'leri
- **Scalar + OpenAPI** - API dokümantasyonu

### Kütüphaneler ve Patternler

- **TS.MediatR** - CQRS pattern implementasyonu
- **TS.Result** - Result Pattern ile hata yönetimi
- **TS.EntityFrameworkCore.GenericRepository** - Generic Repository pattern
- **FluentValidation** - Validation işlemleri
- **Mapster** - Object mapping
- **FluentEmail.Smtp + smtp4dev** - E-posta servisi ve test ortamı
- **JWT Bearer** - Token tabanlı kimlik doğrulama
- **Scrutor** - Dependency injection dekorasyonları

### Frontend

- **Angular 21** - Modern SPA framework
- **TypeScript 5.9** - Type-safe development
- **Bootstrap 5.3** - Responsive UI framework
- **Bootstrap Icons** - İkon seti
- **RxJS 7.8** - Reaktif programlama
- **Vitest** - Test framework

## 🎯 Öne Çıkan Özellikler

### Domain Driven Design (DDD)

Entityler davranışlarını içerisinde barındırır ve domain logic merkezi bir şekilde yönetilir.

```csharp
// Örnek: Product entity
product.SetName(name);
product.SetPrice(price);
product.UpdateStock(stock);
```

**Value Objects** (Money, Address) kullanılarak domain kavramları güçlendirilmiştir.

### Multi-Tenancy Desteği

Her kullanıcı birden fazla şirkette farklı rollerle bulunabilir. **CompanyUser** entity'si ile şirket bazlı yetki yönetimi sağlanır.

### Permission-Based Authentication

- **PermissionBehavior** ile her request yetki kontrolünden geçer
- Yetkiler Redis'te cache'lenir (1 saat)
- Token içinde roller ve şirket bilgisi saklanır
- SuperAdmin her şeye yetkilidir

### CQRS ve Behaviors

**TS.MediatR** kullanılarak:
- **ValidationBehavior** - FluentValidation ile otomatik doğrulama
- **PermissionBehavior** - Permission kontrolü
- Command/Query ayrımı ile sorumluluklar netleştirildi

### Result Pattern

Hata yönetimi için **TS.Result** kullanılarak uygulama genelinde ortak bir veri dönüş stili sağlandı.

### Soft Delete & Auditing

- Tüm entity'ler **BaseEntity**'den türer
- `SaveChangesAsync` override edilerek otomatik auditing
- **Global Query Filters** ile soft delete uygulanır
- `Guid.CreateVersion7()` ile sıralanabilir ID'ler

### Multi-Tenant Global Filters

Expression Tree kullanılarak:
- Tenant bazlı veri izolasyonu
- Soft delete + Tenant filtreleri birleştirilerek performans optimizasyonu

### Refresh Token Mekanizması

- Access Token ve Refresh Token stratejisi
- Güvenli token yenileme mekanizması
- **Background Job** ile süresi geçmiş token temizliği

### File Management

**FileService** ile:
- Ürün fotoğrafları `wwwroot/uploads/` klasörüne kaydedilir
- Dosya boyutu ve format kontrolleri
- Güvenli dosya isimlendirme

### Exception Handling

Global **ExceptionHandler** middleware ile:
- ValidationException → 422
- ForbiddenAccessException → 403
- UnauthorizedAccessException → 401
- Türkçe hata mesajları
- Result pattern ile tutarlı response

### Options Pattern

```csharp
JwtOptions, EmailOptions
```
ile yapılandırma yönetimi **IOptions** ile sağlandı. Ayarların güvenli 

### Rate Limiting

API endpoint'lerinde rate limiter kullanılarak DDoS koruması sağlandı.

### Service Registration Pattern

Her katman kendi servis kayıtlarını yönetir:
```csharp
services.AddApplicationServices();
services.AddInfrastructureServices();
```

## 🎨 Frontend Özellikleri

### Signal-Driven Architecture

Angular Signals ile:
- **Single Source of Truth** - Tüm state servisler içinde
- Reaktif ve performanslı state yönetimi
- Computed values ile türetilmiş state'ler

```typescript
readonly products = this._products.asReadonly();
readonly hasProducts = computed(() => this._products().length > 0);
```

### Base Service Pattern

Tüm servisler **BaseService**'den türer:
- Pagination desteği
- Generic CRUD operasyonları
- Merkezi hata yönetimi

### Smart/Dumb Component Pattern

- **Smart Components** - State yönetimi ve business logic
- **Dumb Components** - Sadece görüntüleme (ProductCard, LoadingSpinner)

### Interceptors

- **authInterceptor** - JWT token ekleme ve refresh logic
- **tenantInterceptor** - Multi-tenant header yönetimi

### Guards

- **authGuard** - Korumalı sayfalara erişim kontrolü
- **guestGuard** - Misafir kullanıcı kontrolü (login/register)

### Lazy Loading

Tüm feature modülleri lazy-loaded olarak yüklenir:
```typescript
loadComponent: () => import('./features/products/...')
```

### Reusable Components

- **ProductCard** - ChangeDetection.OnPush ile optimize edilmiş
- **LoadingSpinner** - Parametrik spinner component
- Signal-based input/output

## 📸 Ekran Görüntüleri

### Angular Frontend

<details>
<summary>Kullanıcı Arayüzü</summary>

#### Ana Sayfa
![Ana Sayfa](screenshots/angular/home.png)

#### Giriş Yapma
![Giriş](screenshots/angular/login.png)

#### Kayıt Olma
![Kayıt](screenshots/angular/register.png)

#### Ürünler Sayfası
![Ürünler](screenshots/angular/products.png)

#### Kategoriler
![Kategoriler](screenshots/angular/categories.png)

#### Sipariş Oluşturma
![Sipariş Oluşturma](screenshots/angular/ordercreation.png)

#### Sipariş Geçmişi
![Sipariş Geçmişi](screenshots/angular/orderhistory.png)

#### Ödeme
![Ödeme](screenshots/angular/payment.png)

#### Profil
![Profil](screenshots/angular/profile.png)


</details>

### Admin Paneli (MVC)

<details>
<summary>Yönetim Paneli</summary>

#### Dashboard
![Dashboard](screenshots/admin/dashboard.png)

#### Giriş Ekranı
![Admin Giriş](screenshots/admin/login.png)

#### Kayıt Ekranı
![Admin Kayıt](screenshots/admin/register.png)

#### Ürün Yönetimi
![Ürün Yönetimi](screenshots/admin/products.png)

#### Kategori Yönetimi
![Kategori Yönetimi](screenshots/admin/categories.png)

#### Kategori Ağacı
![Kategori Ağacı](screenshots/admin/categorytree.png)

#### Marka Yönetimi
![Marka Yönetimi](screenshots/admin/brands.png)

#### Banner Yönetimi
![Banner Yönetimi](screenshots/admin/banners.png)

#### Yorumlar
![Yorumlar](screenshots/admin/comments.png)

#### Sipariş Yönetimi
![Sipariş Yönetimi](screenshots/admin/orders.png)

#### Müşteri Yönetimi
![Müşteri Yönetimi](screenshots/admin/customers.png)

#### Çalışan Yönetimi
![Çalışan Yönetimi](screenshots/admin/employees.png)

#### Permission Yönetimi
![Çalışan Yönetimi](screenshots/admin/permissions.png)

#### Şirket Yönetimi
![Şirket Yönetimi](screenshots/admin/company.png)

</details>

## 📦 Kurulum

### Gereksinimler

- .NET 10 SDK
- Node.js 20+
- MSSQL Server
- Redis Server
- smtp4dev (isteğe bağlı, e-posta testi için)

### Backend

```bash
cd ECommercePlatformServer

# Bağımlılıkları yükle
dotnet restore

# Veritabanını oluştur
dotnet ef database update --project ECommercePlatform.Infrastructure

# API'yi çalıştır
dotnet run --project ECommercePlatform.WebAPI

# Admin panelini çalıştır (opsiyonel)
dotnet run --project ECommercePlatform.MvcAdmin
```

### Frontend

```bash
cd ECommercePlatformClient/.angular

# Bağımlılıkları yükle
npm install

# Geliştirme sunucusunu başlat
npm start

# Tarayıcıda açılacak: http://localhost:4200
```

## 🔧 Konfigürasyon

### appsettings.json (Backend)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ECommerceDB;Trusted_Connection=True;TrustServerCertificate=True;",
    "Redis": "localhost:6379"
  },
  "JwtOptions": {
    "Issuer": "ECommercePlatform",
    "Audience": "ECommerceClients",
    "SecretKey": "your-secret-key-min-32-characters-long",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 30
  },
  "EmailSettings": {
    "Host": "localhost",
    "Port": 25,
    "FromEmail": "noreply@ecommerce.com",
    "FromName": "E-Commerce Platform"
  }
}
```

### environment.ts (Angular)

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  defaultTenantId: 'your-company-guid-here'
};
```

## 📚 API Dokümantasyonu

API dokümantasyonu için Scalar kullanılmaktadır:
```
http://localhost:5000/scalar/v1
```

OpenAPI/Swagger UI alternatif olarak:
```
http://localhost:5000/swagger
```

## 🧪 Test

### Backend
```bash
dotnet test
```

### Frontend
```bash
npm run test
```

## 📝 Seed Data

İlk çalıştırmada otomatik olarak:
- **Roller**: SuperAdmin, CompanyOwner, Employee, Customer
- Her role ait **permissionlar** AspNetRoleClaims tablosunda
- İlk **SuperAdmin kullanıcısı**
- Örnek **şirket** verisi

oluşturulur.

## 🔐 Güvenlik

- JWT Bearer authentication
- Permission-based authorization
- Multi-tenant data isolation
- Soft delete ile veri güvenliği
- Rate limiting (DDoS koruması)
- CORS yapılandırması
- FluentValidation ile input validation
- Parametreli SQL sorguları (SQL injection koruması)
- XSS koruması

## 🛠️ Geliştirme Notları

### Code Quality

- **Prettier** ile kod formatlama (Angular)
- **EditorConfig** ile tutarlı kod stili
- **TypeScript strict mode** aktif
- **Nullable reference types** (.NET)

### Performance

- **Global Query Filters** ile tenant ve soft delete
- **Redis caching** ile permission kontrolü optimizasyonu
- **Angular Signals** ile reaktif state yönetimi
- **Lazy loading** ile bundle size optimizasyonu
- **ChangeDetection.OnPush** ile render optimizasyonu

### Maintenance

- **Background Jobs** ile temizlik işlemleri
- **Structured logging** ile hata takibi
- **Health checks** endpoint'leri
- **Database migrations** ile versiyon kontrolü

## 🚦 Endpoint'ler

### Public Endpoints
- `POST /api/auth/register` - Kayıt olma
- `POST /api/auth/login` - Giriş yapma
- `POST /api/auth/refresh-token` - Token yenileme
- `GET /api/products` - Ürünleri listele
- `GET /api/categories` - Kategorileri listele

### Protected Endpoints (Authentication Required)
- `POST /api/basket/add` - Sepete ürün ekle
- `GET /api/orders/my-orders` - Siparişlerim
- `POST /api/orders/create` - Sipariş oluştur
- `PUT /api/profile/update` - Profil güncelle

### Admin Endpoints (Permission Required)
- `POST /api/products/create` - Ürün ekle
- `PUT /api/products/{id}` - Ürün güncelle
- `DELETE /api/products/{id}` - Ürün sil (soft delete)
- `GET /api/users` - Kullanıcıları listele
- `POST /api/permissions/assign` - Yetki ata

## 🌐 Multi-Tenant Yapı

### Tenant (Şirket) Mantığı

1. Her kullanıcı birden fazla şirkete üye olabilir
2. Her şirkette farklı roller ile bulunabilir
3. Token içinde aktif şirket ID'si bulunur
4. Header'dan `X-Tenant-ID` ile de tenant belirtilebilir
5. Tüm veri sorguları otomatik olarak tenant'a göre filtrelenir

### CompanyUser Entity

```csharp
public class CompanyUser {
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }
    public List<string> Roles { get; set; }
    public List<string> Permissions { get; set; }
}
```

## 📧 E-posta Sistemi

### smtp4dev Kullanımı

Geliştirme ortamında **smtp4dev** ile e-postalar test edilebilir:

```bash
# Docker ile çalıştırma
docker run -p 3000:80 -p 25:25 rnwood/smtp4dev
```

Web arayüzü: `http://localhost:3000`

### E-posta Senaryoları

- Kayıt onay e-postası
- Şifre sıfırlama
- Sipariş onay e-postası
- Çalışan davet e-postası

## 🎁 Bonus Özellikler

- **Banner yönetimi** ile dinamik anasayfa içeriği
- **Kategori ağacı** ile hiyerarşik kategori yapısı
- **Stok takibi** ve otomatik stok güncellemesi
- **Sipariş durumu** takibi (Pending, Processing, Shipped, Delivered)
- **Yorum sistemi** ile ürün değerlendirme
- **Responsive tasarım** ile mobil uyumluluk

## 📄 Lisans

Bu proje eğitim amaçlı Ufuk Abravacı tarafından geliştirilmiştir.
