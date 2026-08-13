# Guía de Ejecución y Configuración del Entorno Local — Sistema Titán

## 1. Requisitos Previos del Sistema

Para compilar y ejecutar la plataforma Titán en un entorno de desarrollo local se requieren los siguientes componentes instalados:

- **Plataforma .NET:** SDK de **.NET 8.0** o superior.
- **Entorno Node.js:** Node.js v18.x / v20.x y gestor de paquetes `npm`.
- **Motor de Base de Datos:** **MySQL Server 8.0** activo en el puerto 3306.
- **CLI Angular:** `@angular/cli` v19.x (`npm install -g @angular/cli`).

---

## 2. Configuración de la Base de Datos (`sigafi_es`)

1. **Creación de la Base de Datos:**
   Verificar que la base de datos `sigafi_es` exista en el servidor MySQL local.

2. **Ejecución de Scripts SQL de Estructura e Inicialización:**
   Ejecutar los scripts ubicados en la carpeta `scripts/base-datos/`:

   ```bash
   # 1. Crear estructuras y tablas del módulo de titulación Tit_*
   mysql -u root -p sigafi_es < scripts/base-datos/Titulacion.sql

   # 2. Cargar catálogos iniciales y matriz de permisos RBAC
   mysql -u root -p sigafi_es < scripts/base-datos/01_seed_rbac_titulacion.sql

   # 3. Alta y asignación de rol administrador a Pamela Parra (0602959553)
   mysql -u root -p sigafi_es < scripts/base-datos/02_asignar_admin_pamela_parra.sql
   ```

---

## 3. Configuración y Ejecución del Backend (.NET 8)

1. **Navegar al directorio del backend:**
   ```bash
   cd backend
   ```

2. **Configurar cadena de conexión y JWT (`backend/src/Titan.Api/appsettings.json`):**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=sigafi_es;Uid=root;Pwd=tu_contraseña;Port=3306;"
     },
     "JwtSettings": {
       "SecretKey": "TITAN_ISTPET_SECRET_KEY_SUPER_SECURE_2026_JWT_TOKEN!",
       "Issuer": "titan-api",
       "Audience": "titan-app",
       "AccessTokenExpirationMinutes": 60,
       "RefreshTokenExpirationDays": 7
     }
   }
   ```

3. **Restaurar paquetes y ejecutar la API REST:**
   ```bash
   dotnet restore
   dotnet run --project src/Titan.Api
   ```

   - **Swagger / OpenAPI:** La documentación interactiva estará disponible en `http://localhost:5000/swagger` (o puerto configurado por la aplicación).

---

## 4. Configuración y Ejecución del Frontend (Angular 19)

1. **Navegar al directorio del frontend:**
   ```bash
   cd frontend
   ```

2. **Instalar dependencias de Node:**
   ```bash
   npm install
   ```

3. **Ejecutar el servidor de desarrollo:**
   ```bash
   npm start
   ```

   - **Acceso Web:** El cliente Angular se ejecutará en `http://localhost:4200/`.
