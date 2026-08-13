-- ==============================================================================
-- SCRIPT SQL DE INICIALIZACIÓN RBAC — SISTEMA TITÁN (ISTPET)
-- Este script es la forma de poblar la estructura RBAC
-- ==============================================================================

USE `sigafi_es`;

-- 1. Registrar Sistema TITAN si no existe
INSERT INTO `rbac_sistema` (`codigo`, `detalle`, `url`, `icono`)
SELECT 'TITAN', 'Sistema de Titulación Académica ISTPET', 'http://localhost:4200', 'academic-cap'
WHERE NOT EXISTS (SELECT 1 FROM `rbac_sistema` WHERE `codigo` = 'TITAN');

SET @id_sistema_titan = (SELECT `idSistema` FROM `rbac_sistema` WHERE `codigo` = 'TITAN' LIMIT 1);

-- 2. Registrar Módulos Funcionales de Titulación
INSERT INTO `rbac_modulos` (`id_sistema`, `Nombre`, `esActivo`)
VALUES
(@id_sistema_titan, 'Configuración y Seguridad RBAC', 1),
(@id_sistema_titan, 'Gestión de Alumnos y Docentes', 1),
(@id_sistema_titan, 'Postulaciones de Titulación', 1),
(@id_sistema_titan, 'Examen Complexivo', 1),
(@id_sistema_titan, 'Trabajos de Integración Curricular', 1),
(@id_sistema_titan, 'Tribunales y Defensas de Grado', 1),
(@id_sistema_titan, 'Actas de Grado y Titulación', 1)
ON DUPLICATE KEY UPDATE `esActivo` = 1;

-- 3. Registrar Operaciones Atómicas Maestras
INSERT INTO `rbac_operaciones` (`NombreOperacion`)
VALUES
('CONSULTAR'), ('CREAR'), ('EDITAR'), ('ELIMINAR'),
('APROBAR'), ('RECHAZAR'), ('ASIGNAR_TUTOR'), ('REGISTRAR_NOTAS'), ('SUSCRIBIR_ACTA')
ON DUPLICATE KEY UPDATE `NombreOperacion` = VALUES(`NombreOperacion`);

-- 4. Vincular Módulos con Operaciones en rbac_modulos_operaciones (Todas las combinaciones)
INSERT IGNORE INTO `rbac_modulos_operaciones` (`idModulos`, `idOperaciones`, `esActivo`, `fecha_creacion`)
SELECT m.`idModulos`, o.`idOperaciones`, 1, CURDATE()
FROM `rbac_modulos` m
CROSS JOIN `rbac_operaciones` o
WHERE m.`id_sistema` = @id_sistema_titan;

-- 5. Registrar Roles Institucionales con prefijo TITAN_ (Únicamente los 3 roles principales)
INSERT INTO `rbac_rol` (`codigo_rol`, `Nombre`, `esActivo`)
VALUES
('TITAN_ADMINISTRADOR', 'Administrador General del Sistema', 1),
('TITAN_DOCENTE', 'Docente Tutor y Evaluador de Tribunal', 1),
('TITAN_ESTUDIANTE', 'Estudiante Postulante de Titulación', 1)
ON DUPLICATE KEY UPDATE `Nombre` = VALUES(`Nombre`), `esActivo` = 1;

-- Desactivar/Limpiar roles antiguos si existieran previamente
UPDATE `rbac_rol` 
SET `esActivo` = 0 
WHERE `codigo_rol` IN ('TITAN_COORDINADOR', 'TITAN_SECRETARIA', 'COORDINADOR', 'SECRETARIA');

-- 6. Matriz de Permisos (rbac_rol_modulo_operacion)

-- TITAN_ADMINISTRADOR -> Todos los permisos
INSERT IGNORE INTO `rbac_rol_modulo_operacion` (`idRol`, `idModulosOperaciones`, `esActivo`, `fecha_asignacion`)
SELECT r.`idRol`, mo.`idModulosOperaciones`, 1, CURDATE()
FROM `rbac_rol` r
JOIN `rbac_modulos_operaciones` mo
WHERE r.`codigo_rol` = 'TITAN_ADMINISTRADOR';

-- TITAN_DOCENTE -> Consultar, Editar, Registrar Notas, Suscribir Acta
INSERT IGNORE INTO `rbac_rol_modulo_operacion` (`idRol`, `idModulosOperaciones`, `esActivo`, `fecha_asignacion`)
SELECT r.`idRol`, mo.`idModulosOperaciones`, 1, CURDATE()
FROM `rbac_rol` r
JOIN `rbac_modulos_operaciones` mo ON 1=1
JOIN `rbac_operaciones` o ON mo.`idOperaciones` = o.`idOperaciones`
WHERE r.`codigo_rol` = 'TITAN_DOCENTE'
  AND o.`NombreOperacion` IN ('CONSULTAR', 'EDITAR', 'REGISTRAR_NOTAS', 'SUSCRIBIR_ACTA');

-- TITAN_ESTUDIANTE -> Consultar, Crear
INSERT IGNORE INTO `rbac_rol_modulo_operacion` (`idRol`, `idModulosOperaciones`, `esActivo`, `fecha_asignacion`)
SELECT r.`idRol`, mo.`idModulosOperaciones`, 1, CURDATE()
FROM `rbac_rol` r
JOIN `rbac_modulos_operaciones` mo ON 1=1
JOIN `rbac_operaciones` o ON mo.`idOperaciones` = o.`idOperaciones`
WHERE r.`codigo_rol` = 'TITAN_ESTUDIANTE'
  AND o.`NombreOperacion` IN ('CONSULTAR', 'CREAR');


-- 7. ASIGNACIÓN DINÁMICA DE ROLES A USUARIOS REALES DE LA BASE DE DATOS SIGAFI
-- Asignar rol TITAN_ESTUDIANTE a los usuarios provenientes de la tabla 'alumno'
INSERT IGNORE INTO `rbac_usuario_rol` (`idUsuario`, `idRol`, `esActivo`, `fecha_creacion`)
SELECT u.`idUsuario`, (SELECT `idRol` FROM `rbac_rol` WHERE `codigo_rol` = 'TITAN_ESTUDIANTE' LIMIT 1), 1, CURDATE()
FROM `usuarios` u
WHERE u.`tablaSigafi` = 'alumno' AND u.`activo` = 1;

-- Asignar rol TITAN_DOCENTE a los usuarios provenientes de la tabla 'profesor'
INSERT IGNORE INTO `rbac_usuario_rol` (`idUsuario`, `idRol`, `esActivo`, `fecha_creacion`)
SELECT u.`idUsuario`, (SELECT `idRol` FROM `rbac_rol` WHERE `codigo_rol` = 'TITAN_DOCENTE' LIMIT 1), 1, CURDATE()
FROM `usuarios` u
WHERE u.`tablaSigafi` = 'profesor' AND u.`activo` = 1;

-- Asignar rol TITAN_ADMINISTRADOR a los usuarios administradores (ej. Pamela Parra o administrador = 1)
INSERT IGNORE INTO `rbac_usuario_rol` (`idUsuario`, `idRol`, `esActivo`, `fecha_creacion`)
SELECT u.`idUsuario`, (SELECT `idRol` FROM `rbac_rol` WHERE `codigo_rol` = 'TITAN_ADMINISTRADOR' LIMIT 1), 1, CURDATE()
FROM `usuarios` u
WHERE u.`tablaSigafi` != 'alumno' AND (u.`administrador` = 1 OR u.`idSigafi` = '0602959553') AND u.`activo` = 1;


