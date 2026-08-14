# Informe Técnico y Estándar — Eliminación de User Secrets y Uso Exclusivo de Appsettings

**Sistema:** Titulación ISTPET — Módulo de Configuración y Backend (.NET 8)  
**Fecha de Actualización:** 12 de Agosto de 2026  
**Estado:** Aplicado y Estandarizado  

---

## 1. Antecedentes y Causa del Cambio

Anteriormente, el archivo de proyecto `TitulacionIstpet.WebApi.csproj` incluía la configuración de **User Secrets** mediante la etiqueta `<UserSecretsId>`. En ASP.NET Core, el proveedor de User Secrets sobreescribe las configuraciones declaradas en `appsettings.json` y `appsettings.Development.json` durante el desarrollo local.

Esto generaba incongruencias cuando las cadenas de conexión o claves JWT configuradas en `appsettings.json` diferían de las almacenadas localmente en `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`.

---

## 2. Decisión Arquitectural y Solución Definitiva

Para alinearse con el flujo de trabajo estándar del equipo y evitar conflictos entre entornos:

1. **Eliminación Total de User Secrets:** Se removió la propiedad `<UserSecretsId>` del archivo `TitulacionIstpet.WebApi.csproj`.
2. **Estandarización en Appsettings:** Toda la configuración del backend se gestiona de forma directa e inequívoca a través de:
   - `appsettings.json` / `appsettings.Development.json` (valores locales de desarrollo ignorados en git).
   - `appsettings.example.json` (plantilla versionada con placeholders).
   - Variables de entorno del sistema (para despliegues en servidores o contenedores).

---

## 3. Estructura Estándar de Configuración (`appsettings.example.json`)

El archivo plantilla `appsettings.example.json` define la estructura sin exponer contraseñas reales:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=sigafi_es;User Id=TU_USUARIO;Password=TU_PASSWORD;"
  },
  "JwtSettings": {
    "SecretKey": "TU_CLAVE_SECRETA_JWT_MINIMO_32_CARACTERES",
    "Issuer": "Titulación ISTPETApi",
    "Audience": "Titulación ISTPETApp",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
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
2. **Modificación de Parámetros:** Cualquier cambio en la base de datos local, usuario, contraseña o puerto debe realizarse directamente en `appsettings.Development.json` local.
3. **Control de Versiones:** Mantener actualizado siempre `appsettings.example.json` con placeholders genéricos.


