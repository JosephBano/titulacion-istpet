# Sistema de Titulación Académica Titulación ISTPET — Documentación Técnica

## Descripción General

El **Sistema de Titulación Académica Titulación ISTPET** es la plataforma informática encargada de digitalizar de forma integral el proceso de titulación académica del **Instituto Tecnológico Superior Traversari (ISTPET)** en Quito, Ecuador. Su objetivo es migrar la gestión manual y en papel de postulaciones, validación de requisitos, cohortes, registro de calificaciones complexivas, recepción de artículos científicos y verificación de aptitud hacia un expediente digital unificado y auditado.

La solución se compone de un backend desacoplado desarrollado en **.NET 8** bajo **Clean Architecture**, integrado con la base de datos relacional MySQL **`sigafi_es`** (incluyendo el esquema nativo SIGAFI, el módulo de titulación `Tit_*` y el subsistema de seguridad `rbac_*`), y un cliente web reactivo desarrollado en **Angular 22** con componentes Standalone y el sistema de diseño Microsoft Fluent Design 2.

---

## Estructura de la Documentación

### 01. Arquitectura del Sistema (`01-arquitectura/`)
- [01. Arquitectura General .NET 8 y Clean Architecture](01-arquitectura/01-arquitectura-general-net8.md)
- [02. Patrones de Diseño y Convenciones de Código](01-arquitectura/02-patrones-y-convenciones.md)

### 02. Backend y Servicios API REST (`02-backend-api/`)
- [01. Autenticación JWT y Rotación de Refresh Tokens](02-backend-api/01-autenticacion-jwt-y-refresh-tokens.md)
- [02. Matriz de Autorización RBAC y Atributo HasPermission](02-backend-api/02-matriz-rbac-y-permisos.md)
- [03. Especificación de Endpoints (Académico y Actores)](02-backend-api/03-endpoints-academico-y-actores.md)
- [04. Middleware Global y Manejo de Errores ProblemDetails](02-backend-api/04-middleware-global-y-respuestas.md)
- [05. Configuración General, Convocatorias y Flujo Automatizado de Titulación](02-backend-api/05-configuracion-general-y-convocatorias.md)
- [06. Health Checks, Diagnóstico y Suite de Pruebas con Postman](02-backend-api/06-health-checks-y-pruebas-postman.md)

### 03. Base de Datos (`03-base-de-datos/`)
- [01. Esquema Relacional SIGAFI y Módulo Tit_*](03-base-de-datos/01-esquema-sigafi-y-modulo-titulacion.md)
- [02. Modelo Relacional de Seguridad RBAC](03-base-de-datos/02-modelo-rbac-relacional.md)

### 04. Frontend Angular (`04-frontend-angular/`)
- [01. Arquitectura Angular 22 Standalone y Signals](04-frontend-angular/01-arquitectura-angular22-standalone.md)
- [02. Sistema de Diseño Fluent Design 2 e Identidad ISTPET](04-frontend-angular/02-sistema-de-diseno-fluent2-istpet.md)
- [03. Servicios, Interceptores HTTP y Guards de Navegación](04-frontend-angular/03-servicios-interceptores-y-guards.md)
- [04. Resiliencia de Red, Caché Offline y Portal Dinámico de Titulación](04-frontend-angular/04-resiliencia-red-cache-y-portal-titulacion.md)
- [05. Componentes UI Modulares y Reutilizables (Clean Architecture & Apple HIG)](04-frontend-angular/05-componentes-modulares-reutilizables.md)

### 05. Despliegue y Operaciones (`05-despliegue-y-operaciones/`)
- [01. Guía de Ejecución y Configuración del Entorno Local](05-despliegue-y-operaciones/01-guia-ejecucion-local.md)

---

## Tecnologías del Sistema

- **Backend:** C# / .NET 8, ASP.NET Core Web API, Entity Framework Core 8 (Pomelo MySQL), JWT Bearer Authentication, FluentValidation.
- **Frontend:** TypeScript, Angular 22 Standalone Components, RxJS, Angular Signals, Microsoft Fluent Design 2.
- **Base de Datos:** MySQL 8.0 (`sigafi_es`).



