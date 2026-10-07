-- Plantilla configurable de requisición de personal: tabla exclusiva de Titulación.
-- Decisión autorizada: separar su persistencia del sistema de Hoja de Vida.
-- Este script solo crea titul_plantillas_documentos y muestra su estructura.
-- No contiene INSERT, UPDATE, DELETE ni ALTER sobre tablas existentes.
-- No utiliza hdv_plantillas_documentos ni el servicio de guardado de RRHH.
-- Contrato contrastado con el mapeo EF; todavía no ejecutado en la base real.
-- El diagnóstico aportado por el usuario el 2026-10-06 confirma MySQL 5.7.21,
-- usuarios InnoDB con PK idUsuario INT signed y ausencia de la tabla nueva.
-- Primero ejecutar 05_verificar_plantilla_requisicion.sql y revisar resultados:
--   * usuarios usa InnoDB y idUsuario es INT signed, PK de una sola columna;
--   * si titul_plantillas_documentos existe, su contrato coincide con este DDL.
-- CREATE IF NOT EXISTS conserva una tabla existente; no corrige diferencias.
-- No se alteran las tablas complexivas, los adjuntos ni los permisos RBAC.

USE `sigafi_es`;

CREATE TABLE IF NOT EXISTS `titul_plantillas_documentos` (
  `idPlantillaDocumento` INT NOT NULL AUTO_INCREMENT,
  `codigo` VARCHAR(50) NOT NULL,
  `definicionJson` MEDIUMTEXT NOT NULL,
  `revision` INT NOT NULL DEFAULT 1,
  `actualizadoPor` INT NULL,
  `fechaCreacion` DATETIME NOT NULL,
  `fechaActualizacion` DATETIME NOT NULL,
  PRIMARY KEY (`idPlantillaDocumento`),
  UNIQUE KEY `ux_titul_plantillas_documentos_codigo` (`codigo`),
  KEY `ix_titul_plantillas_documentos_actualizado_por` (`actualizadoPor`),
  CONSTRAINT `fk_titul_plantillas_documentos_usuario`
    FOREIGN KEY (`actualizadoPor`)
    REFERENCES `usuarios` (`idUsuario`)
    ON DELETE SET NULL
    ON UPDATE NO ACTION
) ENGINE=InnoDB
  DEFAULT CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

SHOW WARNINGS;
SHOW CREATE TABLE `titul_plantillas_documentos`;

-- La API futura guarda fechas UTC y valida JSON, longitud y revision positiva.
-- No usar CHECK como protección: MySQL 5.7 no hace cumplir esas restricciones.
-- Sin fila inicial: GET entrega la definición base con revision=0;
-- el primer PUT autorizado crea revision=1 y obtiene actualizadoPor del JWT.
-- No insertar la cédula ni un idUsuario ilustrativo como autor de la plantilla.
-- La API de Titulación debe persistir exclusivamente en esta tabla mediante
-- su propio repositorio. No llamar a HojaDeVidaWordGeneratorService ni al
-- endpoint de configuración HDV: ese servicio normaliza IdPlantilla al rango 1-4.
