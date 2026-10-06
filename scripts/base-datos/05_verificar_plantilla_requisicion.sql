-- Diagnóstico de solo lectura para MySQL Workbench. No crea tablas ni permisos.
-- Ejecutar antes de 06_plantilla_requisicion.sql y conservar los resultados.
-- Nombres contrastados con SigafiDbContext y RbacService del repositorio.

USE `sigafi_es`;

-- 1. Versión y configuración. El DDL siguiente admite MySQL 5.7 / 8.0.
SELECT VERSION() AS version_mysql,
       DATABASE() AS base_seleccionada,
       @@lower_case_table_names AS lower_case_table_names,
       @@session.time_zone AS zona_horaria_sesion;

-- 2. Plantillas disponibles y dependencias. La tabla nueva puede no existir.
SELECT TABLE_NAME, ENGINE, TABLE_COLLATION
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = 'sigafi_es'
  AND (TABLE_NAME LIKE '%plantill%'
       OR TABLE_NAME IN ('usuarios', 'adjuntos_imagenes',
                        'titul_modulos_complexivo', 'titul_temas',
                        'titul_malla_complexivo', 'titul_malla_docente_complexivo',
                        'titul_modulos_horarios'))
ORDER BY TABLE_NAME;

-- 3. Contratos de plantillas y PK del usuario; no se leen contraseñas ni cuerpos.
SELECT TABLE_NAME, ORDINAL_POSITION, COLUMN_NAME, COLUMN_TYPE,
       IS_NULLABLE, COLUMN_DEFAULT, COLUMN_KEY, EXTRA
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = 'sigafi_es'
  AND (TABLE_NAME LIKE '%plantill%'
       OR (TABLE_NAME = 'usuarios' AND COLUMN_NAME = 'idUsuario')
       OR TABLE_NAME = 'adjuntos_imagenes')
ORDER BY TABLE_NAME, ORDINAL_POSITION;

SELECT TABLE_NAME, INDEX_NAME, NON_UNIQUE, SEQ_IN_INDEX, COLUMN_NAME
FROM information_schema.STATISTICS
WHERE TABLE_SCHEMA = 'sigafi_es'
  AND (TABLE_NAME LIKE '%plantill%'
       OR (TABLE_NAME = 'usuarios' AND INDEX_NAME = 'PRIMARY'))
ORDER BY TABLE_NAME, INDEX_NAME, SEQ_IN_INDEX;

-- 4. Compatibilidad de la FK de actualizadoPor. APTO no valida el esquema nuevo.
SELECT CASE
         WHEN EXISTS (
           SELECT 1 FROM information_schema.TABLES
           WHERE TABLE_SCHEMA = 'sigafi_es' AND TABLE_NAME = 'usuarios'
             AND ENGINE = 'InnoDB'
         ) AND EXISTS (
           SELECT 1 FROM information_schema.COLUMNS
           WHERE TABLE_SCHEMA = 'sigafi_es' AND TABLE_NAME = 'usuarios'
             AND COLUMN_NAME = 'idUsuario' AND DATA_TYPE = 'int'
             AND COLUMN_TYPE NOT LIKE '%unsigned%'
             AND COLUMN_KEY = 'PRI' AND IS_NULLABLE = 'NO'
         ) AND (
           SELECT COUNT(*) FROM information_schema.KEY_COLUMN_USAGE
           WHERE TABLE_SCHEMA = 'sigafi_es' AND TABLE_NAME = 'usuarios'
             AND CONSTRAINT_NAME = 'PRIMARY'
         ) = 1
         THEN 'APTO: usuarios.idUsuario es INT signed y la tabla usa InnoDB'
         ELSE 'REVISAR: no aplicar 06 hasta ajustar la FK al esquema real'
       END AS compatibilidad_autoria;

SHOW CREATE TABLE `usuarios`;

-- Si la tabla nueva ya existe, comparar su contrato completo antes de aplicar 06.
SELECT TABLE_NAME, CONSTRAINT_NAME, COLUMN_NAME,
       REFERENCED_TABLE_NAME, REFERENCED_COLUMN_NAME
FROM information_schema.KEY_COLUMN_USAGE
WHERE TABLE_SCHEMA = 'sigafi_es'
  AND TABLE_NAME = 'titul_plantillas_documentos'
ORDER BY CONSTRAINT_NAME, ORDINAL_POSITION;

-- 5. Cuenta y roles asignados. La cédula se usa solo para este diagnóstico.
SELECT u.`idUsuario`, u.`idSigafi`, u.`Nombre`, u.`activo`, u.`administrador`,
       r.`codigo_rol`, ur.`esActivo` AS asignacion_activa,
       r.`esActivo` AS rol_activo
FROM `usuarios` u
LEFT JOIN `rbac_usuario_rol` ur ON ur.`idUsuario` = u.`idUsuario`
LEFT JOIN `rbac_rol` r ON r.`idRol` = ur.`idRol`
WHERE u.`idSigafi` = '0602959553'
ORDER BY r.`codigo_rol`;

-- Catálogo del módulo; deben estar activas las operaciones ver y editar.
SELECT s.`codigo` AS sistema, m.`Nombre` AS modulo,
       m.`esActivo` AS modulo_activo, o.`NombreOperacion` AS operacion,
       mo.`esActivo` AS operacion_modulo_activa
FROM `rbac_sistema` s
JOIN `rbac_modulos` m ON m.`id_sistema` = s.`idSistema`
JOIN `rbac_modulos_operaciones` mo ON mo.`idModulos` = m.`idModulos`
JOIN `rbac_operaciones` o ON o.`idOperaciones` = mo.`idOperaciones`
WHERE s.`codigo` = 'TITULACION' AND m.`Nombre` = 'Examen Complexivo'
ORDER BY o.`NombreOperacion`;

-- Permisos activos otorgados a la cuenta por sus roles, como en RbacService.
-- Si administrador=1 y no hay permisos de ningún módulo, RbacService aplica
-- su fallback; comprobar también la sesión/API real antes de cambiar permisos.
SELECT DISTINCT r.`codigo_rol`, m.`Nombre` AS modulo,
                o.`NombreOperacion` AS operacion
FROM `usuarios` u
JOIN `rbac_usuario_rol` ur ON ur.`idUsuario` = u.`idUsuario` AND ur.`esActivo` = 1
JOIN `rbac_rol` r ON r.`idRol` = ur.`idRol` AND r.`esActivo` = 1
JOIN `rbac_rol_modulo_operacion` rmo
  ON rmo.`idRol` = r.`idRol` AND rmo.`esActivo` = 1
JOIN `rbac_modulos_operaciones` mo
  ON mo.`idModulosOperaciones` = rmo.`idModulosOperaciones` AND mo.`esActivo` = 1
JOIN `rbac_modulos` m ON m.`idModulos` = mo.`idModulos` AND m.`esActivo` = 1
JOIN `rbac_sistema` s ON s.`idSistema` = m.`id_sistema`
JOIN `rbac_operaciones` o ON o.`idOperaciones` = mo.`idOperaciones`
WHERE u.`idSigafi` = '0602959553' AND u.`activo` = 1
  AND s.`codigo` = 'TITULACION' AND m.`Nombre` = 'Examen Complexivo'
ORDER BY r.`codigo_rol`, o.`NombreOperacion`;
