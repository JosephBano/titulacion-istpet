# Titán — Sistema de Titulación ISTPET

Sistema de gestión del proceso de titulación del **Instituto Tecnológico Superior Traversari (ISTPET)**. Arquitectura cliente-servidor:
un backend .NET 8 y un frontend Angular, cada uno con su propia Clean Architecture
y su propio pipeline de CI.

```
titan/
├── backend/     .NET 8 · Clean Architecture · MySQL 5.7 (legacy) / 8.0 via EF Core + Pomelo
├── frontend/    Angular 19 · zoneless (signals) · Clean Architecture · Fluent Design 2
├── scripts/     Scripts de inicialización SQL (`01_Titulacion.sql`, `02_seed_rbac_titulacion.sql`)
├── docs/        Documentación técnica oficial
└── .github/     Workflows de CI, CODEOWNERS y plantillas
```

## Requisitos

| Herramienta | Versión |
|---|---|
| .NET SDK    | 8.0.x |
| Node.js     | 18.x / 20.x / 22.x LTS |
| MySQL       | 5.7 (instancia legacy) / 8.0 |

## Puesta en marcha

```bash
git clone https://github.com/JorgeDoicela/titan.git
cd titan
```

### Backend

```bash
cd backend

# Los appsettings reales están git-ignored. Partir de la plantilla:
cp src/Titan.Api/appsettings.example.json \
   src/Titan.Api/appsettings.Development.json
# ...y editar ConnectionStrings con las credenciales reales.

dotnet restore
dotnet build
dotnet test
dotnet run --project src/Titan.Api
```

API en `http://localhost:5032` / `https://localhost:7003`, Swagger en `/swagger`, health check en `/health`.

Alternativa sin tocar archivos, usando user secrets (queda fuera del repo por diseño):

```bash
cd backend/src/Titan.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Password=..."
```

### Frontend

```bash
cd frontend
npm ci
npm start        # http://localhost:4200
npm test         # ng test
npm run lint
npm run build -- --configuration production
```

## Arquitectura

### Backend — dependencias hacia adentro (`Titan.*`)

```
Titan.Api  ──►  Titan.Application  ──►  Titan.Domain
    └───────►   Titan.Infrastructure ──────┘
```

| Capa | Responsabilidad | Puede depender de |
|---|---|---|
| `Titan.Domain` | Entidades, enums, excepciones de negocio e interfaces de repositorio. Sin dependencias externas. | nada |
| `Titan.Application` | Casos de uso (CQRS con MediatR), validación (FluentValidation), DTOs. | `Titan.Domain` |
| `Titan.Infrastructure` | EF Core (`TitanDbContext`), adaptadores de repositorio, servicios externos, auth JWT. | `Titan.Application`, `Titan.Domain` |
| `Titan.Api` | Controllers, middleware, DI, configuración. | `Titan.Application`, `Titan.Infrastructure` |

`Titan.Domain` declara `IEstudianteRepository`; `Titan.Infrastructure` lo implementa. Esa inversión
es lo que permite testear la lógica de negocio sin base de datos.

### Frontend — mismo principio

| Capa | Responsabilidad |
|---|---|
| `domain/` | Modelos y **puertos** (interfaces + `InjectionToken`). Sin imports de HTTP. |
| `application/` | Stores con signals; orquestan casos de uso contra los puertos. |
| `infrastructure/` | Adaptadores HTTP que implementan los puertos. Único lugar que conoce las rutas del backend. |
| `presentation/` | Componentes `OnPush` que solo leen signals. |
| `core/` | Interceptores, configuración y guards transversales. |

El binding puerto → adaptador ocurre una sola vez, en `app.config.ts`. Por eso
`estudiantes.store.spec.ts` se testea con un doble en memoria y sin `HttpTestingController`.

## Base de datos: MySQL 5.7 legacy / 8.0

El esquema viene de un sistema existente (`sigafi_es`). Restricciones que impone la versión:

- **Longitud de índices.** Con `utf8mb4` el límite es 767 bytes, o sea **191 caracteres**
  por columna indexada. Toda columna dentro de un índice único debe declarar
  `HasMaxLength(<= 191)`.
- **Sin `CHECK` constraints.** MySQL 5.7 los parsea y los ignora. Las invariantes se
  validan en el dominio, no en la base.
- **Sin funciones de ventana ni CTEs.** Llegaron en MySQL 8.
- La versión se fija explícitamente en `DependencyInjection` / `TitanDbContext`:
  el autodetect de Pomelo abre una conexión al arrancar y rompería el CI, donde no
  hay MySQL.

## Configuración y secretos

Este repositorio es **público**. Ningún archivo con credenciales reales se versiona.

| Ignorado (real) | Versionado (plantilla) |
|---|---|
| `backend/src/Titan.Api/appsettings*.json` | `appsettings.example.json` |
| `frontend/src/environments/environment.prod.ts` | `environment.example.ts` |
| `.env` | `.env.example` |

Tres capas de defensa, en orden:

1. `.gitignore` — evita el commit accidental.
2. **GitGuardian (ggshield)** — escanea todo el historial del PR, no solo el diff final.
3. Job `archivos-prohibidos` — atrapa a quien use `git add -f` para saltarse el `.gitignore`.

> Si una credencial llega a publicarse: **rotala primero**, limpia el historial después.
> Un secreto que estuvo en un repo público se considera comprometido para siempre.

## Flujo de trabajo

Ver [CONTRIBUTING.md](CONTRIBUTING.md) y [docs/RAMAS.md](docs/RAMAS.md).

Resumen: `feature/*` → PR a `develop` → (solo el dueño) PR de `develop` a `main`.

---

## Documentación Técnica Completa

Para una inmersión profunda en el diseño técnico del sistema, consulte la suite de documentación en `docs/`:

- [Índice General de Documentación (`docs/README.md`)](docs/README.md)
- [Arquitectura Backend .NET 8 (`docs/01-arquitectura/`)](docs/01-arquitectura/01-arquitectura-general-net8.md)
- [Especificación de la API REST (`docs/02-backend-api/`)](docs/02-backend-api/01-autenticacion-jwt-y-refresh-tokens.md)
- [Esquema de Base de Datos `sigafi_es` (`docs/03-base-de-datos/`)](docs/03-base-de-datos/01-esquema-sigafi-y-modulo-titulacion.md)
- [Arquitectura Frontend Angular (`docs/04-frontend-angular/`)](docs/04-frontend-angular/01-arquitectura-angular19-standalone.md)
- [Guía de Despliegue y Operación (`docs/05-despliegue-y-operaciones/`)](docs/05-despliegue-y-operaciones/01-guia-ejecucion-local.md)
