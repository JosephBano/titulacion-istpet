-- ==============================================================================
-- SCRIPT DE MIGRACIÓN: ELIMINAR ROLES REDUNDANTES 37 (TITULACION_DOCENTE) Y 38 (TITULACION_ESTUDIANTE)
-- Unifica las asignaciones hacia los roles institucionales globales 'docente' (Id: 25) y 'alumno' (Id: 15)
-- ==============================================================================

USE `sigafi_es`;

SET @id_sistema_titulacion = (SELECT `idSistema` FROM `rbac_sistema` WHERE `codigo` = 'TITULACION' LIMIT 1);
SET @id_rol_alumno = (SELECT `idRol` FROM `rbac_rol` WHERE `codigo_rol` IN ('alumno', 'ALUMNO', 'estudiante', 'ESTUDIANTE') OR `idRol` = 15 ORDER BY (`idRol` = 15) DESC LIMIT 1);
SET @id_rol_docente = (SELECT `idRol` FROM `rbac_rol` WHERE `codigo_rol` IN ('docente', 'DOCENTE', 'profesor', 'PROFESOR') OR `idRol` = 25 ORDER BY (`idRol` = 25) DESC LIMIT 1);

-- 1. Asegurar que los roles institucionales 'alumno' y 'docente' tengan los permisos correspondientes en el sistema Titulación
-- 1.1 Permisos para Docente Institucional en Titulación
INSERT IGNORE INTO `rbac_rol_modulo_operacion` (`idRol`, `idModulosOperaciones`, `esActivo`, `fecha_asignacion`)
SELECT @id_rol_docente, mo.`idModulosOperaciones`, 1, CURDATE()
FROM `rbac_modulos_operaciones` mo
JOIN `rbac_modulos` m ON mo.`idModulos` = m.`idModulos`
JOIN `rbac_operaciones` o ON mo.`idOperaciones` = o.`idOperaciones`
WHERE m.`id_sistema` = @id_sistema_titulacion
  AND o.`NombreOperacion` IN ('ver', 'editar', 'crear')
  AND @id_rol_docente IS NOT NULL;

-- 1.2 Permisos para Alumno Institucional en Titulación
INSERT IGNORE INTO `rbac_rol_modulo_operacion` (`idRol`, `idModulosOperaciones`, `esActivo`, `fecha_asignacion`)
SELECT @id_rol_alumno, mo.`idModulosOperaciones`, 1, CURDATE()
FROM `rbac_modulos_operaciones` mo
JOIN `rbac_modulos` m ON mo.`idModulos` = m.`idModulos`
JOIN `rbac_operaciones` o ON mo.`idOperaciones` = o.`idOperaciones`
WHERE m.`id_sistema` = @id_sistema_titulacion
  AND o.`NombreOperacion` IN ('ver', 'crear')
  AND @id_rol_alumno IS NOT NULL;

-- 2. Migrar asignaciones de usuarios con rol 37 (TITULACION_DOCENTE) hacia el rol institucional 'docente'
INSERT IGNORE INTO `rbac_usuario_rol` (`idUsuario`, `idRol`, `esActivo`, `fecha_creacion`)
SELECT ur.`idUsuario`, @id_rol_docente, 1, CURDATE()
FROM `rbac_usuario_rol` ur
JOIN `rbac_rol` r ON ur.`idRol` = r.`idRol`
WHERE r.`codigo_rol` = 'TITULACION_DOCENTE' OR r.`idRol` = 37;

-- 3. Migrar asignaciones de usuarios con rol 38 (TITULACION_ESTUDIANTE) hacia el rol institucional 'alumno'
INSERT IGNORE INTO `rbac_usuario_rol` (`idUsuario`, `idRol`, `esActivo`, `fecha_creacion`)
SELECT ur.`idUsuario`, @id_rol_alumno, 1, CURDATE()
FROM `rbac_usuario_rol` ur
JOIN `rbac_rol` r ON ur.`idRol` = r.`idRol`
WHERE r.`codigo_rol` = 'TITULACION_ESTUDIANTE' OR r.`idRol` = 38;

-- 4. Eliminar dependencias de permisos de los roles 37 y 38
DELETE rmo FROM `rbac_rol_modulo_operacion` rmo
JOIN `rbac_rol` r ON rmo.`idRol` = r.`idRol`
WHERE r.`codigo_rol` IN ('TITULACION_DOCENTE', 'TITULACION_ESTUDIANTE')
   OR r.`idRol` IN (37, 38);

-- 5. Eliminar asignaciones de usuario de los roles 37 y 38
DELETE ur FROM `rbac_usuario_rol` ur
JOIN `rbac_rol` r ON ur.`idRol` = r.`idRol`
WHERE r.`codigo_rol` IN ('TITULACION_DOCENTE', 'TITULACION_ESTUDIANTE')
   OR r.`idRol` IN (37, 38);

-- 6. Eliminar los roles redundantes 37 y 38 de la tabla rbac_rol
DELETE FROM `rbac_rol`
WHERE `codigo_rol` IN ('TITULACION_DOCENTE', 'TITULACION_ESTUDIANTE')
   OR `idRol` IN (37, 38);

SELECT 'Migración completada con éxito. Roles 37 y 38 eliminados, permisos unificados en alumno (15) y docente (25).' AS Resultado;
