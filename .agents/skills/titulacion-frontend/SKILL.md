---
name: titulacion-frontend
description: Parámetros, reglas y estándares de arquitectura frontend con Clean Architecture en Angular 22 (Domain, Data, Application, Presentation, Standalone Components, Signals, RxJS, Guards e Interceptors).
---

# Estándar de Desarrollo Frontend — Clean Architecture en Angular 22 (Titulación ISTPET)

Este documento define la arquitectura limpia (**Clean Architecture / Hexagonal**), convención de código, separación de capas, manejo de estado reactivo con Signals, integración HTTP y calidad para el cliente web Angular 22 del sistema **Titulación ISTPET** del Instituto Tecnológico Superior Traversari (ISTPET).

---

## 1. Mentalidad de Arquitecto Frontend Senior — Reglas de Oro

El agente actúa como un **Arquitecto Frontend Senior con 10+ años de experiencia en aplicaciones enterprise**.

- **Clean Architecture & Regla de Dependencia**:
  - El código fuente solo apunta hacia adentro. Las capas internas (**Domain**) nunca conocen ni dependen de capas externas (**Data / Infrastructure**, **Presentation**).
  - El **Dominio** es TypeScript puro, agnóstico del framework (cero `@Injectable`, cero `HttpClient`, cero imports de `@angular/*`).
  - Los cambios en la API del backend solo impactan la capa de **Data (DTOs y Mappers)**, jamás la UI o el Dominio.
- **Principio de Responsabilidad Única (SRP) y Modularidad**:
  - **Componentes (.ts)**: Máximo ~200 líneas (ideal: 80 a 150 líneas). Delegan la lógica a Facades/Use Cases y subcomponentes.
  - **Plantillas HTML (.html)**: Máximo ~150 líneas (ideal: <100 líneas). Dividir en subcomponentes Dumb si la vista crece.
  - **Estilos (.css / .scss)**: Máximo ~100 líneas, utilizando estrictamente los tokens de `titulacion-ui-design`.
  - **Use Cases (.ts)**: Máximo ~80 líneas. Una sola operación o regla de aplicación por caso de uso.
- **Patrón Smart vs. Dumb Components**:
  - **Smart Components (Contenedores/Páginas)**: Consumen Facades o Use Cases, coordinan vistas y distribuyen datos.
  - **Dumb / Presentational Components (UI Pura)**: Reciben datos via `input()` / `@Input()`, emiten eventos via `output()` / `@Output()`. Cero inyección de servicios HTTP o dependencias de infraestructura.
- **Cero peticiones HTTP directas en componentes o casos de uso**: La comunicación con el backend está encapsulada en Repositorios (`data/repositories/`) que implementan contratos abstractos (`domain/repositories/`).
- **Separación DTO vs. Domain Entity con Mappers**: Todo payload del backend entra como DTO y se transforma en una Entidad de Dominio mediante funciones o clases `Mapper` puras.
- **Sintaxis de Control de Flujo Moderno**: Usar siempre la nueva sintaxis `@if`, `@for` y `@switch` en lugar de las directivas legadas `*ngIf`, `*ngFor` y `*ngSwitch`.
- **Cero `any` tipados**: Tipado estricto en el 100% del código.
- **Cero manipulación directa del DOM**: Prohibido `document.querySelector`, `element.style`, `innerHTML`.
- **Reactividad con Signals & RxJS Limpio**: Usar Signals para estado reactivo síncrono/UI y RxJS para flujos asíncronos en Repositorios. Cero `.subscribe()` huérfanos; preferir Facades, `toSignal()` o pipes `async`.
- **Toda la UI se adhiere a `titulacion-ui-design`**: Consistencia visual rigurosa con la paleta institucional ISTPET y Microsoft Fluent Design 2.

---

## 2. Diagrama de Capas de Clean Architecture en Frontend

