# Matriz de Autorización RBAC y Atributo HasPermission — Sistema Titulación ISTPET

## 1. Modelo de Autorización RBAC (Role-Based Access Control)

El sistema Titulación ISTPET implementa un esquema de control de acceso basado en roles y permisos parametrizables por módulo y operación.

### 1.1. Atributo `[HasPermission("modulo", "operacion")]`

Los endpoints administrativos o con restricción de seguridad están protegidos con el atributo de autorización granular `HasPermissionAttribute`.

```csharp
[HttpPost("roles")]
[HasPermission("SEGURIDAD_RBAC", "CREAR_ROL")]
public async Task<IActionResult> CreateRol([FromBody] CreateRolRequest request, CancellationToken cancellationToken)
{
    var rol = await _rbacManagementService.CreateRolAsync(request.Nombre, request.CodigoRol, cancellationToken);
    return CreatedAtAction(nameof(GetRoles), new { id = rol.idRol }, rol);
}
```

---

## 2. Endpoints del RbacController (`/api/v1/rbac`)

### 2.1. `GET /api/v1/rbac/sistemas`
Lista los sistemas registrados en la plataforma RBAC.

- **Permiso:** Reclama usuario autenticado (`[Authorize]`).
- **Respuesta (`200 OK`):** Lista de sistemas registrados.

---

### 2.2. `GET /api/v1/rbac/sistemas/{idSistema}/modulos`
Lista los módulos y operaciones asociadas filtrados por ID de sistema.

- **Permiso:** Reclama usuario autenticado (`[Authorize]`).
- **Respuesta (`200 OK`):** Estructura jerárquica de módulos y sus operaciones disponibles.

---

### 2.3. `GET /api/v1/rbac/roles`
Obtiene el catálogo de roles activos en el sistema.

- **Permiso:** Reclama usuario autenticado (`[Authorize]`).

---

### 2.4. `POST /api/v1/rbac/roles`
Crea un nuevo rol en la base de datos `rbac_rol`.

- **Permiso requerido:** `SEGURIDAD_RBAC:CREAR_ROL`
- **Cuerpo de Petición:**
```json
{
  "nombre": "Coordinador de Titulación",
  "codigoRol": "COORDINADOR_TITULACION"
}
```

---

### 2.5. `POST /api/v1/rbac/usuarios/{idUsuario}/roles/{idRol}`
Asigna un rol específico a un usuario registrado.

- **Permiso requerido:** `SEGURIDAD_RBAC:ASIGNAR_ROL`

---

### 2.6. `DELETE /api/v1/rbac/usuarios/{idUsuario}/roles/{idRol}`
Remueve un rol asignado a un usuario.

- **Permiso requerido:** `SEGURIDAD_RBAC:DESASIGNAR_ROL`

---

### 2.7. `POST /api/v1/rbac/roles/{idRol}/permisos/{idModuloOperacion}`
Asocia una operación de módulo (`rbac_modulos_operaciones`) a un rol (`rbac_rol_modulo_operacion`).

- **Permiso requerido:** `SEGURIDAD_RBAC:CONFIGURAR_PERMISOS`


