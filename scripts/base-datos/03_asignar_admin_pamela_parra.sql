-- ==============================================================================
-- SCRIPT DE ALTA Y ASIGNACIÓN DE ROL ADMINISTRADOR — PAMELA PARRA (0602959553)
-- Sistema Titán — Instituto Tecnológico Superior Traversari (ISTPET)
-- ==============================================================================

USE `sigafi_es`;

-- 1. Insertar o actualizar credenciales (Hash SHA256 de 'Admin123!') para idSigafi '0602959553'
INSERT INTO `usuarios` (
    `idSigafi`, 
    `tablaSigafi`, 
    `nombre`, 
    `contrasenia`, 
    `activo`, 
    `administrador`, 
    `emailInstitucional`
)
VALUES (
    '0602959553', 
    'profesor', 
    'PAMELA PARRA', 
    '3eb3fe66b31e3b4d10fa70b5cad49c7112294af6ae4e476a1c405155d45aa121', 
    1, 
    1, 
    'pameparralema@hotmail.com'
)
ON DUPLICATE KEY UPDATE 
    `contrasenia` = '3eb3fe66b31e3b4d10fa70b5cad49c7112294af6ae4e476a1c405155d45aa121',
    `activo` = 1,
    `administrador` = 1,
    `emailInstitucional` = 'pameparralema@hotmail.com';

-- Forzar actualización por idSigafi en caso de registro previo
UPDATE `usuarios` 
SET 
    `contrasenia` = '3eb3fe66b31e3b4d10fa70b5cad49c7112294af6ae4e476a1c405155d45aa121',
    `activo` = 1,
    `administrador` = 1
WHERE `idSigafi` = '0602959553';

-- 2. Asignar el rol de Administrador (TITAN_ADMINISTRADOR) en la tabla rbac_usuario_rol
INSERT IGNORE INTO `rbac_usuario_rol` (`idUsuario`, `idRol`, `esActivo`, `fecha_creacion`)
SELECT 
    u.`idUsuario`, 
    r.`idRol`, 
    1, 
    CURDATE()
FROM `usuarios` u
JOIN `rbac_rol` r ON r.`codigo_rol` IN ('TITAN_ADMINISTRADOR', 'ADMINISTRADOR', 'ADMIN_SIST')
WHERE u.`idSigafi` = '0602959553' AND u.`tablaSigafi` = 'profesor';


