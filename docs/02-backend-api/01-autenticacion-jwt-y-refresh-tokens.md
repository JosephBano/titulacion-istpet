# Autenticación JWT y Rotación de Refresh Tokens — Sistema Titulación ISTPET

## 1. Mecanismo de Autenticación

El sistema Titulación ISTPET utiliza JSON Web Tokens (JWT) firmados mediante la clave secreta institucional con el algoritmo **HMAC-SHA256 (HS256)**.

### 1.1. Propiedades del Token JWT

- **Emisor (`iss`):** Definido en `appsettings.json` (`JwtSettings:Issuer`).
- **Audiencia (`aud`):** Definido en `appsettings.json` (`JwtSettings:Audience`).
- **Duración de Access Token:** 60 minutos por defecto.
- **Duración de Refresh Token:** 7 días por defecto.

---

## 2. Flujo de Rotación de Refresh Tokens (Token Rotation)

Para evitar ataques de reutilización de tokens interceptados, los refresh tokens se almacenan en la tabla `rbac_refresh_tokens` de la base de datos `sigafi_es` en forma de hash SHA-256.

```mermaid
sequenceDiagram
    autonumber
    Cliente->>API: POST /api/v1/auth/login (credenciales)
    API->>DB: Valida usuario en usuarios/usuarios_web y genera hash de clave
    DB-->>API: Usuario válido
    API->>DB: Registra nuevo refresh_token en rbac_refresh_tokens (con IP y DeviceInfo)
    API-->>Cliente: 200 OK (AccessToken + RefreshToken)

    Note over Cliente, API: Al caducar el AccessToken (60 min)

    Cliente->>API: POST /api/v1/auth/refresh-token (AccessToken + RefreshToken)
    API->>DB: Busca RefreshToken activo en rbac_refresh_tokens
    API->>DB: Revoca el RefreshToken anterior (marca revoked_at)
    API->>DB: Genera e inserta nuevo RefreshToken
    API-->>Cliente: 200 OK (Nuevo AccessToken + Nuevo RefreshToken)
```

---

## 3. Especificación de Endpoints del AuthController (`/api/v1/auth`)

### 3.1. `POST /api/v1/auth/login`
Inicia sesión y genera el par inicial de tokens.

- **Autorización:** `[AllowAnonymous]`
- **Cuerpo de Petición (`LoginRequestDto`):**
```json
{
  "usernameOrEmail": "estudiante@institutotraversari.edu.ec",
  "password": "Password123!",
  "systemCode": "TITULACION"
}
```
- **Respuesta de Éxito (`200 OK` - `LoginResponseDto`):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

---

### 3.2. `POST /api/v1/auth/refresh-token`
Renueva el par de tokens caducado.

