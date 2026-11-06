-- ==============================================================================
-- SCRIPT SQL: FLUJO DE PAGOS DE TITULACIÓN POR MODALIDAD, TOPES Y APROBACIÓN
-- Sistema de Titulación Académica ISTPET
-- Compatible con MySQL 5.7.21 e InnoDB utf8mb4
-- ==============================================================================

USE `sigafi_es`;

-- 1. Asegurar roles institucionales RBAC requeridos
INSERT INTO `rbac_rol` (`codigo_rol`, `Nombre`, `esActivo`)
VALUES
('CONTABILIDAD', 'Área de Contabilidad y Colecturía', 1),
('SECRETARIA', 'Secretaría General y Titulación', 1)
ON DUPLICATE KEY UPDATE `esActivo` = 1;

-- 2. Asegurar Módulo Pagos de Titulación en el Sistema TITULACION
SET @id_sistema_titulacion = (SELECT `idSistema` FROM `rbac_sistema` WHERE `codigo` = 'TITULACION' LIMIT 1);

INSERT INTO `rbac_modulos` (`id_sistema`, `Nombre`, `esActivo`)
SELECT @id_sistema_titulacion, 'Pagos de Titulación', 1
WHERE @id_sistema_titulacion IS NOT NULL
ON DUPLICATE KEY UPDATE `esActivo` = 1;

SET @id_modulo_pagos = (SELECT `idModulos` FROM `rbac_modulos` WHERE `id_sistema` = @id_sistema_titulacion AND `Nombre` = 'Pagos de Titulación' LIMIT 1);

-- 3. Vincular operaciones para Pagos de Titulación (ver=1, editar=2, crear=3)
INSERT IGNORE INTO `rbac_modulos_operaciones` (`idModulos`, `idOperaciones`, `esActivo`, `fecha_creacion`)
SELECT @id_modulo_pagos, o.`idOperaciones`, 1, CURDATE()
FROM `rbac_operaciones` o
WHERE @id_modulo_pagos IS NOT NULL
  AND o.`NombreOperacion` IN ('ver', 'crear', 'editar');

-- 4. Asignar permisos por rol:
-- TITULACION_ADMIN y SECRETARIA: ver, crear (registrar pagos)
INSERT IGNORE INTO `rbac_rol_modulo_operacion` (`idRol`, `idModulosOperaciones`, `esActivo`, `fecha_asignacion`)
SELECT r.`idRol`, mo.`idModulosOperaciones`, 1, CURDATE()
FROM `rbac_rol` r
JOIN `rbac_modulos_operaciones` mo ON mo.`idModulos` = @id_modulo_pagos
JOIN `rbac_operaciones` o ON mo.`idOperaciones` = o.`idOperaciones`
WHERE r.`codigo_rol` IN ('TITULACION_ADMIN', 'SECRETARIA')
  AND o.`NombreOperacion` IN ('ver', 'crear');

-- CONTABILIDAD: ver, editar (aprobar / rechazar)
INSERT IGNORE INTO `rbac_rol_modulo_operacion` (`idRol`, `idModulosOperaciones`, `esActivo`, `fecha_asignacion`)
SELECT r.`idRol`, mo.`idModulosOperaciones`, 1, CURDATE()
FROM `rbac_rol` r
JOIN `rbac_modulos_operaciones` mo ON mo.`idModulos` = @id_modulo_pagos
JOIN `rbac_operaciones` o ON mo.`idOperaciones` = o.`idOperaciones`
WHERE r.`codigo_rol` = 'CONTABILIDAD'
  AND o.`NombreOperacion` IN ('ver', 'editar');

