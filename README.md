# Sistema de Titulación Académica Titán — ISTPET

Plataforma informática oficial para la **digitalización integral del proceso de titulación académica** del **Instituto Tecnológico Superior Traversari (ISTPET)** en Quito, Ecuador.

El sistema migra la gestión manual y física en papel de postulaciones, validación de requisitos por carrera, cohortes, registro de notas de examen complexivo, recepción de artículos científicos y verificación de aptitud hacia un expediente académico digital unificado, seguro y auditado.

---

## Arquitectura y Tecnologías

La solución sigue una arquitectura desacoplada cliente-servidor orientada a servicios REST:

* **Backend:** C# / .NET 8 estructurado bajo **Clean Architecture** (`Titan.Domain`, `Titan.Application`, `Titan.Infrastructure`, `Titan.Api`).
* **Persistencia:** MySQL (EF Core + Pomelo) sobre la base de datos institucional **`sigafi_es`** (tablas nativas SIGAFI + módulo de titulación `Tit_*` + subsistema de seguridad `rbac_*`).
* **Seguridad:** Autenticación JWT Bearer con rotación de Refresh Tokens persistidos en DB (`rbac_refresh_tokens`) y control de acceso granular por permisos (**RBAC** con el atributo `HasPermission`).
* **Frontend Web:** **Angular 19** con componentes Standalone, manejo de estado reactivo mediante **Signals**, RxJS, interceptores HTTP y el sistema de diseño **Microsoft Fluent Design 2** con la paleta cromática oficial ISTPET.

---

## Estructura del Monorepo

```
titulacion_istpet/
├── backend/                    <- Solución en C# .NET 8 (Clean Architecture)
│   ├── src/
│   │   ├── Titan.Domain/       <- Entidades del dominio e interfaces base
│   │   ├── Titan.Application/  <- DTOs e interfaces de servicios de aplicación
│   │   ├── Titan.Infrastructure/<- TitanDbContext (EF Core 8 MySQL), JWT, AuthService
│   │   └── Titan.Api/          <- Controladores REST (Auth, Rbac, Academico, Actores)
│   └── Titan.slnx
│
├── frontend/                   <- Cliente Web en Angular 19 Standalone
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/           <- Guards, Interceptors HTTP, Models, Services
│   │   │   └── pages/          <- Vistas del sistema (login, tableros)
│   │   └── styles.css          <- Design tokens Fluent Design 2 (Azul Marino ISTPET #002855)
│   ├── angular.json
│   └── package.json
│
├── .github/                    <- Workflows de CI/CD, CODEOWNERS y plantillas de PR
├── scripts/                    <- Scripts de base de datos y gobernanza
├── docs/                       <- Suite de documentación técnica oficial
└── README.md
```

---

## Puesta en Marcha

### Requisitos Previos
* .NET 8.0 SDK
* Node.js v18+ / v20+ / v22+
* Servidor MySQL activo en el puerto 3306

### 1. Backend (.NET 8 API)
```bash
cd backend
dotnet restore
dotnet build
dotnet run --project src/Titan.Api
```
- API activa por defecto en `http://localhost:5000` (Swagger UI disponible en `/swagger`).

### 2. Frontend (Angular 19 Web)
```bash
cd frontend
npm install
npm start
```
- Aplicación web accesible en `http://localhost:4200/`.

---

## Flujo de Trabajo y Gobernanza

Ver [CONTRIBUTING.md](CONTRIBUTING.md) y [docs/RAMAS.md](docs/RAMAS.md).