- **Autorización:** `[AllowAnonymous]`
- **Cuerpo de Petición (`RefreshTokenRequestDto`):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
}
```
- **Respuesta de Éxito (`200 OK`):** Retorna `LoginResponseDto` con nuevo token de acceso y nuevo refresh token.

---

### 3.3. `POST /api/v1/auth/logout`
Cierra la sesión del usuario revocando el refresh token especificado.

- **Autorización:** `[Authorize]`
- **Cuerpo de Petición:** `RefreshTokenRequestDto`
- **Respuesta de Éxito (`200 OK`):** `{ "message": "Sesión cerrada exitosamente." }`

---

### 3.4. `GET /api/v1/auth/me`
Retorna los datos del usuario autenticado, sus roles activos y su matriz de permisos.

- **Autorización:** `[Authorize]`
- **Parámetros de Consulta:** `systemCode` (opcional, por defecto `"TITULACION"`).
- **Respuesta de Éxito (`200 OK` - `UserPermissionsDto`):**
```json
{
  "userId": 42,
  "username": "jorge.doicela",
  "email": "estudiante@institutotraversari.edu.ec",
  "roles": ["ESTUDIANTE"],
  "permissions": ["ACADEMICO_CONSULTAR", "ACTORES_CONSULTAR"]
}
```

---

## 4. Flujo de Recuperación de Acceso Institucional

Mecanismo autónomo de 3 pasos para la verificación y restablecimiento de credenciales sin dependencia de soporte técnico manual.

```mermaid
sequenceDiagram
    autonumber
    actor Usuario
    participant Frontend
    participant API as AuthController
    participant DB as SIGAFI (Alumnos/Profesores/Usuarios)

    Usuario->>Frontend: Ingresa Cédula/Documento
    Frontend->>API: POST /api/v1/auth/recuperacion/validar-documento
    API->>DB: Verifica existencia y aptitud para titulación (último nivel/egresado)
    API-->>Frontend: 200 OK (Valido, NombreCompleto)

    Usuario->>Frontend: Ingresa Fecha de Nacimiento
    Frontend->>API: POST /api/v1/auth/recuperacion/validar-fecha-nacimiento
    API->>DB: Comprueba coincidencia con fecha registrada
    API-->>Frontend: 200 OK (Valido, Token firmado temporal HMAC-SHA256 de 15 min)

    Usuario->>Frontend: Ingresa Nueva Contraseña y Confirmación
    Frontend->>API: POST /api/v1/auth/recuperacion/restablecer-contrasena (Documento, Token, NuevaClave)
    API->>API: Valida firma de token y expiración
    API->>DB: Hashea contraseña con BCrypt y actualiza en Usuarios/Alumnos/Profesores
    API-->>Frontend: 200 OK (Exito: true)
    Frontend-->>Usuario: Muestra confirmación y permite inicio de sesión inmediato
```

### 4.1. `POST /api/v1/auth/recuperacion/validar-documento`
Comprueba si el número de cédula o documento corresponde a un estudiante o docente habilitado para el sistema de titulación.

- **Autorización:** `[AllowAnonymous]`
- **Cuerpo (`ValidarDocumentoRecuperacionRequestDto`):**
```json
{
  "numeroDocumento": "1725555377"
}
```
- **Respuesta (`200 OK`):**
```json
{
  "valido": true,
  "mensaje": "Documento verificado correctamente.",
  "nombreCompleto": "Alexander Rafael Martinez Morillo",
  "requiereFechaNacimiento": true
}
```

### 4.2. `POST /api/v1/auth/recuperacion/validar-fecha-nacimiento`
Comprueba la fecha de nacimiento contra los registros institucionales de SIGAFI y emite un token de recuperación temporal firmado con HMAC-SHA256 (válido por 15 minutos).

- **Autorización:** `[AllowAnonymous]`
- **Cuerpo (`ValidarFechaNacimientoRequestDto`):**
```json
{
  "numeroDocumento": "1725555377",
  "fechaNacimiento": "2003-02-22"
}
```
- **Respuesta (`200 OK`):**
```json
{
  "valido": true,
  "mensaje": "Identidad validada exitosamente.",
  "token": "MTcyNTU1NTM3NzoxNzk2NzA...=="
}
```

### 4.3. `POST /api/v1/auth/recuperacion/restablecer-contrasena`
Verifica el token temporal, valida que las contraseñas coincidan y tengan al menos 6 caracteres, y actualiza el hash de la contraseña en las entidades correspondientes.

- **Autorización:** `[AllowAnonymous]`
- **Cuerpo (`RestablecerContrasenaRequestDto`):**
```json
{
  "numeroDocumento": "1725555377",
  "token": "MTcyNTU1NTM3NzoxNzk2NzA...==",
  "nuevaContrasenia": "NuevaClave2026*",
  "confirmarContrasenia": "NuevaClave2026*"
}
```
- **Respuesta (`200 OK`):**
```json
{
  "exito": true,
  "mensaje": "Contraseña restablecida exitosamente. Ya puedes iniciar sesión con tu nueva contraseña."
}
```



