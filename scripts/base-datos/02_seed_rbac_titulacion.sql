-- ==============================================================================
-- SCRIPT SQL DE INICIALIZACIÓN RBAC — SISTEMA DE TITULACIÓN ACADÉMICA (ISTPET)
-- Este script puebla los módulos, operaciones y permisos
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

-- 3. Registrar Operaciones Atómicas Maestras
INSERT INTO `rbac_operaciones` (`NombreOperacion`)
VALUES
('CONSULTAR'), ('CREAR'), ('EDITAR'), ('ELIMINAR'),
('APROBAR'), ('RECHAZAR'), ('ASIGNAR_TUTOR'), ('REGISTRAR_NOTAS'), ('SUSCRIBIR_ACTA')
ON DUPLICATE KEY UPDATE `NombreOperacion` = VALUES(`NombreOperacion`);

-- 4. Vincular Módulos con Operaciones en rbac_modulos_operaciones (Únicamente para el sistema TITULACION)
INSERT IGNORE INTO `rbac_modulos_operaciones` (`idModulos`, `idOperaciones`, `esActivo`, `fecha_creacion`)
SELECT m.`idModulos`, o.`idOperaciones`, 1, CURDATE()
FROM `rbac_modulos` m
CROSS JOIN `rbac_operaciones` o
WHERE m.`id_sistema` = @id_sistema_titulacion;

-- 5. Asegurar Roles Específicos del Subsistema de Titulación
INSERT INTO `rbac_rol` (`codigo_rol`, `Nombre`, `esActivo`)
VALUES
('TITULACION_ADMIN', 'Administrador General de Titulación', 1),
('TITULACION_DOCENTE', 'Docente Tutor y Evaluador de Titulación', 1),
('TITULACION_ESTUDIANTE', 'Estudiante Postulante de Titulación', 1)
ON DUPLICATE KEY UPDATE `esActivo` = 1;

-- 6. Matriz de Permisos (rbac_rol_modulo_operacion)
-- NOTA: Se filtran estrictamente por m.id_sistema = @id_sistema_titulacion para no alterar otros sistemas.

-- 6.1 Rol Administrador de Titulación -> Acceso a todas las operaciones de TITULACION
INSERT IGNORE INTO `rbac_rol_modulo_operacion` (`idRol`, `idModulosOperaciones`, `esActivo`, `fecha_asignacion`)
SELECT r.`idRol`, mo.`idModulosOperaciones`, 1, CURDATE()
FROM `rbac_rol` r
JOIN `rbac_modulos_operaciones` mo ON 1=1
JOIN `rbac_modulos` m ON mo.`idModulos` = m.`idModulos`
WHERE r.`codigo_rol` IN ('TITULACION_ADMIN', 'ADMINISTRADOR', 'ADMIN_SIST')
  AND m.`id_sistema` = @id_sistema_titulacion;

-- 6.2 Rol Docente de Titulación -> Operaciones académicas de TITULACION
INSERT IGNORE INTO `rbac_rol_modulo_operacion` (`idRol`, `idModulosOperaciones`, `esActivo`, `fecha_asignacion`)
SELECT r.`idRol`, mo.`idModulosOperaciones`, 1, CURDATE()
FROM `rbac_rol` r
JOIN `rbac_modulos_operaciones` mo ON 1=1
JOIN `rbac_modulos` m ON mo.`idModulos` = m.`idModulos`
JOIN `rbac_operaciones` o ON mo.`idOperaciones` = o.`idOperaciones`
WHERE r.`codigo_rol` IN ('TITULACION_DOCENTE', 'DOCENTE', 'PROFESOR')
  AND m.`id_sistema` = @id_sistema_titulacion
  AND o.`NombreOperacion` IN ('CONSULTAR', 'EDITAR', 'REGISTRAR_NOTAS', 'SUSCRIBIR_ACTA');

-- 6.3 Rol Estudiante de Titulación -> Consulta y postulación en TITULACION
INSERT IGNORE INTO `rbac_rol_modulo_operacion` (`idRol`, `idModulosOperaciones`, `esActivo`, `fecha_asignacion`)
SELECT r.`idRol`, mo.`idModulosOperaciones`, 1, CURDATE()
FROM `rbac_rol` r
JOIN `rbac_modulos_operaciones` mo ON 1=1
JOIN `rbac_modulos` m ON mo.`idModulos` = m.`idModulos`
JOIN `rbac_operaciones` o ON mo.`idOperaciones` = o.`idOperaciones`
WHERE r.`codigo_rol` IN ('TITULACION_ESTUDIANTE', 'ESTUDIANTE', 'ALUMNO')
  AND m.`id_sistema` = @id_sistema_titulacion
  AND o.`NombreOperacion` IN ('CONSULTAR', 'CREAR');

-- 7. Asignación del rol TITULACION_ADMIN a usuarios administradores y al usuario 0602959553
INSERT IGNORE INTO `rbac_usuario_rol` (`idUsuario`, `idRol`, `esActivo`, `fecha_creacion`)
SELECT u.`idUsuario`, (SELECT `idRol` FROM `rbac_rol` WHERE `codigo_rol` = 'TITULACION_ADMIN' LIMIT 1), 1, CURDATE()
FROM `usuarios` u
WHERE (u.`idSigafi` = '0602959553' OR (u.`administrador` = 1 AND u.`activo` = 1));
