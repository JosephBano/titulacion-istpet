-- ==============================================================================
-- SCRIPT DE LIMPIEZA DE DATOS: POSTULACIONES DE PRUEBA Y EVIDENCIAS ASOCIADAS
-- Sistema de Titulación - ISTPET
-- ==============================================================================
-- Propósito:
--   Elimina de forma controlada y segura las postulaciones de prueba registradas
--   durante la fase de desarrollo, preservando la configuración de convocatorias,
--   modalidades, requisitos y la tabla global institucional `adjuntos_imagenes`.
--
-- Orden de ejecución respetando integridad referencial:
--   1. Evaluaciones de requisitos emitidas por docentes (`titul_responsable_evidencia`)
--   2. Requisitos asociados a las postulaciones de alumnos (`titul_postulacion_alumnos_requisitos_modalidad`)
--   3. Expedientes de postulación (`titul_postulacion_alumnos`)
--
-- NOTA IMPORTANTE:
--   La tabla `adjuntos_imagenes` NO se modifica por instrucción de gobernanza
--   para garantizar la integridad de otros módulos de SIGAFI.
-- ==============================================================================

USE `sigafi_es`;

START TRANSACTION;

-- 1. Eliminar evaluaciones registradas por docentes sobre los requisitos de las postulaciones
DELETE FROM `titul_responsable_evidencia` 
WHERE `idPostulacionAlumnoRequisitoModalidad` IN (
    SELECT `idPostulacionAlumnoRequisitoModalidad` 
    FROM `titul_postulacion_alumnos_requisitos_modalidad`
);

-- 2. Eliminar requisitos instanciados para las postulaciones de alumnos
DELETE FROM `titul_postulacion_alumnos_requisitos_modalidad`;

-- 3. Eliminar los expedientes de postulación de alumnos de prueba
DELETE FROM `titul_postulacion_alumnos`;

COMMIT;
