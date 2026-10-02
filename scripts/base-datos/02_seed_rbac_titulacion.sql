-- ==============================================================================
-- SCRIPT SQL DE INICIALIZACIÓN RBAC — SISTEMA DE TITULACIÓN ACADÉMICA (ISTPET)
-- Alineado estrictamente con el catálogo institucional SIGAFI (4 operaciones estándar)
-- ==============================================================================

USE `sigafi_es`;

-- 1. Registrar Sistema TITULACION si no existe
INSERT INTO `rbac_sistema` (`codigo`, `detalle`, `url`, `icono`)
SELECT 'TITULACION', 'Sistema de Titulación Académica ISTPET', 'http://localhost:4200', 'academic-cap'
WHERE NOT EXISTS (SELECT 1 FROM `rbac_sistema` WHERE `codigo` = 'TITULACION');

SET @id_sistema_titulacion = (SELECT `idSistema` FROM `rbac_sistema` WHERE `codigo` = 'TITULACION' LIMIT 1);

-- 2. Registrar Módulos Funcionales exclusivos de Titulación
INSERT INTO `rbac_modulos` (`id_sistema`, `Nombre`, `esActivo`)
VALUES
(@id_sistema_titulacion, 'Configuración y Seguridad RBAC', 1),
(@id_sistema_titulacion, 'Gestión de Alumnos y Docentes', 1),
(@id_sistema_titulacion, 'Postulaciones de Titulación', 1),
(@id_sistema_titulacion, 'Examen Complexivo', 1),
(@id_sistema_titulacion, 'Trabajos de Integración Curricular', 1),
(@id_sistema_titulacion, 'Tribunales y Defensas de Grado', 1),
(@id_sistema_titulacion, 'Actas de Grado y Titulación', 1)
ON DUPLICATE KEY UPDATE `esActivo` = 1;

-- 3. Vincular Módulos con las 4 Operaciones Estándar Existentes (ver=1, editar=2, crear=3, eliminar=4)
INSERT IGNORE INTO `rbac_modulos_operaciones` (`idModulos`, `idOperaciones`, `esActivo`, `fecha_creacion`)
SELECT m.`idModulos`, o.`idOperaciones`, 1, CURDATE()
FROM `rbac_modulos` m
CROSS JOIN `rbac_operaciones` o
WHERE m.`id_sistema` = @id_sistema_titulacion
  AND o.`NombreOperacion` IN ('ver', 'crear', 'editar', 'eliminar');

-- 4. Asegurar Rol de Administrador General de Titulación (si no existe)
INSERT INTO `rbac_rol` (`codigo_rol`, `Nombre`, `esActivo`)
VALUES
('TITULACION_ADMIN', 'Administrador General de Titulación', 1)
ON DUPLICATE KEY UPDATE `esActivo` = 1;

-- 5. Matriz de Permisos (rbac_rol_modulo_operacion)

-- 5.1 Rol Administrador de Titulación -> Todas las 4 operaciones en todos los módulos de TITULACION
INSERT IGNORE INTO `rbac_rol_modulo_operacion` (`idRol`, `idModulosOperaciones`, `esActivo`, `fecha_asignacion`)
SELECT r.`idRol`, mo.`idModulosOperaciones`, 1, CURDATE()
FROM `rbac_rol` r
JOIN `rbac_modulos_operaciones` mo ON 1=1
JOIN `rbac_modulos` m ON mo.`idModulos` = m.`idModulos`
WHERE r.`codigo_rol` IN ('TITULACION_ADMIN', 'ADMINISTRADOR', 'ADMIN_SIST')
  AND m.`id_sistema` = @id_sistema_titulacion;

-- 5.2 Rol Docente Institucional -> Operaciones ver, editar y crear en módulos académicos
INSERT IGNORE INTO `rbac_rol_modulo_operacion` (`idRol`, `idModulosOperaciones`, `esActivo`, `fecha_asignacion`)
SELECT r.`idRol`, mo.`idModulosOperaciones`, 1, CURDATE()
FROM `rbac_rol` r
JOIN `rbac_modulos_operaciones` mo ON 1=1
JOIN `rbac_modulos` m ON mo.`idModulos` = m.`idModulos`
JOIN `rbac_operaciones` o ON mo.`idOperaciones` = o.`idOperaciones`
WHERE r.`codigo_rol` IN ('docente', 'DOCENTE', 'profesor', 'PROFESOR')
  AND m.`id_sistema` = @id_sistema_titulacion
  AND o.`NombreOperacion` IN ('ver', 'editar', 'crear');

-- 5.3 Rol Alumno Institucional -> Operaciones ver y crear en postulaciones
INSERT IGNORE INTO `rbac_rol_modulo_operacion` (`idRol`, `idModulosOperaciones`, `esActivo`, `fecha_asignacion`)
SELECT r.`idRol`, mo.`idModulosOperaciones`, 1, CURDATE()
FROM `rbac_rol` r
JOIN `rbac_modulos_operaciones` mo ON 1=1
JOIN `rbac_modulos` m ON mo.`idModulos` = m.`idModulos`
JOIN `rbac_operaciones` o ON mo.`idOperaciones` = o.`idOperaciones`
WHERE r.`codigo_rol` IN ('alumno', 'ALUMNO', 'estudiante', 'ESTUDIANTE')
  AND m.`id_sistema` = @id_sistema_titulacion
  AND o.`NombreOperacion` IN ('ver', 'crear');

-- 6. Asignación del rol TITULACION_ADMIN al usuario 0602959553 y administradores
INSERT IGNORE INTO `rbac_usuario_rol` (`idUsuario`, `idRol`, `esActivo`, `fecha_creacion`)
SELECT u.`idUsuario`, (SELECT `idRol` FROM `rbac_rol` WHERE `codigo_rol` = 'TITULACION_ADMIN' LIMIT 1), 1, CURDATE()
FROM `usuarios` u
WHERE (u.`idSigafi` = '0602959553' OR (u.`administrador` = 1 AND u.`activo` = 1));

-- 7. Asegurar banderas de administrador y correo institucional para usuario 0602959553
UPDATE `usuarios`
SET `administrador` = 1,
    `activo` = 1,
    `email_institucional` = IF(NULLIF(`email_institucional`, '') IS NULL, 'pamela.parra@istpet.edu.ec', `email_institucional`)
WHERE `idSigafi` = '0602959553';
