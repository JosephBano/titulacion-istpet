# Informe Técnico y Estándar — Eliminación de User Secrets y Uso Exclusivo de Appsettings

**Sistema:** Titán ISTPET — Módulo de Configuración y Backend (.NET 8)  
**Fecha de Actualización:** 12 de Agosto de 2026  
**Estado:** Aplicado y Estandarizado  

---

## 1. Antecedentes y Causa del Cambio

Anteriormente, el archivo de proyecto `Titan.Api.csproj` incluía la configuración de **User Secrets** mediante la etiqueta `<UserSecretsId>`. En ASP.NET Core, el proveedor de User Secrets sobreescribe las configuraciones declaradas en `appsettings.json` y `appsettings.Development.json` durante el desarrollo local.

Esto generaba incongruencias cuando las cadenas de conexión o claves JWT configuradas en `appsettings.json` diferían de las almacenadas localmente en `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`.

---

## 2. Decisión Arquitectural y Solución Definitiva

Para alinearse con el flujo de trabajo estándar del equipo y evitar conflictos entre entornos:

1. **Eliminación Total de User Secrets:** Se removió la propiedad `<UserSecretsId>` del archivo [Titan.Api.csproj](file:///c:/Users/DESARROLLADOR/Downloads/titan/backend/src/Titan.Api/Titan.Api.csproj).
2. **Estandarización en Appsettings:** Toda la configuración del backend se gestiona de forma directa e inequívoca a través de:
   - [appsettings.json](file:///c:/Users/DESARROLLADOR/Downloads/titan/backend/src/Titan.Api/appsettings.json) (configuración base y valores por defecto).
   - [appsettings.Development.json](file:///c:/Users/DESARROLLADOR/Downloads/titan/backend/src/Titan.Api/appsettings.Development.json) (ajustes específicos de desarrollo si aplica).
   - Variables de entorno del sistema (para despliegues en servidores o contenedores).

---

## 3. Estructura Estándar de Configuración (`appsettings.json`)

El archivo [appsettings.json](file:///c:/Users/DESARROLLADOR/Downloads/titan/backend/src/Titan.Api/appsettings.json) pasa a ser la fuente única de verdad para el desarrollo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1;Port=3306;Database=sigafi_es;Uid=root;Pwd=12345;"
  },
  "JwtSettings": {
    "SecretKey": "TitanSystemSecretKeyForJwtAuthenticationSuperSecure2026!",
    "Issuer": "TitanApi",
    "Audience": "TitanApp",
    "ExpirationMinutes": "60"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## 4. Reglas de Trabajo para el Equipo

1. **Sin User Secrets:** Queda prohibido volver a agregar `<UserSecretsId>` al archivo `.csproj`.
2. **Modificación de Parámetros:** Cualquier cambio en la base de datos local, usuario, contraseña o puerto debe realizarse directamente en `appsettings.json` o `appsettings.Development.json`.
3. **Control de Versiones:** Si se requiere un archivo de configuración plantilla libre de credenciales de producción, mantener actualizado `appsettings.json.template`.
