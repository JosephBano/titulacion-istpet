-- ==============================================================================
-- SCRIPT DE ASIGNACIÓN DE ROL DOCENTE (TITAN_DOCENTE) A PROFESORES ACTIVOS
-- Sistema Titán — Instituto Tecnológico Superior Traversari (ISTPET)
-- ==============================================================================

USE `sigafi_es`;

-- 1. Crear el rol TITAN_DOCENTE si no existe
INSERT IGNORE INTO `rbac_rol` (`codigo_rol`, `Nombre`, `esActivo`)
VALUES ('TITAN_DOCENTE', 'Docente / Evaluador de Titulación', 1);

-- 2. Asignar el rol TITAN_DOCENTE únicamente a profesores ACTIVOS sin fecha de retiro vencida
INSERT IGNORE INTO `rbac_usuario_rol` (`idUsuario`, `idRol`, `esActivo`, `fecha_creacion`)
SELECT 
    u.`idUsuario`, 
    (SELECT `idRol` FROM `rbac_rol` WHERE `codigo_rol` = 'TITAN_DOCENTE' LIMIT 1), 
    1, 
    CURDATE()
FROM `usuarios` u
INNER JOIN `profesores` p ON p.`idProfesor` = u.`idSigafi`
WHERE (p.`activo` = 1 OR p.`activo` IS NULL)
  AND (p.`fecha_retiro` IS NULL OR p.`fecha_retiro` > CURDATE())
  AND u.`activo` = 1;