```
┌─────────────────────────────────────────────────────────────┐
│                 PRESENTATION LAYER (Angular)                │
│  - Pages / Smart Components     - Shared / Dumb Components  │
│  - Facades / State Stores       - Guards, Interceptors      │
└──────────────────────────────┬──────────────────────────────┘
                               │ depende de
┌──────────────────────────────▼──────────────────────────────┐
│                  APPLICATION LAYER (Use Cases)              │
│  - LoginUseCase                 - GetTitulacionUseCase      │
│  - ExportReportUseCase          - RegisterStudentUseCase    │
└──────────────────────────────┬──────────────────────────────┘
                               │ depende de
┌──────────────────────────────▼──────────────────────────────┐
│                    DOMAIN LAYER (Core Puro)                 │
│  - Entities & Models            - Repository Interfaces     │
│  - Value Objects                - Domain Exceptions / Types │
└──────────────────────────────▲──────────────────────────────┘
                               │ implementa contratos de
┌──────────────────────────────┴──────────────────────────────┐
│               DATA / INFRASTRUCTURE LAYER                   │
│  - API DTOs                     - Mappers (DTO <-> Domain)  │
│  - HttpRepositories (HttpClient)- LocalStorage Adapters     │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. Estructura de Directorios del Proyecto

```
frontend/src/app/
├── domain/                         <- CAPA DOMINIO (TypeScript puro, sin dependencias de Angular)
│   ├── models/                     <- Entidades y modelos inmutables de dominio
│   │   ├── auth.model.ts
│   │   └── titulacion.model.ts
│   └── repositories/               <- Contratos abstractos (Ports)
│       ├── auth.repository.ts
│       └── titulacion.repository.ts
│
├── data/                           <- CAPA DATOS / INFRAESTRUCTURA (Adapters e Implementaciones)
│   ├── dtos/                       <- Contratos exactos que devuelve/recibe el backend API
│   │   ├── auth.dto.ts
│   │   └── titulacion.dto.ts
│   ├── mappers/                    <- Transformadores puros: DTO <-> Domain Entity
│   │   ├── auth.mapper.ts
│   │   └── titulacion.mapper.ts
│   └── repositories/               <- Implementación HTTP de los repositorios del dominio
│       ├── auth-http.repository.ts
│       └── titulacion-http.repository.ts
│
├── application/                    <- CAPA APLICACIÓN (Casos de Uso / Orquestación)
│   └── use-cases/
│       ├── auth/
│       │   ├── login.usecase.ts
│       │   └── logout.usecase.ts
│       └── titulacion/
│           ├── get-titulacion-by-id.usecase.ts
│           └── list-titulaciones.usecase.ts
│
├── presentation/                   <- CAPA PRESENTACIÓN (UI Angular, Signals, Componentes)
│   ├── facades/                    <- State Management / Facades (Conectan UseCases con Signals)
│   │   ├── auth.facade.ts
│   │   └── titulacion.facade.ts
│   ├── pages/                      <- Vistas completas / Smart Components (Rutas)
│   │   ├── login/
│   │   │   └── login.component.ts
│   │   └── titulacion/
│   │       ├── components/         <- Dumb components específicos de la feature
│   │       │   ├── titulacion-table.component.ts
│   │       │   └── titulacion-filter.component.ts
│   │       ├── titulacion.component.ts   <- Smart Page
│   │       ├── titulacion.component.html
│   │       └── titulacion.component.css
│   ├── shared/                     <- UI Components reutilizables (Botones, Modales, Cards, Pipes)
│   │   └── components/
│   └── core/                       <- Interceptors HTTP, Guards, Providers globales
│       ├── guards/
│       │   ├── auth.guard.ts
│       │   └── permission.guard.ts
│       └── interceptors/
│           └── jwt.interceptor.ts
│
├── app.config.ts                   <- Inyección de dependencias (DI Providers: UseClass / Tokens)
└── app.routes.ts                   <- Definición de rutas con Lazy Loading
```

---

## 4. Implementación por Capas (Ejemplo Paso a Paso)

### 4.1. Capa de Dominio (`domain/`)
TypeScript puro. No usa decoradores de Angular ni `HttpClient`.

```typescript
// domain/models/auth.model.ts
export interface UserSession {
  readonly id: number;
  readonly username: string;
  readonly fullName: string;
  readonly email: string;
  readonly roles: readonly string[];
  readonly permissions: Readonly<Record<string, string[]>>;
}

export interface AuthTokens {
  readonly accessToken: string;
  readonly refreshToken: string;
  readonly expiresIn: number;
}

export interface LoginCredentials {
  readonly usernameOrEmail: string;
  readonly password: string;
  readonly systemCode: string;
}
```

```typescript
// domain/repositories/auth.repository.ts
import { Observable } from 'rxjs';
import { AuthTokens, LoginCredentials, UserSession } from '../models/auth.model';

