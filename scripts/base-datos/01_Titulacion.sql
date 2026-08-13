-- ==============================================================================
-- TABLAS REALIZADAS EN REUNIÓN
-- ==============================================================================

USE `sigafi_es`;

-- 1. Catálogo de Modalidades de Titulación
CREATE TABLE IF NOT EXISTS `Tit_ModalidadesTitulacion` (
    `idModalidadTitulacion` INT AUTO_INCREMENT PRIMARY KEY,
    `modalidadTitulacion` VARCHAR(100) NOT NULL,
    `esComplexivo` TINYINT DEFAULT 0,
    `esArticuloCientifico` TINYINT DEFAULT 0,
    `generaTesis` TINYINT DEFAULT 0,
    `activo` TINYINT DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2. Modalidades de Titulación habilitadas por Carrera
CREATE TABLE IF NOT EXISTS `Tit_ModalidadesTitulacionCarreras` (
    `idModalidadTitulacionCarrera` INT AUTO_INCREMENT PRIMARY KEY,
    `idCarrera` INT NOT NULL,
    `idModalidad` INT NOT NULL,
    `idModalidadTitulacion` INT NOT NULL,
    `fechaRegistro` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `activo` TINYINT DEFAULT 1,
    `fechaDesactiva` DATETIME DEFAULT NULL,
    FOREIGN KEY (`idCarrera`) REFERENCES `carreras`(`idCarrera`),
    FOREIGN KEY (`idModalidad`) REFERENCES `modalidades`(`idModalidad`),
    FOREIGN KEY (`idModalidadTitulacion`) REFERENCES `Tit_ModalidadesTitulacion`(`idModalidadTitulacion`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3. Cohortes de Titulación
CREATE TABLE IF NOT EXISTS `Tit_Cohortes` (
    `idCohorte` INT AUTO_INCREMENT PRIMARY KEY,
    `idPeriodo` VARCHAR(20) NOT NULL,
    `descripcion` VARCHAR(100) NOT NULL,
    `fechaRegistro` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `activo` TINYINT DEFAULT 1,
    FOREIGN KEY (`idPeriodo`) REFERENCES `periodos`(`idPeriodo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 4. Requisitos Configurables de Titulación
CREATE TABLE IF NOT EXISTS `Tit_Requisitos` (
    `idRequisito` INT AUTO_INCREMENT PRIMARY KEY,
    `requisito` VARCHAR(150) NOT NULL,
    `esArchivo` TINYINT DEFAULT 1,
    `esBit` TINYINT DEFAULT 0,
    `subeAlumno` TINYINT DEFAULT 1,
    `subeColaborador` TINYINT DEFAULT 0,
    `activo` TINYINT DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5. Postulación del Estudiante
CREATE TABLE IF NOT EXISTS `Tit_PostulacionAlumnos` (
    `idPostulacionAlumno` INT AUTO_INCREMENT PRIMARY KEY,
    `idMatricula` INT NOT NULL,
    `idCohorte` INT NOT NULL,
    `idModalidadTitulacionCarrera` INT NOT NULL,
    `fechaRegistro` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `estadoPostulacion` VARCHAR(50) DEFAULT 'PENDIENTE', -- PENDIENTE, APROBADA, RECHAZADA
    `observacion` VARCHAR(500) DEFAULT NULL,
    `activo` TINYINT DEFAULT 1,
    FOREIGN KEY (`idMatricula`) REFERENCES `matriculas`(`idMatricula`),
    FOREIGN KEY (`idCohorte`) REFERENCES `Tit_Cohortes`(`idCohorte`),
    FOREIGN KEY (`idModalidadTitulacionCarrera`) REFERENCES `Tit_ModalidadesTitulacionCarreras`(`idModalidadTitulacionCarrera`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 6. Documentos / Requisitos Subidos por el Estudiante
CREATE TABLE IF NOT EXISTS `Tit_PostulacionAlumnosRequisitos` (
    `idPostulacionAlumnoRequisito` INT AUTO_INCREMENT PRIMARY KEY,
    `idPostulacionAlumno` INT NOT NULL,
    `idRequisito` INT NOT NULL,
    `esRequisitoFinal` TINYINT DEFAULT 0, -- 0: Inicial / Postulación, 1: Requisito Final
    `documentoPdf` VARCHAR(255) DEFAULT NULL,
    `observacion` VARCHAR(500) DEFAULT NULL,
    `fechaRegistro` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `activo` TINYINT DEFAULT 1,
    FOREIGN KEY (`idPostulacionAlumno`) REFERENCES `Tit_PostulacionAlumnos`(`idPostulacionAlumno`),
    FOREIGN KEY (`idRequisito`) REFERENCES `Tit_Requisitos`(`idRequisito`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 7. Calificaciones de Examen Complexivo
CREATE TABLE IF NOT EXISTS `Tit_CalificacionesComplexivo` (
    `idCalificacionComplexivo` INT AUTO_INCREMENT PRIMARY KEY,
    `idPostulacionAlumno` INT NOT NULL,
    `calificacionTeorica` DECIMAL(5,2) DEFAULT 0.00,
    `calificacionPractica` DECIMAL(5,2) DEFAULT 0.00,
    `calificacionDefensa` DECIMAL(5,2) DEFAULT 0.00,
    `promedioFinal` DECIMAL(5,2) DEFAULT 0.00,
    `fechaRegistro` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `idUsuarioRegistra` INT NOT NULL,
    FOREIGN KEY (`idPostulacionAlumno`) REFERENCES `Tit_PostulacionAlumnos`(`idPostulacionAlumno`),
    FOREIGN KEY (`idUsuarioRegistra`) REFERENCES `usuarios`(`idUsuario`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 8. Artículos Científicos
CREATE TABLE IF NOT EXISTS `Tit_ArticuloCientifico` (
    `idArticuloCientifico` INT AUTO_INCREMENT PRIMARY KEY,
    `idPostulacionAlumno` INT NOT NULL,
    `revista` VARCHAR(150) NOT NULL,
    `impacto` VARCHAR(100) DEFAULT NULL,
    `tema` VARCHAR(1000) NOT NULL,
    `enlace` VARCHAR(500) DEFAULT NULL,
    `archivoPdf` VARCHAR(255) DEFAULT NULL,
    `fechaRegistro` DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (`idPostulacionAlumno`) REFERENCES `Tit_PostulacionAlumnos`(`idPostulacionAlumno`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
