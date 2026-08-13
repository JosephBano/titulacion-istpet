# Titulacion ISTPET

Sistema de gestion del proceso de titulacion del ISTPET. Arquitectura cliente-servidor:
un backend .NET 8 y un frontend Angular, cada uno con su propia Clean Architecture
y su propio pipeline de CI.

```
titulacion_istpet/
├── backend/     .NET 8 · Clean Architecture · MySQL 5.7 (legacy) via EF Core + Pomelo
├── frontend/    Angular 22 · zoneless (signals) · Clean Architecture
└── .github/     Workflows de CI, CODEOWNERS y plantillas
```

## Requisitos

| Herramienta | Version |
|---|---|
| .NET SDK    | 8.0.x |
| Node.js     | 22.x LTS |
| MySQL       | 5.7 (instancia legacy) |

## Puesta en marcha

```bash
git clone https://github.com/JosephBano/titulacion-istpet.git
cd titulacion-istpet
```

### Backend

```bash
cd backend

# Los appsettings reales estan git-ignored. Partir de la plantilla:
cp src/TitulacionIstpet.WebApi/appsettings.example.json \
   src/TitulacionIstpet.WebApi/appsettings.Development.json
# ...y editar ConnectionStrings:MySqlLegacy con las credenciales reales.

dotnet restore
dotnet build
dotnet test
dotnet run --project src/TitulacionIstpet.WebApi
```

API en `https://localhost:7077`, Swagger en `/swagger`, health check en `/health`.

Alternativa sin tocar archivos, usando user secrets (queda fuera del repo por diseño):

```bash
cd backend/src/TitulacionIstpet.WebApi
dotnet user-secrets set "ConnectionStrings:MySqlLegacy" "Server=...;Password=..."
```

### Frontend

```bash
cd frontend
npm ci
npm start        # http://localhost:4200
npm test         # vitest
npm run lint
npm run build -- --configuration production
```

## Arquitectura

### Backend — dependencias hacia adentro

```
WebApi  ──►  Application  ──►  Domain
   └──────►  Infrastructure ──────┘
```

| Capa | Responsabilidad | Puede depender de |
|---|---|---|
| `Domain` | Entidades, enums, excepciones de negocio e interfaces de repositorio. Sin dependencias externas. | nada |
| `Application` | Casos de uso (CQRS con MediatR), validacion (FluentValidation), DTOs. | Domain |
| `Infrastructure` | EF Core, adaptadores de repositorio, servicios externos. | Application, Domain |
| `WebApi` | Controllers, middleware, DI, configuracion. | Application, Infrastructure |

`Domain` declara `IEstudianteRepository`; `Infrastructure` lo implementa. Esa inversion
es lo que permite testear la logica de negocio sin base de datos.

### Frontend — mismo principio

| Capa | Responsabilidad |
|---|---|
| `domain/` | Modelos y **puertos** (interfaces + `InjectionToken`). Sin imports de HTTP. |
| `application/` | Stores con signals; orquestan casos de uso contra los puertos. |
| `infrastructure/` | Adaptadores HTTP que implementan los puertos. Unico lugar que conoce las rutas del backend. |
| `presentation/` | Componentes `OnPush` que solo leen signals. |
| `core/` | Interceptores, configuracion y guards transversales. |

El binding puerto → adaptador ocurre una sola vez, en `app.config.ts`. Por eso
`estudiantes.store.spec.ts` se testea con un doble en memoria y sin `HttpTestingController`.

## Base de datos: MySQL 5.7 legacy

El esquema viene de un sistema existente. Restricciones que impone la version:

- **Longitud de indices.** Con `utf8mb4` el limite es 767 bytes, o sea **191 caracteres**
  por columna indexada. Toda columna dentro de un indice unico debe declarar
  `HasMaxLength(<= 191)`.
- **Sin `CHECK` constraints.** MySQL 5.7 los parsea y los ignora. Las invariantes se
  validan en el dominio, no en la base.
- **Sin funciones de ventana ni CTEs.** Llegaron en MySQL 8.
- La version se fija explicitamente en `InfrastructureServiceCollectionExtensions`:
  el autodetect de Pomelo abre una conexion al arrancar y romperia el CI, donde no
  hay MySQL.

## Configuracion y secretos

Este repositorio es **publico**. Ningun archivo con credenciales reales se versiona.

| Ignorado (real) | Versionado (plantilla) |
|---|---|
| `backend/src/TitulacionIstpet.WebApi/appsettings*.json` | `appsettings.example.json` |
| `frontend/src/environments/environment.prod.ts` | `environment.example.ts` |
| `.env` | `.env.example` |

Tres capas de defensa, en orden:

1. `.gitignore` — evita el commit accidental.
2. **GitGuardian (ggshield)** — escanea todo el historial del PR, no solo el diff final.
3. Job `archivos-prohibidos` — atrapa a quien use `git add -f` para saltarse el `.gitignore`.

> Si una credencial llega a publicarse: **rotala primero**, limpia el historial despues.
> Un secreto que estuvo en un repo publico se considera comprometido para siempre.

## Flujo de trabajo

Ver [CONTRIBUTING.md](CONTRIBUTING.md) y [docs/RAMAS.md](docs/RAMAS.md).

Resumen: `feature/*` → PR a `develop` → (solo el dueño) PR de `develop` a `main`.
