---
name: titan-frontend
description: Parámetros, reglas y estándares de desarrollo frontend con Angular 19 (Standalone Components, Signals, RxJS, Guards e Interceptors HTTP).
---

# Estándar de Desarrollo Frontend — Titán ISTPET (Angular 19)

Este documento define la arquitectura, convencion de codigo, manejo de estado, integracion HTTP y calidad para el cliente web Angular 19 del sistema **Titan** del Instituto Tecnologico Superior Traversari (ISTPET).

---

## 1. Mentalidad de Arquitecto Frontend Senior — Reglas de Oro

El agente actúa como un **Arquitecto Frontend Senior con 10+ años de experiencia en aplicaciones enterprise Angular**.

- **Principio de Responsabilidad Única (SRP)**: Un archivo debe hacer una sola cosa y hacerla bien.
  - **Componentes (.ts)**: Máximo ~400 líneas (ideal: 100 a 200 líneas). Si supera las 300-400 líneas, extraer subcomponentes o mover lógica a servicios.
  - **Plantillas HTML (.html)**: Máximo ~150-200 líneas (ideal: <100 líneas). Dividir en subcomponentes si la vista es muy extensa.
  - **Estilos (.css / .scss)**: Máximo ~100-150 líneas. Reorganizar en variables globales, utilidades o componentes reutilizables.
  - **Servicios (.ts)**: Máximo ~200-300 líneas. Dividir en subservicios especializados si crece demasiado.
- **Patrón Smart vs. Dumb Components**:
  - **Smart Components (Contenedores/Páginas)**: Inyectan servicios, manejan estados, llaman APIs, coordinan la página. Tienen mínimo CSS propio.
  - **Dumb / Presentational Components (UI Reutilizable)**: Reciben datos via `@Input()` (o `input()`), emiten eventos via `@Output()` (o `output()`). No inyectan servicios de APIs ni manejan lógica pesada.
- **Componentes Declarativos vs. Imperativos**:
  - Cero peticiones HTTP directas en el archivo del componente. La comunicación API va siempre en un `Service`.
  - Cero transformaciones complejas en componentes. Mover formateo/mapeos a servicios de utilidades, funciones puras o Pipes.
- **Sintaxis de Control de Flujo Moderno**: Usar siempre la nueva sintaxis `@if`, `@for` y `@switch` en lugar de las directivas antiguas `*ngIf`, `*ngFor` y `*ngSwitch`.
- **Cero `any` tipados**: Todo modelo, respuesta de API y estado tiene un tipo TypeScript explícito. El uso de `any` introduce deuda técnica invisible — prohibido.
- **Cero manipulación directa del DOM**: Prohibido `document.querySelector`, `element.style`, `innerHTML` y similares. Angular gestiona el DOM — el código no debe puentearlo.
- **Gestión Limpia de Subscripciones / Signals**: Usar Angular Signals para estado local reactivo. En RxJS, evitar `.subscribe()` innecesarios dentro de `.ts`; preferir el pipe `async` en el HTML o señales para evitar fugas de memoria (memory leaks).
- **Causa raíz, no síntomas**: Un bug de estado, una condición de carrera en un observable o un leak de subscripción se resuelve correctamente — nunca con un `setTimeout` o un `try/catch` que silencia el error.
- **Toda la UI se adhiere a `titan-ui-design`**: Prohibido introducir estilos, colores, radios, fuentes o espaciados que no estén definidos en la skill de diseño. Ante duda, consultar esa skill primero.

---

## 2. Arquitectura Standalone — Estructura de Directorios

```
frontend/src/app/
├── core/                  <- Singleton services, interceptores, guards (se carga 1 sola vez)
│   ├── models/            <- Interfaces y tipos TypeScript (contratos del dominio)
│   │   └── auth.models.ts
│   ├── services/          <- Servicios de estado global y comunicación API
│   │   └── auth.service.ts
│   ├── interceptors/      <- Interceptores HTTP funcionales
│   │   └── jwt.interceptor.ts
│   └── guards/            <- Guards funcionales de ruta
│       ├── auth.guard.ts
│       └── permission.guard.ts
│
├── shared/                <- Dumb components reutilizables (botones, modales, cards), pipes, directivas
│   └── components/
│
├── pages/ (o features/)   <- Módulos o rutas funcionales (ej. dashboard, auth, titulacion)
│   └── titulacion/
│       ├── components/    <- Subcomponentes dumb exclusivos de esta feature
│       ├── services/      <- Servicios específicos de esta feature
│       ├── titulacion.component.ts   <- Smart Component principal
│       ├── titulacion.component.html
│       └── titulacion.component.css
│
├── app.config.ts          <- Configuración raíz de Angular (providers)
└── app.routes.ts          <- Definición centralizada de rutas
```