-- 5. Tabla de Configuración de Pagos por Modalidad
CREATE TABLE IF NOT EXISTS `titul_configuracion_pagos_modalidad` (
    `idConfiguracionPago` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `idModalidadTitulacion` INT NOT NULL,
    `idPeriodo` VARCHAR(20) NULL,
    `valorBase` DECIMAL(10,2) NOT NULL,
    `maximoInversion` DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    `totalTitulacion` DECIMAL(10,2) NOT NULL DEFAULT 700.00,
    `idEspecie` INT NULL,
    `activo` TINYINT(1) NOT NULL DEFAULT 1,
    `fechaRegistro` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `fechaModificacion` DATETIME NULL,
    KEY `ix_titul_cfg_modalidad` (`idModalidadTitulacion`),
    KEY `ix_titul_cfg_periodo` (`idPeriodo`),
    KEY `ix_titul_cfg_especie` (`idEspecie`),
    CONSTRAINT `fk_titul_cfg_modalidad` FOREIGN KEY (`idModalidadTitulacion`) REFERENCES `titul_modalidades` (`idModalidadTitulacion`),
    CONSTRAINT `fk_titul_cfg_especie` FOREIGN KEY (`idEspecie`) REFERENCES `especies` (`idEspecie`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 6. Tabla de Pagos de Titulación
CREATE TABLE IF NOT EXISTS `titul_pagos` (
    `idPagoTitulacion` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `idPostulacionAlumno` INT NOT NULL,
    `idMatricula` INT NOT NULL,
    `idConfiguracionPago` INT NULL,
    `tipoRubro` VARCHAR(20) NOT NULL, -- 'BASE' o 'INVERSION'
    `numeroFactura` VARCHAR(50) NOT NULL,
    `valor` DECIMAL(10,2) NOT NULL,
    `estado` VARCHAR(20) NOT NULL DEFAULT 'REGISTRADO', -- 'REGISTRADO', 'APROBADO', 'RECHAZADO'
    `fechaRegistro` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `idUsuarioRegistro` INT NOT NULL,
    `observacion` VARCHAR(500) NULL,
    `motivoRechazo` VARCHAR(500) NULL,
    `fechaRevision` DATETIME NULL,
    `idUsuarioRevision` INT NULL,
    `idPagoSigafi` INT NULL,
    `idCredito` INT NULL,
    `comprobanteArchivo` VARCHAR(255) NULL,
    KEY `ix_titul_pagos_postulacion` (`idPostulacionAlumno`),
    KEY `ix_titul_pagos_matricula` (`idMatricula`),
    KEY `ix_titul_pagos_factura` (`numeroFactura`),
    KEY `ix_titul_pagos_estado` (`estado`),
    CONSTRAINT `fk_titul_pagos_postulacion` FOREIGN KEY (`idPostulacionAlumno`) REFERENCES `titul_postulacion_alumnos` (`idPostulacionAlumnos`),
    CONSTRAINT `fk_titul_pagos_matricula` FOREIGN KEY (`idMatricula`) REFERENCES `matriculas` (`idMatricula`),
    CONSTRAINT `fk_titul_pagos_usuario_reg` FOREIGN KEY (`idUsuarioRegistro`) REFERENCES `usuarios` (`idUsuario`),
    CONSTRAINT `fk_titul_pagos_usuario_rev` FOREIGN KEY (`idUsuarioRevision`) REFERENCES `usuarios` (`idUsuario`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 7. Tabla de Historial de Estados de Pago
CREATE TABLE IF NOT EXISTS `titul_pagos_historial` (
    `idPagoHistorial` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `idPagoTitulacion` INT NOT NULL,
    `estadoAnterior` VARCHAR(20) NULL,
    `estadoNuevo` VARCHAR(20) NOT NULL,
    `idUsuario` INT NOT NULL,
    `motivo` VARCHAR(500) NULL,
    `fechaCambio` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    KEY `ix_titul_pagos_historial_pago` (`idPagoTitulacion`),
    CONSTRAINT `fk_titul_pagos_historial_pago` FOREIGN KEY (`idPagoTitulacion`) REFERENCES `titul_pagos` (`idPagoTitulacion`) ON DELETE CASCADE,
    CONSTRAINT `fk_titul_pagos_historial_usuario` FOREIGN KEY (`idUsuario`) REFERENCES `usuarios` (`idUsuario`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 8. Semilla de configuraciones por defecto
-- Examen Complexivo: $700 base, $0 inversión
INSERT INTO `titul_configuracion_pagos_modalidad` (`idModalidadTitulacion`, `idPeriodo`, `valorBase`, `maximoInversion`, `totalTitulacion`, `activo`, `fechaRegistro`)
SELECT m.`idModalidadTitulacion`, NULL, 700.00, 0.00, 700.00, 1, NOW()
FROM `titul_modalidades` m
WHERE (m.`modalidadTitulacion` LIKE '%complexivo%' OR m.`esComplexivo` = '1' OR m.`esComplexivo` = 'true')
  AND NOT EXISTS (
      SELECT 1 FROM `titul_configuracion_pagos_modalidad` c
      WHERE c.`idModalidadTitulacion` = m.`idModalidadTitulacion` AND c.`idPeriodo` IS NULL
  );

-- Propuesta Tecnológica / Trabajos de Integración Curricular: $500 base, hasta $200 inversión
INSERT INTO `titul_configuracion_pagos_modalidad` (`idModalidadTitulacion`, `idPeriodo`, `valorBase`, `maximoInversion`, `totalTitulacion`, `activo`, `fechaRegistro`)
SELECT m.`idModalidadTitulacion`, NULL, 500.00, 200.00, 700.00, 1, NOW()
FROM `titul_modalidades` m
WHERE (m.`modalidadTitulacion` LIKE '%propuesta%' OR m.`modalidadTitulacion` LIKE '%tecnol%' OR m.`generaTesis` = '1')
  AND NOT EXISTS (
      SELECT 1 FROM `titul_configuracion_pagos_modalidad` c
      WHERE c.`idModalidadTitulacion` = m.`idModalidadTitulacion` AND c.`idPeriodo` IS NULL
  );