// Usamos abstract class como puerto y token de DI para Angular
export abstract class AuthRepository {
  abstract login(credentials: LoginCredentials): Observable<AuthTokens>;
  abstract getCurrentSession(): Observable<UserSession>;
  abstract refreshToken(refreshToken: string): Observable<AuthTokens>;
  abstract logout(): Observable<void>;
}
```

---

### 4.2. Capa de Datos (`data/`)
Contiene los DTOs de la API, los Mappers y la implementación del Repositorio.

```typescript
// data/dtos/auth.dto.ts
export interface LoginRequestDto {
  username_or_email: string;
  password: string;
  system_code: string;
}

export interface LoginResponseDto {
  access_token: string;
  refresh_token: string;
  expires_in: number;
  token_type: string;
}

export interface UserProfileDto {
  user_id: number;
  username: string;
  display_name: string;
  email: string;
  roles: string[];
  permissions: Record<string, string[]>;
}
```

```typescript
// data/mappers/auth.mapper.ts
import { AuthTokens, LoginCredentials, UserSession } from '../../domain/models/auth.model';
import { LoginRequestDto, LoginResponseDto, UserProfileDto } from '../dtos/auth.dto';

export class AuthMapper {
  static toDto(credentials: LoginCredentials): LoginRequestDto {
    return {
      username_or_email: credentials.usernameOrEmail,
      password: credentials.password,
      system_code: credentials.systemCode
    };
  }

  static toTokens(dto: LoginResponseDto): AuthTokens {
    return {
      accessToken: dto.access_token,
      refreshToken: dto.refresh_token,
      expiresIn: dto.expires_in
    };
  }

  static toSession(dto: UserProfileDto): UserSession {
    return {
      id: dto.user_id,
      username: dto.username,
      fullName: dto.display_name,
      email: dto.email,
      roles: Object.freeze([...dto.roles]),
      permissions: Object.freeze({ ...dto.permissions })
    };
  }
}
```

```typescript
// data/repositories/auth-http.repository.ts
import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { AuthRepository } from '../../domain/repositories/auth.repository';
import { AuthTokens, LoginCredentials, UserSession } from '../../domain/models/auth.model';
import { LoginResponseDto, UserProfileDto } from '../dtos/auth.dto';
import { AuthMapper } from '../mappers/auth.mapper';

@Injectable({ providedIn: 'root' })
export class AuthHttpRepository implements AuthRepository {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/auth';

  login(credentials: LoginCredentials): Observable<AuthTokens> {
    const payload = AuthMapper.toDto(credentials);
    return this.http
      .post<LoginResponseDto>(`${this.baseUrl}/login`, payload)
      .pipe(map(AuthMapper.toTokens));
  }

  getCurrentSession(): Observable<UserSession> {
    return this.http
      .get<UserProfileDto>(`${this.baseUrl}/me`)
      .pipe(map(AuthMapper.toSession));
  }

