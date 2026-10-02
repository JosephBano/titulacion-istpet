---
name: compilar-proyectos
description: >-
  Automatiza la búsqueda, detección, compilación, resolución de bloqueos de seguridad de Windows
  (Smart App Control / 0x800711C7) y ejecución concurrente de proyectos Frontend (Angular) y Backend (.NET 8).
  Úsalo siempre que el usuario solicite compilar, ejecutar, levantar el servidor, probar ambos proyectos,
  o solucionar problemas de puertos en uso (Address already in use / 10048) o bloqueos de ejecución en Windows.
---

# Skill: Compilar y Ejecutar Proyectos (Fullstack .NET & Angular)

Esta skill automatiza la detección inteligente de rutas, resolución de políticas de seguridad en Windows (Smart App Control), liberación preventiva de puertos y puesta en marcha concurrente del **Backend (.NET 8)** y **Frontend (Angular)**.

---

## 1. Reglas Críticas de Entorno (Windows)

1. **Restricción de Windows Smart App Control en `Downloads` (Error `0x800711C7`):**
   - Windows 11 bloquea por defecto la carga de DLLs y ejecutables compilados localmente dentro de la carpeta `Downloads`.
   - **Solución Automática:** Mantener o sincronizar el backend en la ruta de desarrollo confiable:
     `C:\Users\DESARROLLADOR-PC02\Desktop\titulacion-istpet\backend`
   - Aplicar `Get-ChildItem -Recurse | Unblock-File` tras sincronizar archivos.

2. **Prevención de Puertos Ocupados (Error `SocketException (10048)` / `Address already in use`):**
   - Backend API: Puerto por defecto `5192` (o `7077`).
   - Frontend Web: Puerto por defecto `4200`.
   - Siempre verificar y liberar procesos colgados antes de iniciar nuevas instancias.

---

## 2. Flujo Automatizado de Ejecución

### Paso 1: Detección y Liberación de Puertos
Antes de levantar los servicios, liberar los puertos para evitar conflictos:

```powershell
# Liberar puerto 5192 (Backend) y 4200 (Frontend) si están ocupados
Get-NetTCPConnection -LocalPort 5192,4200 -ErrorAction SilentlyContinue | ForEach-Object {
    Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue
}
```

---

### Paso 2: Backend (.NET 8) - Compilación y Arranque

#### A. Si se edita en `Downloads` y se ejecuta en `Desktop` (Sincronización):
```powershell
# 1. Sincronizar fuentes desde el workspace hacia la carpeta de ejecución de Desktop
$origen = "C:\Users\DESARROLLADOR-PC02\Downloads\Proyectos Desarrollados\Complejos\Sistema Titulacion\backend"
$destino = "C:\Users\DESARROLLADOR-PC02\Desktop\titulacion-istpet\backend"

New-Item -ItemType Directory -Force -Path $destino | Out-Null
Copy-Item -Path "$origen\*" -Destination $destino -Recurse -Force -Exclude "bin","obj"
Get-ChildItem -Path $destino -Recurse | Unblock-File

# 2. Compilar y levantar con Hot-Reload (dotnet watch) o Normal (dotnet run)
cd $destino
dotnet watch --project src\TitulacionIstpet.WebApi\TitulacionIstpet.WebApi.csproj --launch-profile "http"
```

#### B. Si se ejecuta directamente desde `Desktop\titulacion-istpet\backend`:
```powershell
cd "C:\Users\DESARROLLADOR-PC02\Desktop\titulacion-istpet\backend"
dotnet run --project src\TitulacionIstpet.WebApi\TitulacionIstpet.WebApi.csproj --launch-profile "http"
```

---

### Paso 3: Frontend (Angular) - Arranque de Desarrollo

Desde la carpeta `frontend`:
```powershell
cd "C:\Users\DESARROLLADOR-PC02\Downloads\Proyectos Desarrollados\Complejos\Sistema Titulacion\frontend"
# Iniciar servidor de desarrollo
npm start
# O alternativamente:
# npx ng serve --port 4200
```

---

## 3. Flujo de Compilación y Publicación (Producción / Release)

Para generar los paquetes finales listos para desplegar en servidor (IIS o Kestrel):

```powershell
# 1. Publicar Backend (.NET 8)
dotnet publish backend\src\TitulacionIstpet.WebApi\TitulacionIstpet.WebApi.csproj -c Release -o .\publish\backend

# 2. Compilar Frontend (Angular Producción)
cd frontend
npm run build -- --configuration production
cd ..

# 3. Copiar bundle de Angular a carpeta de publicación
New-Item -ItemType Directory -Force -Path ".\publish\frontend" | Out-Null
Copy-Item -Path ".\frontend\dist\titulacion-istpet-web\browser\*" -Destination ".\publish\frontend\" -Recurse -Force
```

---

## 4. Diagnóstico Rápido de Errores Comunes

| Error / Síntoma | Causa | Solución |
| :--- | :--- | :--- |
| `0x800711C7` (Directiva de Control bloqueó este archivo) | Smart App Control bloquea DLLs en `Downloads`. | Ejecutar el backend desde `Desktop\titulacion-istpet\backend` o aplicar `Unblock-File`. |
| `10048` (Address already in use: 5192) | Instancia previa del backend sigue viva en background. | `Get-NetTCPConnection -LocalPort 5192 \| ForEach { Stop-Process -Id $_.OwningProcess -Force }` |
| `MSB1009: El archivo de proyecto no existe` | Ruta relativa ejecutada desde un subdirectorio incorrecto. | Usar rutas completas o situarse en la raíz del proyecto. |
| `401 Unauthorized` en llamadas de prueba | El backend funciona correctamente pero exige JWT válido. | Normal para endpoints protegidos (la API está viva y respondiendo). |
