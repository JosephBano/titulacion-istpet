# 06. Health Checks, Diagnóstico y Suite de Pruebas con Postman

## 1. Subsistema de Health Checks Enterprise (.NET 8)

El sistema implementa el estándar `Microsoft.Extensions.Diagnostics.HealthChecks` de ASP.NET Core para proveer observabilidad en tiempo real sobre el estado de la aplicación y la conectividad con la base de datos MySQL `SigafiDbContext`.

### 1.1. Endpoints de Diagnóstico
| Endpoint | Tipo | Propósito | Comportamiento |
| :--- | :--- | :--- | :--- |
| `GET /health` | Readiness Probe | Verifica el estado general del API y la conexión activa a MySQL. | Retorna `200 OK` si la BD está conectada (`Healthy`) o `503 Service Unavailable` si la BD está caída (`Unhealthy`). |
| `GET /health/live` | Liveness Probe | Verificación ligera de proceso sin consultar BD. | Retorna `200 OK` inmediatamente para balanceadores de carga / Kubernetes. |

### 1.2. Estructura de Respuesta JSON
```json
{
  "status": "Healthy",
  "totalDurationMs": 12.45,
  "environment": "Development",
  "timestampUtc": "2026-08-31T17:15:00Z",
  "version": "1.0.0",
  "checks": [
    {
      "name": "mysql_sigafi_database",
      "status": "Healthy",
      "durationMs": 11.89,
      "description": null,
      "error": null
    }
  ]
}
```

> **Manejo Seguro en Producción:** En entorno `Development`, los errores de conexión de base de datos se detallan en el campo `error` para facilitar la depuración. En `Production` o `Staging`, los mensajes de error internos se ocultan para evitar fuga de información sensible o cadenas de conexión.

---

## 2. Colección y Entorno Automatizado de Postman

Los artefactos de pruebas para Postman residen en la carpeta [`docs/postman/`](../postman/):
1. **Colección:** `docs/postman/titulacion_istpet.postman_collection.json`
2. **Entorno:** `docs/postman/titulacion_istpet.postman_environment.json`

### 2.1. Configuración del Entorno Postman
- `baseUrl`: `http://localhost:5000` (o `https://localhost:7001`).
- `accessToken`: (Se auto-completa al hacer Login).
- `refreshToken`: (Se auto-completa al hacer Login).

### 2.2. Script Automático de Autenticación
La petición `POST /api/v1/auth/login` contiene en su pestaña **Tests** el script:
```javascript
if (pm.response.code === 200) {
    var data = pm.response.json();
    pm.environment.set("accessToken", data.accessToken);
    pm.environment.set("refreshToken", data.refreshToken);
    console.log("Tokens guardados exitosamente en el entorno Postman");
}
```
Todas las carpetas de la colección están configuradas con **Bearer Token Inheritance (`{{accessToken}}`)**, por lo que al iniciar sesión una sola vez, todas las peticiones a Configuración, Convocatorias, Postulaciones y Actores quedan autenticadas automáticamente.

---

## 3. Suite de Pruebas Automatizadas del Proyecto

El backend cuenta con una estrategia de pruebas multinivel ejecutada mediante `dotnet test`:

1. **Domain Tests (`TitulacionIstpet.Domain.Tests`):** Valida lógica pura de negocio, reglas de roles y asignación de permisos RBAC.
2. **Application Tests (`TitulacionIstpet.Application.Tests`):** Valida casos de uso CQRS, verificador BCrypt, validaciones FluentValidation, consultas de elegibilidad y creación de postulaciones.
3. **Integration Tests (`TitulacionIstpet.IntegrationTests`):**
   - **Aislamiento de Capas (NetArchTest):** Garantiza que Domain no dependa de Infrastructure ni EF Core, y que Controllers no dependan de DbContext directamente.
   - **Health Endpoint Tests:** Verifica la respuesta JSON estructurada y las sondas de liveness y readiness.

### Ejecución de Pruebas:
```bash
cd backend
dotnet test --logger "console;verbosity=normal"
```
*Resultado actual: **117 pruebas ejecutadas, 117 pruebas aprobadas (100% de éxito)**.*