  refreshToken(refreshToken: string): Observable<AuthTokens> {
    return this.http
      .post<LoginResponseDto>(`${this.baseUrl}/refresh`, { refresh_token: refreshToken })
      .pipe(map(AuthMapper.toTokens));
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/logout`, {});
  }
}
```

---

### 4.3. Capa de Aplicación (`application/use-cases/`)
Encapsula la lógica de orquestación y las reglas de negocio de la aplicación.

```typescript
// application/use-cases/auth/login.usecase.ts
import { inject, Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { AuthRepository } from '../../../domain/repositories/auth.repository';
import { AuthTokens, LoginCredentials } from '../../../domain/models/auth.model';

@Injectable({ providedIn: 'root' })
export class LoginUseCase {
  private readonly authRepository = inject(AuthRepository);

  execute(credentials: LoginCredentials): Observable<AuthTokens> {
    // Validaciones de negocio o normalizaciones previas si aplican
    const normalized: LoginCredentials = {
      ...credentials,
      usernameOrEmail: credentials.usernameOrEmail.trim()
    };

    return this.authRepository.login(normalized).pipe(
      tap(tokens => {
        localStorage.setItem('access_token', tokens.accessToken);
        localStorage.setItem('refresh_token', tokens.refreshToken);
      })
    );
  }
}
```

---

### 4.4. Capa de Presentación (`presentation/`)

#### A. Facade / State Management con Angular Signals
El Facade une los casos de uso con el estado reactivo (`Signals`) que consumirá la vista.

```typescript
// presentation/facades/auth.facade.ts
import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { LoginUseCase } from '../../application/use-cases/auth/login.usecase';
import { LoginCredentials, UserSession } from '../../domain/models/auth.model';
import { AuthRepository } from '../../domain/repositories/auth.repository';

@Injectable({ providedIn: 'root' })
export class AuthFacade {
  private readonly loginUseCase = inject(LoginUseCase);
  private readonly authRepo = inject(AuthRepository);
  private readonly router = inject(Router);

  // Signals privadas de estado
  private readonly _session = signal<UserSession | null>(null);
  private readonly _isLoading = signal<boolean>(false);
  private readonly _errorMessage = signal<string | null>(null);

  // Signals públicas de solo lectura
  readonly session = this._session.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly errorMessage = this._errorMessage.asReadonly();
  readonly isAuthenticated = computed(() => this._session() !== null);
  readonly userFullName = computed(() => this._session()?.fullName ?? '');

  login(credentials: LoginCredentials): void {
    this._isLoading.set(true);
    this._errorMessage.set(null);

    this.loginUseCase.execute(credentials).subscribe({
      next: () => {
        this.loadSession();
      },
      error: (err) => {
        this._isLoading.set(false);
        this._errorMessage.set(err.error?.message ?? 'Credenciales inválidas o error de autenticación');
      }
    });
  }

  loadSession(): void {
    this._isLoading.set(true);
    this.authRepo.getCurrentSession().subscribe({
      next: (session) => {
        this._session.set(session);
        this._isLoading.set(false);
        this.router.navigate(['/dashboard']);
      },
      error: () => {
        this.logout();
        this._isLoading.set(false);
      }
    });
  }

  logout(): void {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    this._session.set(null);
    this.router.navigate(['/login']);
  }

  hasPermission(module: string, operation: string): boolean {
    const permissions = this._session()?.permissions ?? {};
    return permissions[module]?.includes(operation) ?? false;
  }
}
```

#### B. Smart Component (Página contenedora)
```typescript
// presentation/pages/login/login.component.ts
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthFacade } from '../../facades/auth.facade';
import { LoginCredentials } from '../../../domain/models/auth.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  protected readonly authFacade = inject(AuthFacade);

  protected credentials: LoginCredentials = {
    usernameOrEmail: '',
    password: '',
    systemCode: 'TITAN_WEB'
  };

  onSubmit(): void {
    if (!this.credentials.usernameOrEmail || !this.credentials.password) return;
    this.authFacade.login(this.credentials);
  }
}
```

#### C. Template con Control Flow Moderno y Fluent Design
```html
<!-- presentation/pages/login/login.component.html -->
<div class="fluent-card login-card">
  <header class="card-header">
    <h1 class="fluent-title-lg">Acceso al Sistema Titulación ISTPET</h1>
    <p class="fluent-body-muted">Ingrese con sus credenciales institucionales ISTPET</p>
  </header>

  @if (authFacade.errorMessage(); as error) {
    <div class="fluent-alert fluent-alert-danger" role="alert">
      <span>{{ error }}</span>
    </div>
  }

  <form (ngSubmit)="onSubmit()" #loginForm="ngForm" class="fluent-form">
    <div class="fluent-field">
      <label for="username" class="fluent-label">Usuario o Correo</label>
      <input
        id="username"
        name="username"
        type="text"
        class="fluent-input"
        [(ngModel)]="credentials.usernameOrEmail"
        required
        [disabled]="authFacade.isLoading()"
        placeholder="ej. jdoicela@istpet.edu.ec"
      />
    </div>

    <div class="fluent-field">
      <label for="password" class="fluent-label">Contraseña</label>
      <input
        id="password"
        name="password"
        type="password"
        class="fluent-input"
        [(ngModel)]="credentials.password"
        required
        [disabled]="authFacade.isLoading()"
      />
    </div>

    <button
      type="submit"
      class="fluent-btn-primary"
      [disabled]="loginForm.invalid || authFacade.isLoading()"
    >
      @if (authFacade.isLoading()) {
        <span class="fluent-spinner"></span> Iniciando sesión...
      } @else {
        Ingresar
      }
    </button>
  </form>
</div>
```

---

## 5. Configuración de Inyección de Dependencias (`app.config.ts`)

Conecta las abstracciones del Dominio con las implementaciones de Datos mediante el sistema de DI de Angular:

```typescript
// app.config.ts
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { jwtInterceptor } from './presentation/core/interceptors/jwt.interceptor';
import { AuthRepository } from './domain/repositories/auth.repository';
import { AuthHttpRepository } from './data/repositories/auth-http.repository';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([jwtInterceptor])),

    // Inyección de Clean Architecture: Puerto (Dominio) -> Adaptador (Data)
    { provide: AuthRepository, useClass: AuthHttpRepository }
  ]
};
```

---

## 6. Guards e Interceptors Funcionales (`presentation/core/`)

```typescript
// presentation/core/guards/auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthFacade } from '../../facades/auth.facade';

export const authGuard: CanActivateFn = () => {
  const authFacade = inject(AuthFacade);
  const router = inject(Router);

  if (authFacade.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/login']);
};
```

```typescript
// presentation/core/interceptors/jwt.interceptor.ts
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthFacade } from '../../facades/auth.facade';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('access_token');

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        inject(AuthFacade).logout();
      }
      return throwError(() => error);
    })
  );
};
```

---

## 7. Tabla Comparativa de Responsabilidades

| Capa | Ubicación | Depende de | ¿Tiene Angular? | Responsabilidad Principal |
|---|---|---|---|---|
| **Domain** | `app/domain/` | Ninguna | ❌ No (TypeScript puro) | Modelos inmutables, tipos, contratos de repositorios (`abstract class`). |
| **Data** | `app/data/` | `Domain` | ✅ Sí (`HttpClient`) | DTOs de API, Mappers (DTO ↔ Dominio) y repositorios HTTP. |
| **Application** | `app/application/` | `Domain` | ✅ Sí (`@Injectable`) | Casos de uso (`execute()`), lógica de aplicación, persistencia local. |
| **Presentation** | `app/presentation/` | `Application`, `Domain` | ✅ Sí (Angular UI) | Facades (Signals), Smart Components, Dumb Components, Guards, Interceptors. |

---

## 8. Anti-patrones Prohibidos

| Prohibido | Causa del Problema | Solución Clean Architecture |
|---|---|---|
| Inyectar `HttpClient` en componentes o casos de uso | Acopla la UI a la tecnología de transporte | Inyectar `AuthRepository` (contrato abstracto) implementado en `data/` |
| Usar DTOs directamente en la UI/HTML | Si el backend cambia el schema JSON, se rompen todas las vistas | Mapear DTOs a Modelos de Dominio mediante `Mappers` |
| Importar `data/` o `presentation/` dentro de `domain/` | Rompe la Regla de Dependencia de Clean Architecture | `domain/` es 100% aislado e independiente |
| Componentes con más de 200 líneas con llamadas HTTP y lógica de estado | Monolito inmanejable | Dividir en Facade (Signals) + UseCase + Subcomponentes Dumb |
| Directivas legadas `*ngIf`, `*ngFor`, `*ngSwitch` | Sintaxis deprecada en Angular moderno | Usar `@if`, `@for`, `@switch` |
| Usar `any` en modelos, DTOs o respuestas HTTP | Introduce deuda técnica y bugs silenciosos | Crear DTOs e Interfaces explícitas |
| Modificar el DOM con `document.querySelector` | Bypassa el ciclo de vida y renderizado de Angular | Template bindings, Signals y ViewChild |
| `.subscribe()` anidados dentro de componentes | Fugas de memoria y callback hell | Facades reactivos con Signals o pipe `async` |

---

## 9. Referencias de Código y Archivos Clave

- [app.config.ts](file:///c:/Users/DESARROLLADOR/Desktop/Proyectos/titan/frontend/src/app/app.config.ts)
- [app.routes.ts](file:///c:/Users/DESARROLLADOR/Desktop/Proyectos/titan/frontend/src/app/app.routes.ts)
- [titulacion-ui-design SKILL.md](file:///c:/Users/DESARROLLADOR/Desktop/Proyectos/titan/.agents/skills/titulacion-ui-design/SKILL.md)


