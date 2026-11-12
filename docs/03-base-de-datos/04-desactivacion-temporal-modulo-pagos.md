# Documento Técnico: Desactivación Preventiva del Módulo "Pagos de Titulación" para Producción

| Metadato | Detalle |
| :--- | :--- |
| **Fecha de Elaboración** | 11 de noviembre de 2026 |
| **Sistema** | Sistema de Titulación Académica ISTPET |
| **Módulo Afectado** | Gestión Financiera / Pagos de Titulación (`app-pagos-tab`) |
| **Estado en Producción** | **Oculto / Desactivado Preventivamente** vía Feature Flag |
| **Script SQL Pendiente** | [`scripts/base-datos/07_pagos_titulacion_modalidad.sql`](../../scripts/base-datos/07_pagos_titulacion_modalidad.sql) |

---

## 1. Contexto y Justificación del Despliegue

Durante la fase de preparación para el pase a producción del Sistema de Titulación Académica, se identificó que la base de datos de producción institucional (`sigafi_es`) aún no cuenta con las tablas DDL ni las estructuras relacionales diseñadas para el módulo de control financiero y pagos.

Si un usuario con rol administrativo, de secretaría o de colecturía accediera a la pestaña **"Pagos de Titulación"**, el backend de .NET 8 (vía Entity Framework Core y `IPagosTitulacionService`) ejecutaría consultas automáticas contra tablas inexistentes en MySQL, arrojando excepciones fatales no controladas (`MySqlException: Table 'sigafi_es.titul_configuracion_pagos_modalidad' doesn't exist` / HTTP 500).

> [!IMPORTANT]
> **Criterio de Calidad Institucional:**  
> *"Es preferible que una funcionalidad en espera de migración de base de datos no sea visible para los usuarios finales, a presentar interfaces con pantallas rotas o errores 500."*

---

## 2. Diagnóstico Técnico de Estructuras Faltantes

El módulo de pagos requiere de forma estricta las siguientes entidades y tablas en la base de datos MySQL `sigafi_es`:

| Tabla Requerida | Propósito Funcional | Llaves Foráneas Críticas |
| :--- | :--- | :--- |
| `titul_configuracion_pagos_modalidad` | Parametriza el arancel base ($700 total, base e inversión) por cada modalidad (Complexivo, Propuesta Tecnológica, Artículo). | `idModalidadTitulacion` ➔ `titul_modalidades`<br>`idEspecie` ➔ `especies` |
| `titul_pagos` | Registra facturas, cuotas, montos abonados, comprobantes PDF y estados (`REGISTRADO`, `APROBADO`, `RECHAZADO`). | `idPostulacionAlumno` ➔ `titul_postulacion_alumnos`<br>`idMatricula` ➔ `matriculas`<br>`idUsuarioRegistro` ➔ `usuarios` |
| `titul_pagos_historial_estados` | Pista de auditoría obligatoria de cambios de estado y motivos de rechazo por Colecturía. | `idPagoTitulacion` ➔ `titul_pagos`<br>`idUsuario` ➔ `usuarios` |
| Registros RBAC | Módulo `'Pagos de Titulación'` y permisos `ver`, `crear`, `editar` para roles `TITULACION_ADMIN`, `SECRETARIA` y `CONTABILIDAD`. | `rbac_modulos`, `rbac_modulos_operaciones`, `rbac_rol_modulo_operacion` |

---

## 3. Solución Implementada (Ocultamiento Limpio vía Feature Flag)

Para garantizar un pase a producción impecable sin errores visuales ni excepciones en consola, se implementó una desactivación mediante un **Feature Flag centralizado** en el frontend de Angular:

### 3.1. Control en `DashboardComponent`
En [`frontend/src/app/pages/dashboard/dashboard.component.ts`](../../frontend/src/app/pages/dashboard/dashboard.component.ts):

```typescript
// Feature flag de Pagos de Titulación: temporalmente desactivado para producción mientras se aplican las migraciones DDL
readonly moduloPagosHabilitado = false;

puedeVerPagos(): boolean {
  if (!this.moduloPagosHabilitado) return false;
  return this.isAdmin() || this.isSecretaria() || this.isContabilidad();
}
```

### 3.2. Protección del Menú Lateral y Enrutamiento
1. **Menú Lateral Oculto:** El bloque de navegación `<h4 class="sidebar-eyebrow">Gestión Financiera</h4>` y el botón `"Pagos de Titulación"` solo se renderizan si `@if (puedeVerPagos())` es verdadero.
2. **Protección de Pestaña Activa:** La vista `@if (puedeVerPagos() && activeTab() === 'pagos')` queda bloqueada.
3. **Protección contra URLs Forzadas:** Si un usuario intenta escribir manualmente `/dashboard?tab=pagos` en la URL o recargar la página, la función `esTabPermitido('pagos')` detecta que el módulo no está disponible y lo redirige automáticamente a su pestaña por defecto (`resumen` o `cronograma`), impidiendo llamadas HTTP al backend de pagos.

---

## 4. Guía de Activación Cuando la Base de Datos esté Lista

Cuando el equipo de Infraestructura / DBA ejecute la migración en el servidor MySQL de producción, el procedimiento de reactivación es de solo 2 pasos:

### Paso 1: Ejecutar el Script SQL Idempotente en MySQL
Ejecutar el script oficial existente en el repositorio:
📁 **Ruta:** [`scripts/base-datos/07_pagos_titulacion_modalidad.sql`](../../scripts/base-datos/07_pagos_titulacion_modalidad.sql)

**Verificación posterior en MySQL:**
```sql
USE `sigafi_es`;

-- 1. Verificar existencia de las tablas
SHOW TABLES LIKE 'titul_%pago%';

-- 2. Verificar parametrizaciones iniciales
SELECT * FROM `titul_configuracion_pagos_modalidad`;

-- 3. Verificar permisos asignados
SELECT r.codigo_rol, m.Nombre AS modulo, o.NombreOperacion
FROM rbac_rol r
JOIN rbac_rol_modulo_operacion rmo ON r.idRol = rmo.idRol
JOIN rbac_modulos_operaciones mo ON rmo.idModulosOperaciones = mo.idModulosOperaciones
JOIN rbac_modulos m ON mo.idModulos = m.idModulos
JOIN rbac_operaciones o ON mo.idOperaciones = o.idOperaciones
WHERE m.Nombre = 'Pagos de Titulación';
```

### Paso 2: Activar el Feature Flag en el Frontend
En [`frontend/src/app/pages/dashboard/dashboard.component.ts`](../../frontend/src/app/pages/dashboard/dashboard.component.ts):

```typescript
// Cambiar a true una vez ejecutado el script en producción:
readonly moduloPagosHabilitado = true;
```

---

## 5. Conclusión

Con esta medida:
- El despliegue a producción es **100% seguro y libre de errores 500**.
- Los módulos principales de Titulación (Portal del Estudiante, Selección de Modalidades, Requisitos Académicos, Validación Docente, Cohortes y Examen Complexivo) operan al 100%.
- El módulo de pagos queda preparado y dormido a nivel de código, listo para encenderse sin refactorizaciones complejas una vez que la base de datos institucional lo soporte.
