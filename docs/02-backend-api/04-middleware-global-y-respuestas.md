# Middleware Global y Manejo de Errores ProblemDetails — Sistema Titulación ISTPET

## 1. Estándar de Respuesta de Error (RFC 7807)

El backend de Titulación ISTPET captura todas las excepciones no controladas a nivel de middleware global, evitando la fuga de información sensible o trazas de pila desprotegidas en entornos de producción.

Todas las respuestas con código de estado HTTP 4xx o 5xx siguen el estándar **RFC 7807 (ProblemDetails)**.

### 1.1. Estructura de un Error Estandarizado

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Recurso no encontrado",
  "status": 404,
  "detail": "El estudiante con cédula 1723456789 no fue encontrado en la base de datos.",
  "instance": "/api/v1/actores/alumnos/1723456789",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00"
}
```

---

## 2. Matriz de Mapeo de Excepciones

| Excepción / Evento | Código HTTP | Título |
|---|---|---|
| `UnauthorizedAccessException` | `401 Unauthorized` | Credenciales inválidas o sesión expirada. |
| `KeyNotFoundException` / Recurso `null` | `404 Not Found` | Recurso no encontrado. |
| `ValidationException` (`FluentValidation`) | `400 Bad Request` / `422` | Error de validación en parámetros de entrada. |
| `InvalidOperationException` | `400 Bad Request` | Operación de negocio no permitida. |
| Excepción no controlada (`Exception`) | `500 Internal Server Error` | Error interno del servidor. |

---

## 3. Política de Orígenes Cruzados (CORS)

La API define políticas de CORS explícitas configuradas en `Program.cs`:

- **Desarrollo Local:** Permite orígenes explícitos (`http://localhost:4200` para cliente Angular).
- **Entorno Institucional:** Restricción a dominios institucionales autorizados (`*.institutotraversari.edu.ec`). Prohibido el uso de `AllowAnyOrigin()` en despliegues productivos.