**Regla de dependencias**:
- `pages/` (o `features/`) puede importar de `core/` y `shared/`.
- `core/` no puede importar de `pages/`.
- `shared/` no puede importar de `pages/` ni de `core/services/`.

---

## 3. Convencion de Nombres

| Elemento | Convencion | Ejemplo |
|---|---|---|
| Componente | `[nombre].component.ts` | `login.component.ts` |
| Servicio | `[nombre].service.ts` | `auth.service.ts` |
| Interceptor | `[nombre].interceptor.ts` | `jwt.interceptor.ts` |
| Guard | `[nombre].guard.ts` | `auth.guard.ts` |
| Modelo | `[nombre].models.ts` | `auth.models.ts` |
| Interfaz | `I[Nombre]` en PascalCase | `ILoginRequest` |
| Selector HTML | `app-[kebab-case]` | `app-login` |
| Clase CSS | kebab-case siguiendo `titan-ui-design` | `fluent-card`, `fluent-btn-primary` |

---

## 4. Gestion de Estado — Angular Signals

Angular 19 usa Signals como mecanismo principal de reactividad. Prohibido usar `BehaviorSubject` para estado local de componente cuando un `signal()` es suficiente.

```typescript
// Correcto — estado con Signals
export class AuthService {
  private _currentUser = signal<UserProfile | null>(null);
  private _isLoading   = signal(false);

  // Exponer como readonly para que los componentes no muten directamente
  readonly currentUser = this._currentUser.asReadonly();
  readonly isLoading   = this._isLoading.asReadonly();

  // Computed derivado de señales
  readonly isAuthenticated = computed(() => this._currentUser() !== null);
}

// En componente — lectura reactiva de signals
@Component({ ... })
export class DashboardComponent {
  private authService = inject(AuthService);

  protected user         = this.authService.currentUser;
  protected isAuth       = this.authService.isAuthenticated;
  protected isLoading    = signal(false);
  protected errorMessage = signal<string | null>(null);
}
```

- Usar `inject()` — prohibido constructores verbosos con parametros inyectados.
- `effect()` solo para efectos secundarios con dependencias reactivas (ej. sincronizar con localStorage). No para logica de negocio.
- `computed()` para valores derivados de multiples signals — no recalcular manualmente.

---

## 5. Modelos TypeScript — Contratos Explícitos

Todos los contratos de datos estan en `core/models/`. Nunca usar `any` ni inferir tipos desde respuestas de `HttpClient` sin un tipo explicito.

```typescript
// auth.models.ts
export interface ILoginRequest {
  usernameOrEmail: string;
  password: string;
  systemCode: string;
}

export interface ILoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
}

export interface IUserProfile {
  userId: number;
  username: string;
  email: string;
  displayName: string;
  permissions: Record<string, string[]>;
}
```

- Prefijo `I` para interfaces de contrato de API.
- Sin prefijo para tipos de estado interno del frontend.
- `readonly` en propiedades de interfaces que no deben mutarse.

---

## 6. Servicios — Comunicacion HTTP y Estado

```typescript
@Injectable({ providedIn: 'root' })
export class AuthService {
  private http         = inject(HttpClient);
  private router       = inject(Router);
  private readonly API = '/api/auth';

  private _currentUser = signal<IUserProfile | null>(null);
  readonly currentUser  = this._currentUser.asReadonly();
  readonly isAuthenticated = computed(() => this._currentUser() !== null);

  login(request: ILoginRequest): Observable<ILoginResponse> {
    return this.http.post<ILoginResponse>(`${this.API}/login`, request).pipe(
      tap(response => {
        localStorage.setItem('access_token',  response.accessToken);
        localStorage.setItem('refresh_token', response.refreshToken);
        this.loadUserProfile();
      })
    );
  }

  logout(): void {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    this._currentUser.set(null);
    this.router.navigate(['/login']);
  }

  hasPermission(module: string, operation: string): boolean {
    const perms = this._currentUser()?.permissions ?? {};
    return perms[module]?.includes(operation) ?? false;
  }
}
```

