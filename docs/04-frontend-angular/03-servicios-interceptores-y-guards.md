# Servicios, Interceptores HTTP y Guards de Navegación — Sistema Titulación ISTPET

## 1. Interceptores HTTP Funcionales (Angular 22)

El cliente web utiliza interceptores de HTTP basados en funciones (`HttpInterceptorFn`) configurados en `app.config.ts`.

### 1.1. Interceptor de Autenticación (`auth.interceptor.ts`)
Intercepta cada petición saliente hacia la API REST e inyecta la cabecera `Authorization: Bearer <AccessToken>` si el usuario posee una sesión activa.

```typescript
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getAccessToken();

  if (token) {
    const clonedReq = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`)
    });
    return next(clonedReq);
  }

  return next(req);
};
```

---

### 1.2. Interceptor de Manejo de Errores (`error.interceptor.ts`)
Captura respuestas con código de error HTTP (401, 403, 500), procesa la estructura `ProblemDetails` retornada por el backend y redirige a la pantalla de login si la sesión expiró.

---

## 2. Guardias de Navegación (Guards)

### 2.1. Guardia de Autenticación (`auth.guard.ts`)
Verifica si existe un token JWT válido almacenado. Si la sesión no es válida, redirige al usuario hacia `/login`.

```typescript
import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
  return false;
};
```

---

### 2.2. Guardia de Permisos RBAC (`permission.guard.ts`)
Valida si el usuario autenticado posee el permiso específico exigido en la metadata de la ruta (`route.data['permission']`).

---

## 3. Servicios Core de la Aplicación

- **`AuthService`:** Gestiona las llamadas a `POST /api/v1/auth/login`, renovación de tokens, cierre de sesión y almacenamiento seguro en `localStorage`.
- **`RbacService`:** Consulta la matriz de roles y permisos del usuario desde `GET /api/v1/auth/me`.
- **`AcademicoService`:** Consume endpoints de carreras, asignaturas, mallas y períodos lectivos (`/api/v1/academico`).


