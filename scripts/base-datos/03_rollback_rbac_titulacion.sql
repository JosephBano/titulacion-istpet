-- ==============================================================================
-- SCRIPT SQL DE ROLLBACK / LIMPIEZA RBAC — SISTEMA DE TITULACIÓN (ISTPET)
-- Compatible con MySQL 5.7+
-- Este script revierte y elimina de forma limpia y segura TODO lo registrado por 
-- el módulo de Titulación (y pruebas previas de Titán) sin afectar otros sistemas.
-- ==============================================================================

USE `sigafi_es`;

-- 1. Identificar los IDs de sistema a limpiar ('TITULACION' y 'TITAN')
SET @id_sistema_titulacion = (SELECT `idSistema` FROM `rbac_sistema` WHERE `codigo` = 'TITULACION' LIMIT 1);
SET @id_sistema_titan       = (SELECT `idSistema` FROM `rbac_sistema` WHERE `codigo` = 'TITAN' LIMIT 1);

-- 2. Eliminar refresh tokens de prueba generados para roles de Titulación / Titán
-- NOTA: Se ejecuta primero para filtrar por los roles antes de eliminarlos, protegiendo tokens de otros sistemas.
DELETE rt FROM `rbac_refresh_tokens` rt
JOIN `rbac_usuario_rol` ur ON rt.`idUsuario` = ur.`idUsuario`
JOIN `rbac_rol` r ON ur.`idRol` = r.`idRol`
WHERE r.`codigo_rol` IN (
    'TITULACION_ADMIN', 'TITULACION_DOCENTE', 'TITULACION_ESTUDIANTE',
    'TITAN_ADMIN', 'TITAN_ADMINISTRADOR', 'TITAN_DOCENTE', 'TITAN_ESTUDIANTE',
    'TITAN_COORDINADOR', 'TITAN_SECRETARIA'
);

-- 3. Eliminar permisos de la matriz de autorización (rbac_rol_modulo_operacion)
-- Solo se eliminan permisos de los módulos que pertenecen a Titulación / Titán
DELETE rmo FROM `rbac_rol_modulo_operacion` rmo
JOIN `rbac_modulos_operaciones` mo ON rmo.`idModulosOperaciones` = mo.`idModulosOperaciones`
JOIN `rbac_modulos` m ON mo.`idModulos` = m.`idModulos`
WHERE m.`id_sistema` IN (@id_sistema_titulacion, @id_sistema_titan);

-- 4. Eliminar asignaciones de usuario a roles exclusivos de Titulación / Titán (rbac_usuario_rol)
DELETE ur FROM `rbac_usuario_rol` ur
JOIN `rbac_rol` r ON ur.`idRol` = r.`idRol`
WHERE r.`codigo_rol` IN (
    'TITULACION_ADMIN', 'TITULACION_DOCENTE', 'TITULACION_ESTUDIANTE',
    'TITAN_ADMIN', 'TITAN_ADMINISTRADOR', 'TITAN_DOCENTE', 'TITAN_ESTUDIANTE',
    'TITAN_COORDINADOR', 'TITAN_SECRETARIA'
);

-- 5. Eliminar las relaciones modulo-operacion de Titulación (rbac_modulos_operaciones)
DELETE mo FROM `rbac_modulos_operaciones` mo
JOIN `rbac_modulos` m ON mo.`idModulos` = m.`idModulos`
WHERE m.`id_sistema` IN (@id_sistema_titulacion, @id_sistema_titan);

-- 6. Eliminar los módulos de Titulación (rbac_modulos)
DELETE FROM `rbac_modulos` 
WHERE `id_sistema` IN (@id_sistema_titulacion, @id_sistema_titan);

-- 7. Eliminar los roles exclusivos creados para Titulación / Titán (rbac_rol)
-- NOTA: NO se tocan roles globales como ADMINISTRADOR, DOCENTE, ESTUDIANTE, etc.
DELETE FROM `rbac_rol`
WHERE `codigo_rol` IN (
    'TITULACION_ADMIN', 'TITULACION_DOCENTE', 'TITULACION_ESTUDIANTE',
    'TITAN_ADMIN', 'TITAN_ADMINISTRADOR', 'TITAN_DOCENTE', 'TITAN_ESTUDIANTE',
    'TITAN_COORDINADOR', 'TITAN_SECRETARIA'
);

-- 8. Eliminar el registro del sistema en rbac_sistema
DELETE FROM `rbac_sistema` 
WHERE `codigo` IN ('TITULACION', 'TITAN');