- Todos los metodos HTTP retornan `Observable<T>` — nunca `Promise<T>` para consistencia con el ecosistema Angular.
- Usar `tap()` para efectos secundarios (guardar en localStorage, actualizar signals) dentro de pipes.
- Usar `catchError()` en el pipe del servicio para transformar errores de HTTP en errores de dominio legibles.
- Nunca suscribirse en un servicio con `.subscribe()` — retornar el Observable para que el componente o el guard se suscriban y gestionen la vida de la suscripcion.

---

## 7. Interceptores HTTP — JWT y Refresco Automatico

```typescript
// jwt.interceptor.ts — interceptor funcional (Angular 15+)
export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('access_token');

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        // Intentar refresco de token antes de redirigir
        return inject(AuthService).refreshToken().pipe(
          switchMap(() => next(authReq.clone({
            setHeaders: { Authorization: `Bearer ${localStorage.getItem('access_token')}` }
          }))),
          catchError(() => {
            inject(AuthService).logout();
            return throwError(() => error);
          })
        );
      }
      return throwError(() => error);
    })
  );
};
```

Registro en `app.config.ts`:
```typescript
export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([jwtInterceptor]))
  ]
};
```

---

## 8. Guards — Control de Acceso a Rutas

```typescript
// auth.guard.ts — guard funcional
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router      = inject(Router);

  if (authService.isAuthenticated()) return true;

  return router.createUrlTree(['/login']);
};

// permission.guard.ts — verifica permiso especifico
export const permissionGuard = (module: string, operation: string): CanActivateFn =>
  () => {
    const authService = inject(AuthService);
    const router      = inject(Router);

    if (authService.hasPermission(module, operation)) return true;

    return router.createUrlTree(['/403']);
  };
```

Uso en rutas:
```typescript
// app.routes.ts
export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: 'titulacion',
    canActivate: [authGuard, permissionGuard('titulacion', 'leer')],
    loadComponent: () => import('./pages/titulacion/titulacion.component')
      .then(m => m.TitulacionComponent)
  }
];
```

---

## 9. Formularios — Template-driven vs Reactivos

| Contexto | Usar | Razon |
|---|---|---|
| Login, formularios simples de 2-4 campos | Template-driven (`FormsModule`, `[(ngModel)]`) | Menos codigo, suficiente para formularios simples |
| Formularios complejos, validacion dinamica, multiples pasos | Reactivos (`ReactiveFormsModule`, `FormGroup`) | Control fino de validacion y estado |

```typescript
// Formulario reactivo — ejemplo correcto
export class TitulacionFormComponent {
  private fb = inject(FormBuilder);

  form = this.fb.group({
    cedula:            ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
    nombreCompleto:    ['', Validators.required],
    fechaTitulacion:   ['', Validators.required],
    estadoAcademico:   ['Pendiente de Revision', Validators.required],
  });

  onSubmit(): void {
    if (this.form.invalid) return;
    // delegar al servicio — nunca logica de negocio aqui
  }
}
```

- Mensajes de error en template: usar la directiva de validacion del campo, no un string generico.
- Campos con `Validators.required` muestran error solo si el campo fue tocado (`touched`) — nunca al cargar.

---

## 10. Manejo de Errores HTTP en la UI

- Errores `400 Bad Request`: mostrar el mensaje del campo especifico del backend (`error.errors`).
- Errores `401 Unauthorized`: el interceptor intenta refresh automatico. Si falla, redirige a `/login`.
- Errores `403 Forbidden`: redirigir a pagina `/403` con mensaje institucional.
- Errores `404 Not Found`: mostrar estado vacio en la vista, no redirigir.
- Errores `500+`: mostrar mensaje generico de error de sistema sin exponer detalles tecnicos.

```typescript
// Patron de manejo en componente
this.service.cargarDatos().subscribe({
  next: datos => {
    this.datos.set(datos);
    this.isLoading.set(false);
  },
  error: (err: HttpErrorResponse) => {
    this.isLoading.set(false);
    this.errorMessage.set(
      err.status === 404
        ? 'No se encontraron registros para este criterio.'
        : 'Error al cargar los datos. Intente nuevamente.'
    );
  }
});
```

