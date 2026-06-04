# 🏨 StayHub — Otel Rezervasyon Sistemi

StayHub, ASP.NET Core MVC ile geliştirilmiş full stack bir otel rezervasyon yönetim uygulamasıdır. Kullanıcılar otel arayabilir, oda seçebilir ve rezervasyon oluşturabilir. Yöneticiler ise panel üzerinden tüm rezervasyon ve oda işlemlerini yönetebilir.

---

## 🚀 Özellikler

- Kullanıcı kaydı, girişi ve oturum yönetimi
- Oda listeleme, filtreleme ve müsaitlik kontrolü
- Rezervasyon oluşturma, güncelleme ve iptal
- Admin paneli — oda ve rezervasyon yönetimi
- Entity Framework Core ile veritabanı entegrasyonu
- Responsive tasarım (mobil uyumlu)

---

## 🛠️ Kullanılan Teknolojiler

| Katman | Teknoloji |
|--------|-----------|
| Backend | C#, ASP.NET Core MVC |
| ORM | Entity Framework Core, LINQ |
| Veritabanı | SQL Server |
| Frontend | HTML5, CSS3, JavaScript |
| Mimari | MVC Pattern, Repository Pattern |
| Araçlar | Visual Studio, Git |



## 📁 Proje Yapısı

```
StayHub/
├── Controllers/        # MVC Controller sınıfları
├── Models/             # Entity ve ViewModel sınıfları
├── Views/              # Razor View dosyaları (.cshtml)
├── wwwroot/            # Statik dosyalar (CSS, JS, görseller)
├── Properties/         # Uygulama ayar dosyaları
├── screenshots/        # README ekran görüntüleri
└── appsettings.json    # Veritabanı bağlantı ayarları
```

---

## ⚙️ Kurulum

### Gereksinimler
- .NET 6+ SDK
- SQL Server (LocalDB veya full)
- Visual Studio 2022 / VS Code

### Adımlar

1. **Projeyi klonla**
```bash
git clone https://github.com/muhammetakln/StayHub.git
cd StayHub
```

2. **Veritabanı bağlantısını ayarla**

`appsettings.json` dosyasındaki connection string'i kendi SQL Server bilgilerinle güncelle:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=StayHubDb;Trusted_Connection=True;"
}
```

3. **Migration uygula**
```bash
dotnet ef database update
```

4. **Projeyi çalıştır**
```bash
dotnet run
```

# 🏨 StayHub — Otel Rezervasyon Sistemi

StayHub, ASP.NET Core MVC ile geliştirilmiş full stack bir otel rezervasyon yönetim uygulamasıdır. Kullanıcılar otel arayabilir, oda seçebilir ve rezervasyon oluşturabilir. Yöneticiler ise panel üzerinden tüm rezervasyon ve oda işlemlerini yönetebilir.

---

## 🚀 Özellikler

- Kullanıcı kaydı, girişi ve oturum yönetimi
- Oda listeleme, filtreleme ve müsaitlik kontrolü
- Rezervasyon oluşturma, güncelleme ve iptal
- Admin paneli — oda ve rezervasyon yönetimi
- Entity Framework Core ile veritabanı entegrasyonu
- Responsive tasarım (mobil uyumlu)

---

## 🛠️ Kullanılan Teknolojiler

| Katman | Teknoloji |
|--------|-----------|
| Backend | C#, ASP.NET Core MVC |
| ORM | Entity Framework Core, LINQ |
| Veritabanı | SQL Server |
| Frontend | HTML5, CSS3, JavaScript |
| Mimari | MVC Pattern, Repository Pattern |
| Araçlar | Visual Studio, Git |

## 📁 Proje Yapısı

```
StayHub/
├── Controllers/        # MVC Controller sınıfları
├── Models/             # Entity ve ViewModel sınıfları
├── Views/              # Razor View dosyaları (.cshtml)
├── wwwroot/            # Statik dosyalar (CSS, JS, görseller)
├── Properties/         # Uygulama ayar dosyaları
├── screenshots/        # README ekran görüntüleri
└── appsettings.json    # Veritabanı bağlantı ayarları
```

---

## ⚙️ Kurulum

### Gereksinimler
- .NET 6+ SDK
- SQL Server (LocalDB veya full)
- Visual Studio 2022 / VS Code

### Adımlar

1. **Projeyi klonla**
```bash
git clone https://github.com/muhammetakln/StayHub.git
cd StayHub
```

2. **Veritabanı bağlantısını ayarla**

`appsettings.json` dosyasındaki connection string'i kendi SQL Server bilgilerinle güncelle:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=StayHubDb;Trusted_Connection=True;"
}
```

3. **Migration uygula**
```bash
dotnet ef database update
```

4. **Projeyi çalıştır**
```bash
dotnet run
```

Uygulama `https://localhost:5001` adresinde çalışacaktır.

---

## 👤 Geliştirici

**Muhammet Akalin** — Full Stack Developer

- GitHub: [@muhammetakln](https://github.com/muhammetakln)
- LinkedIn: [muhammet-akalinn](https://www.linkedin.com/in/muhammet-akalinn-703aa4406/)
- Portfolio: [muhammetakln.github.io](https://muhammetakln.github.io)

---

## 📄 Lisans

Bu proje MIT lisansı ile lisanslanmıştır.
---

## 👤 Geliştirici

**Muhammet Akalin** — Full Stack Developer

- GitHub: [@muhammetakln](https://github.com/muhammetakln)
- LinkedIn: [muhammet-akalinn](https://www.linkedin.com/in/muhammet-akalinn-703aa4406/)
- Portfolio: [muhammetakln.github.io](https://muhammetakln.github.io)

---

## 📄 Lisans

Bu proje MIT lisansı ile lisanslanmıştır.