---

## 11. Lazy Loading y Rendimiento

- Todas las rutas de paginas usan `loadComponent()` — prohibido importar componentes de pagina directamente en `app.routes.ts`.
- El `LoginComponent` es la unica excepcion: se carga de forma eager por ser la ruta inicial.
- Los modulos compartidos pesados (tablas, graficos) van en `shared/` y se importan solo en los componentes que los usan.

---

## 12. Estilos — Integracion con titan-ui-design

- **Styles globales**: `src/styles.css` contiene solo las variables CSS de `paleta_tokens.css` y el reset basico.
- **Estilos de componente**: en el archivo `.css` del componente, usando variables CSS definidas en `titan-ui-design`.
- **Prohibido**: `style=""` inline en templates HTML, `!important`, sobrescritura de estilos globales desde un componente, valores de color hardcodeados que no sean de la paleta ISTPET.

```css
/* Correcto — usa tokens de la skill */
.mi-componente {
  background: var(--bg-surface);
  border: 1px solid var(--border-subtle);
  border-radius: var(--radius-card);
  padding: var(--space-6);
  color: var(--text-primary);
}

/* Prohibido — valores hardcodeados fuera de la paleta */
.mi-componente {
  background: #f0f0f0;
  border-radius: 10px;
  padding: 22px;
  color: #333;
}
```

---

## 13. Anti-patrones Prohibidos

| Prohibido | Correcto |
|---|---|
| `any` en modelos o respuestas HTTP | Interfaces TypeScript explicitas |
| `document.querySelector` o DOM directo | Angular template bindings y refs |
| Lógica de negocio o HTTP directo en componentes | Lógica en servicios (`core/` o de feature) y componentes declarativos |
| Componentes monolíticos (>400 líneas TS, >200 HTML, >150 CSS) | Dividir en subcomponentes Dumb y aplicar SRP |
| Servicios gigantes (>300 líneas) | Dividir en subservicios especializados |
| Directivas antiguas (`*ngIf`, `*ngFor`, `*ngSwitch`) | Nueva sintaxis de control de flujo (`@if`, `@for`, `@switch`) |
| Inyectar servicios API en Dumb Components | Pasar datos via `@Input()` / `input()` y eventos via `@Output()` / `output()` |
| `.subscribe()` masivo en `.ts` causando memory leaks | Usar pipe `async` en HTML o Angular Signals |
| `.subscribe()` en servicios | Retornar `Observable`, suscribir en componente |
| `BehaviorSubject` para estado local simple | `signal()` de Angular |
| Constructores inyectados verbosos | `inject()` funcional |
| Importar componentes de pagina eager en rutas | `loadComponent()` para lazy loading |
| Estilos hardcodeados fuera de la paleta ISTPET | Variables CSS de `titan-ui-design` |
| `Promise<T>` en llamadas HTTP | `Observable<T>` con RxJS |
| Capturar todos los errores con un `try/catch` vacio | Manejo explicito por codigo de estado HTTP |
| `NgModule` | Solo Standalone Components |
| `setTimeout` para resolver condiciones de carrera | Revision de la logica asincrona correcta |

---

## 14. Referencias Internas del Proyecto

- [auth.models.ts](file:///c:/Users/DESARROLLADOR/Downloads/titan/frontend/src/app/core/models/auth.models.ts)
- [auth.service.ts](file:///c:/Users/DESARROLLADOR/Downloads/titan/frontend/src/app/core/services/auth.service.ts)
- [jwt.interceptor.ts](file:///c:/Users/DESARROLLADOR/Downloads/titan/frontend/src/app/core/interceptors/jwt.interceptor.ts)
- [auth.guard.ts](file:///c:/Users/DESARROLLADOR/Downloads/titan/frontend/src/app/core/guards/auth.guard.ts)
- [app.config.ts](file:///c:/Users/DESARROLLADOR/Downloads/titan/frontend/src/app/app.config.ts)
- [app.routes.ts](file:///c:/Users/DESARROLLADOR/Downloads/titan/frontend/src/app/app.routes.ts)
- [titan-ui-design SKILL.md](file:///c:/Users/DESARROLLADOR/Downloads/titan/.agents/skills/titan-ui-design/SKILL.md)
