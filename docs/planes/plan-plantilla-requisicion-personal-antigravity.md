# Plan de implementación: plantilla configurable de requisición de personal

Fecha: 6 de octubre de 2026. Proyecto: Titulación ISTPET.

Este documento es el traspaso de trabajo para el agente de Antigravity. Contiene
las decisiones propuestas, el estado real del código, las fases pendientes y
las condiciones para considerar terminada la implementación.

**Preparación de MySQL:** guardar la plantilla compartida requiere crear
`titul_plantillas_documentos` antes de probar su API. El botón de precarga ya
implementado no necesita cambios de base de datos. El usuario ejecutó el diagnóstico
en Workbench y compartió los resultados el 6 de octubre de 2026: MySQL 5.7.21,
FK compatible y permisos de Pamela correctos. La tabla nueva no aparece en el
esquema consultado. El script de creación `06` está preparado y su aplicación
sigue pendiente; Antigravity debe verificarla al iniciar la fase 2.

**Decisión final del usuario:** utilizar una tabla exclusiva de Titulación para
mantener independiente el almacenamiento del sistema de Hoja de Vida. El usuario
aportó también el `SHOW CREATE TABLE` de `hdv_plantillas_documentos` y confirmó
que su consulta de triggers no devolvió filas. La sección 4 documenta la revisión
del consumidor de RRHH y la separación que debe conservar la implementación.

## 1. Resultado esperado

Pamela y los administradores autorizados podrán configurar el formato de
requisición desde una interfaz: título, textos, etiquetas, colores, fondo,
logotipo y valores iniciales. La plantilla se guardará como una definición JSON
validada en MySQL. Sus cambios se conservarán al recargar y se compartirán con
los usuarios autorizados del sistema.

Desde el curso de preparación complexivo, **Solicitar requisición de personal**
abrirá un formulario nuevo con el cargo y las vacantes calculados a partir de
los módulos sin docente de la cohorte/carrera seleccionada. El formulario será
editable y generará un PDF con el formato configurado.

La definición del formato y los datos de una solicitud tienen ciclos diferentes:

| Objeto                  | Contenido                                                             | Persistencia de esta entrega                         |
| ----------------------- | --------------------------------------------------------------------- | ---------------------------------------------------- |
| Plantilla               | Apariencia, etiquetas, opciones, textos y reglas de valores iniciales | MySQL, compartida y editable por permisos            |
| Contexto del curso      | Carrera, cohorte, módulos sin docente y vacantes calculadas           | Derivado de la planificación existente               |
| Borrador de requisición | Solicitante, cargo, vacantes, perfil, remuneración y demás respuestas | Formulario en memoria; PDF conservado por el usuario |

El botón prepara un borrador. La aprobación y las tres firmas siguen siendo
manuales. Una futura gestión de expedientes de RRHH tendrá sus propias tablas,
estados y permisos; no es necesaria para configurar el formato.

## 2. Estado actual y trabajo ya implementado

### Funcionalidad existente

- Angular standalone, signals, formularios `NgForm` y date pickers en español.
- El dashboard navega mediante `activeTab` / `setActiveTab`; estos componentes
  todavía no son rutas individuales del router.
- `RequisicionPersonalComponent` contiene las etiquetas, catálogos y el formato
  en código. Genera una vista previa HTML A4 en un diálogo centrado y su impresión
  utiliza `print-document.ts`.
- La requisición no tiene una API de persistencia ni una tabla de solicitudes.
- La planificación del curso utiliza las cinco tablas complexivas existentes.
  `idProfesor = null` permite conservar horarios con asignación docente pendiente.
- La impresión de planificación ya genera PDF vectorial con `jsPDF` y
  `jspdf-autotable`, con bordes de 0,35 mm y colores por módulo.

### Implementado con este traspaso

1. Botón **Solicitar requisición de personal** en la barra de acciones del curso.
2. Conteo de vacantes en módulos activos sin docentes en asignaciones activas.
3. Evento tipado `requisicionSolicitada` con cargo, vacantes y contexto de origen.
4. El dashboard abre la pestaña de requisición y transmite `datosIniciales`.
5. La requisición inicializa cargo y número de vacantes; conserva la fecha
   actual, el solicitante autenticado y la edición manual de los campos.
6. Al salir de requisición se limpia la precarga del dashboard. Al volver desde
   el sidebar se mantiene el comportamiento de un formulario nuevo.

**Pendiente para Antigravity:** la plantilla editable, su API/persistencia,
el manejo de imágenes y la generación PDF de requisición descritos abajo.
El JSON de ejemplo de este directorio es una propuesta; la aplicación aún no lo carga.

### Regla implementada de vacantes

- Se utiliza únicamente la cohorte/carrera seleccionada.
- Se consideran módulos activos que no tienen ningún `idProfesor` en una
  asignación activa.
- Cada módulo sin asignaciones activas aporta una vacante inicial.
- Si un módulo tiene varias asignaciones activas con profesor pendiente,
  aporta una vacante por cada una de esas asignaciones.
- Se excluyen módulos inactivos y módulos que ya tienen docente en una asignación
  activa. La ampliación a módulos parcialmente cubiertos requeriría ajustar esta
  regla explícitamente.
- El número representa puestos pendientes estimados: un docente podría impartir
  varios módulos, por lo que Pamela puede corregir la cantidad en la solicitud.
- No se fija el valor `3` en código. Tres módulos vacíos producen tres vacantes.
- La acción se deshabilita mientras se edita o guarda un módulo; desaparece
  cuando no hay módulos elegibles o la cohorte/carrera es histórica/inactiva.

Ejemplo de entrada actual al formulario:

```json
{
  "cargo": "Docente para curso complexivo de la carrera de Diseño Gráfico",
  "numeroVacantes": 3,
  "origen": {
    "idCohorteCarrera": 27,
    "carrera": "Diseño Gráfico",
    "cohorte": "Convocatoria Ordinaria OCT2026",
    "idsMallaComplexivo": [101, 102, 103]
  }
}
```

Los identificadores de este ejemplo son ilustrativos. La implementación toma los
identificadores devueltos por la API, sin sembrar registros ficticios.

## 3. Diseño recomendado de la plantilla JSON

Usar [plantilla-requisicion-personal.v1.ejemplo.json](plantilla-requisicion-personal.v1.ejemplo.json)
como contrato de referencia y base para la plantilla predeterminada.

| Sección del JSON   | Responsabilidad                                                                      |
| ------------------ | ------------------------------------------------------------------------------------ |
| `schemaVersion`    | Versión de la estructura que entienden frontend y backend                            |
| `codigo`           | Identificador estable: `requisicion-personal`                                        |
| `documento`        | Título, página A4, tipografía, colores, bordes, logo, fondo y nota final             |
| `secciones`        | Identificadores y títulos de las seis secciones del formato                          |
| `campos`           | Etiquetas y ayudas, vinculadas a claves estables del formulario                      |
| `opciones`         | Catálogos de institución, contratación, jornada, remuneración, motivo y competencias |
| `valoresIniciales` | Valores literales o referencias permitidas al contexto de la solicitud               |
| `firmas`           | Títulos y etiquetas de los tres espacios para firma manual                           |

La primera versión mantiene el diseño institucional y sus campos tipados.
Los usuarios editan propiedades de este diseño mediante controles visuales.
La configuración no será HTML/CSS arbitrario ni código ejecutable. Las claves
`cargo`, `numeroVacantes`, `solicitante`, etc. son identificadores internos
estables aunque se cambie su etiqueta visible.

### Propiedades que podrá editar Pamela

- Título y subtítulo del documento.
- Títulos de sección, etiquetas y ayudas de los campos existentes.
- Nota final y títulos de los tres bloques de firma.
- Colores de texto, encabezados, destacados, fondo y bordes.
- Fuente entre las familias compatibles del PDF: `sans`, `serif`, `mono`.
- Tamaño de texto entre 8 y 12 pt, márgenes entre 8 y 25 mm y borde entre 0,2 y 1 mm.
- Logotipo institucional o una imagen cargada; fondo de página opcional,
  ajuste `contain` / `cover` y opacidad entre 0 y 1.
- Etiquetas de opciones y valores iniciales permitidos.

Mantener obligatorios los campos y reglas actualmente exigidos por el formulario.
Los tipos, límites, claves y reglas condicionales, como «Otro», permanecen en un
registro tipado de campos del módulo. Cambiar una etiqueta no cambia la validación.
Las opciones usan IDs estables; cambiar «Servicios Profesionales» como texto
visible conserva su selección interna `servicios_profesionales`.

### Valores iniciales y contexto

Permitir solo tres fuentes: `literal`, `contexto` y `texto`. Ejemplos:

```json
{
  "cargo": {
    "fuente": "texto",
    "valor": "Docente para curso complexivo de la carrera de {{curso.carrera}}"
  },
  "numeroVacantes": { "fuente": "contexto", "clave": "curso.numeroVacantes" },
  "solicitante": { "fuente": "contexto", "clave": "solicitante.nombre" },
  "area": { "fuente": "literal", "valor": "Unidad de Titulación" }
}
```

Variables admitidas inicialmente: `fechaActual`, `solicitante.nombre`,
`curso.carrera`, `curso.cohorte` y `curso.numeroVacantes`. La fecha se produce
en formato local `YYYY-MM-DD`; el número de vacantes conserva tipo numérico.
El nombre procede de la sesión actual, no del JSON ni de una cédula fija.

Al abrir desde el curso, sus datos suministran el contexto para resolver el
texto de cargo configurado y el número real de vacantes. El `cargo` precalculado
del evento actual puede mantenerse como compatibilidad durante la transición;
el formulario deberá resolver el texto desde la plantilla para que una edición
de Pamela tenga efecto. Al abrir manualmente, los valores que dependen de un
curso ausente quedan vacíos y no muestran marcadores `{{...}}` sin resolver.

Los valores iniciales se aplican una sola vez al crear el borrador. Esperar a
cargar la plantilla antes de habilitar la edición; un refresco posterior no
puede sobrescribir el cargo, las vacantes o cualquier respuesta ya editada.
Remuneración, importe total, fechas de ingreso, experiencia y perfiles conservan
sus valores manuales o los valores iniciales expresamente configurados.

## 4. Persistencia y cambio de base de datos propuesto

Recomendación: una plantilla institucional activa de requisición, compartida
entre los administradores autorizados. Guardar el JSON en MySQL y conservar
una definición base en código para inicialización/restauración.

El mapeo actual contiene `plantillas`, `plantillasparametros`,
`plantilla_contrato` y `vac_plantillas_documentos`. Se revisaron sus entidades,
columnas y relaciones:

| Tabla                       | Contrato observado en EF / diagnóstico Workbench                                                        | Consecuencia para esta función                                                                                                                                     |
| --------------------------- | ------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `plantillas`                | Archivo `VARCHAR(100)`, nombre y usuario como texto                                                     | No almacena la definición JSON completa                                                                                                                            |
| `plantillasparametros`      | Parámetros de posición X/Y, fuente, tamaño y alineación                                                 | No implementa el contrato JSON propuesto                                                                                                                           |
| `plantilla_contrato`        | Cuerpo `MEDIUMTEXT`, dedicación, tipo de contrato, institución, sello y fondo obligatorios              | Pertenece al contrato de contratación; reutilizarla exigiría relaciones ajenas al formato de requisición                                                           |
| `vac_plantillas_documentos` | Código único, cuerpo `TEXT`, nombre, estado y fecha                                                     | Puede almacenar texto genérico, pero no tiene revisión ni autoría y está vinculada al contrato del módulo VAC; `TEXT` tampoco cubre el límite de 128 KiB propuesto |
| `hdv_plantillas_documentos` | Código único, cuerpo `LONGTEXT` con JSON, nombre, estado y fecha; latin1; consumidor localizado en RRHH | Reutilizarla es técnicamente posible con un código exclusivo; la decisión final del usuario es conservar su almacenamiento independiente                           |

Una tabla propia permite guardar el JSON con revisión y autoría sin cambiar los
contratos existentes. La API serializa y valida el contenido: un campo de texto
en MySQL no valida por sí mismo la estructura de la plantilla.

### Separación respecto al sistema de Hoja de Vida

Se revisó el proyecto local
`/home/bryan/Projects/RRHH/gestion_recursos_humanos_istpet`, en particular
`HojaDeVidaWordGeneratorService.cs`, `HojaDeVidaRepository.cs`, su controller
y el mapeo EF de `HdvPlantillasDocumentos`.

- RRHH ya almacena configuraciones JSON en `cuerpo`; no era imprescindible
  crear otra tabla para almacenar JSON como tal.
- Sus lecturas filtran por `HOJA_DE_VIDA`, `HOJA_DE_VIDA_1` a `_4` o el prefijo
  `HOJA_DE_VIDA_`. El guardado normaliza `IdPlantilla` fuera de 1-4 a `1` y
  actualiza el registro histórico `HOJA_DE_VIDA` al guardar la plantilla 1.
- La conversión a `1` corresponde a ese servicio, no a una regla o trigger de
  MySQL. Compartir su endpoint con el DTO de requisición sería una integración
  incorrecta; un código propio podría permitir compartir solo la tabla.
- El usuario confirmó el esquema de HDV y la ausencia de triggers visibles,
  y autorizó finalmente una tabla nueva para mantener independientes los sistemas.

La implementación aprobada utiliza `titul_plantillas_documentos` y el repositorio,
DTOs y endpoints propios descritos en este plan. La plantilla de requisición
se identifica por `codigo = 'requisicion-personal'`; sus IDs no se interpretan
como números de plantilla HDV. El editor de Titulación y su API no llaman a
`HojaDeVidaWordGeneratorService` ni al endpoint `plantilla-config` de RRHH.
No se copian ni actualizan registros `HOJA_DE_VIDA*`, y el script `06` solo crea
la tabla nueva y consulta su estructura. La FK a usuarios registra autoría;
crear la tabla no modifica filas de usuarios.

### Verificación de MySQL y orden de aplicación

Archivos preparados siguiendo la carpeta de scripts existente del proyecto:

1. [05_verificar_plantilla_requisicion.sql](../../scripts/base-datos/05_verificar_plantilla_requisicion.sql):
   diagnóstico de solo lectura para Workbench. Devuelve versión, tablas, columnas,
   índices, compatibilidad de la FK de autoría, roles y permisos de Pamela.
2. [06_plantilla_requisicion.sql](../../scripts/base-datos/06_plantilla_requisicion.sql):
   creación de una sola tabla. No inserta el JSON ni modifica roles, adjuntos,
   solicitudes o planificación.

Resultados que deben comprobarse antes de aplicar el segundo archivo:

- El servidor es MySQL 5.7 / 8.0 y se trabaja en `sigafi_es`.
- `usuarios` usa InnoDB y `idUsuario` es `INT` con signo, PK de una columna.
  El diagnóstico debe devolver `APTO` en `compatibilidad_autoria`.
- Si `titul_plantillas_documentos` ya existe, su estructura coincide con el DDL.
  `CREATE TABLE IF NOT EXISTS` conserva una tabla existente y no la adapta.
- Los permisos efectivos de Pamela permiten `Examen Complexivo:ver` y `editar`,
  y su sesión obtiene el rol normalizado `TITULACION_ADMIN`. Revisar también
  el fallback de administrador de `RbacService` si la consulta de permisos por
  roles no devuelve filas. La cédula del diagnóstico no se usa para autorizar la API.

Si la tabla no existe y la FK es compatible, aplicar `06` en pruebas y conservar
el `SHOW CREATE TABLE` devuelto. Si hay diferencias de tipo/estructura, ajustar
el DDL y el mapeo a partir de esos resultados antes de ejecutar la creación.
Verificar después con la cuenta de conexión de la API que puede leer/escribir
la tabla; crearla con una cuenta de Workbench no prueba esos permisos.

**Estado de esta verificación:** las columnas y relaciones se han contrastado
con el código, los scripts y los resultados reales de Workbench aportados por
el usuario el **6 de octubre de 2026, 09:50 (America/Guayaquil)**. El diagnóstico
ejecutó 11 consultas y reportó cero filas actualizadas.

| Comprobación                  | Resultado confirmado                                                                                           |
| ----------------------------- | -------------------------------------------------------------------------------------------------------------- |
| Servidor                      | MySQL **5.7.21**, base `sigafi_es`                                                                             |
| Nombres de tablas             | `lower_case_table_names = 0`; utilizar los nombres exactos del DDL                                             |
| Zona horaria de la sesión     | `SYSTEM`; las fechas nuevas deben escribirse explícitamente en UTC                                             |
| Tabla de plantillas propuesta | `titul_plantillas_documentos` no aparece en el listado ni sus columnas/índices                                 |
| FK de autoría                 | `usuarios`: InnoDB, PK única `idUsuario INT(11)` con signo; diagnóstico **APTO**                               |
| Pamela                        | Usuario `53`, activo, administrador; rol `TITULACION_ADMIN` y asignación activos                               |
| Permisos del módulo           | `Examen Complexivo`: `ver`, `editar`, `crear`, `eliminar`, activos y otorgados al rol                          |
| Recursos gráficos             | `adjuntos_imagenes` ya existe; PK física `idAdjuntos_Imagenes`                                                 |
| Tabla HDV                     | `LONGTEXT` con JSON, InnoDB, latin1, PK e índice único por código; consulta de triggers vacía según el usuario |

**Decisión:** aplicar el script `06` para agregar la tabla propia. No se necesitan
cambios en roles/permisos ni en las cinco tablas complexivas para esta plantilla.
Los IDs confirmados son evidencia del diagnóstico; la aplicación sigue resolviendo
el usuario y permisos desde la sesión, sin fijar `53` en código.

El DDL de `06` coincide con el tipo y motor de `usuarios` y utiliza sintaxis
compatible con MySQL 5.7.21. Su ejecución y el primer guardado real aún no se han
verificado. La conexión directa de Codex permanece bloqueada; la confirmación del
esquema proviene de los resultados de Workbench, no de una conexión propia.

### Tabla propuesta: `titul_plantillas_documentos`

| Columna                | Tipo propuesto                   | Uso                                  |
| ---------------------- | -------------------------------- | ------------------------------------ |
| `idPlantillaDocumento` | `INT AUTO_INCREMENT PRIMARY KEY` | Identificador                        |
| `codigo`               | `VARCHAR(50) NOT NULL UNIQUE`    | `requisicion-personal`               |
| `definicionJson`       | `MEDIUMTEXT NOT NULL`            | JSON validado de la definición       |
| `revision`             | `INT NOT NULL DEFAULT 1`         | Control de concurrencia de guardado  |
| `actualizadoPor`       | `INT NULL`                       | Identificador de usuario autenticado |
| `fechaCreacion`        | `DATETIME NOT NULL`              | Fecha UTC del servidor               |
| `fechaActualizacion`   | `DATETIME NOT NULL`              | Fecha UTC del servidor               |

Usar InnoDB y `utf8mb4`. `MEDIUMTEXT` evita depender del tipo JSON nativo de una
versión concreta del servidor; la API valida y serializa un contrato tipado.
Limitar el documento a 128 KiB y excluir imágenes/base64 de esa columna.
`schemaVersion` dentro del JSON y `revision` de la fila resuelven problemas distintos.

El JWT actual emite `ClaimTypes.NameIdentifier` con `usuarios.idUsuario` y
`IUsuarioActual.UserId` lee esa claim. El DDL preparado utiliza una FK nullable
`actualizadoPor → usuarios.idUsuario`, con `ON DELETE SET NULL` para conservar
el formato si se elimina la cuenta, y `ON UPDATE NO ACTION`. El mapeo nuevo
debe conservar ese comportamiento (`DeleteBehavior.SetNull`). La compatibilidad
del tipo y motor se comprueba con el script de diagnóstico.

El `SigafiDbContext` principal es generado: agregar un mapeo parcial propio. Actualmente
`SigafiDbContext.Complexivo.cs` ya implementa `OnModelCreatingPartial`; hay que
llamar desde esa única implementación a un método de configuración de plantillas,
en lugar de crear una segunda implementación del mismo método parcial.

El DDL no usa `CHECK`, JSON nativo ni índices de texto largos. Es compatible con
MySQL 5.7.21 confirmado en Workbench. `utf8mb4_unicode_ci` se configura solo para
la tabla nueva: que `usuarios` y `adjuntos_imagenes` usen latin1 no impide una FK
entre IDs enteros. No cambiar la codificación de la base completa o de las tablas
legacy para esta función. Las fechas no tienen un default basado en la zona horaria
de la sesión: la API escribe ambas fechas en UTC.

La versión fija de Pomelo todavía es `5.7.44` en
`InfrastructureServiceCollectionExtensions.cs` y en la herramienta
`backend/tools/ComplexivoEjemplos/Program.cs`. Alinear ambas referencias a la
versión real **5.7.21** durante la fase 2, conservando la configuración explícita
para no abrir una conexión de autodetección en CI.

No hace falta insertar una plantilla por SQL: sin fila, GET devuelve la definición
base y revisión `0`; el primer PUT autorizado inserta el JSON validado con revisión
`1` y autoría real. La tabla vacía debe existir antes de probar esos endpoints.
Esta tabla y su contenido **no se han creado** durante este traspaso.

### Concurrencia

`GET` devuelve la revisión junto con la definición. `PUT` recibe
`revisionEsperada`; el repositorio actualiza solo si sigue coincidiendo y aumenta
la revisión en la misma operación/transacción. En conflicto, responder `409`
utilizando `DominioException`, ya mapeada por el middleware. Mantener el borrador
del editor y ofrecer recargar la versión actual sin sobrescribirla automáticamente.
El creador/actualizador se obtiene de las claims del JWT, no del cuerpo del cliente.

## 5. API, permisos e imágenes

Agregar un controller fino de plantillas o ampliar el módulo Complexivo,
con `[Authorize(Roles = "TITULACION_ADMIN")]` y los permisos existentes:

| Método | Ruta propuesta                                           | Permiso                    |
| ------ | -------------------------------------------------------- | -------------------------- |
| GET    | `/api/v1/complexivo/plantilla-requisicion`               | `Examen Complexivo:ver`    |
| PUT    | `/api/v1/complexivo/plantilla-requisicion`               | `Examen Complexivo:editar` |
| POST   | `/api/v1/complexivo/plantilla-requisicion/imagenes`      | `Examen Complexivo:editar` |
| GET    | `/api/v1/complexivo/plantilla-requisicion/imagenes/{id}` | `Examen Complexivo:ver`    |

El `PUT` inicial puede crear el registro con `revisionEsperada = 0`. Si aún no
existe, el `GET` devuelve la definición base, revisión `0` y `esPredeterminada = true`.
El índice único resuelve la carrera de dos guardados iniciales simultáneos.

Envelope de respuesta:

```json
{
  "codigo": "requisicion-personal",
  "revision": 2,
  "esPredeterminada": false,
  "fechaActualizacion": "2026-10-06T14:30:00Z",
  "definicion": { "schemaVersion": 1, "codigo": "requisicion-personal" }
}
```

En este ejemplo abreviado, `definicion` contiene en la respuesta real **todo** el
documento del JSON de referencia. El cuerpo de actualización será
`{ "revisionEsperada": 2, "definicion": { ... } }`.

Validaciones: versión soportada, propiedades/claves permitidas, longitudes,
IDs de opciones únicos, valores iniciales compatibles con el tipo del campo,
variables admitidas, colores hexadecimales, márgenes y tamaños dentro de rango.
Validar referencias de imágenes existentes y permitidas. Los errores conservan
el contrato `ProblemDetails` del proyecto (`400`, `401`, `403`, `409`, `413`).

### Aprovechar el manejo de adjuntos existente

Existe `AdjuntosImagenesController`, la tabla `adjuntos_imagenes` y el repositorio
`IRepositorioAdjuntosImagenes`. La ruta actual de subida es
`/api/adjuntos-imagenes/subir`. Reutilizar el registro de metadatos y las
convenciones de archivos existentes; el JSON guardará `idAdjuntosImagenes`.
El servidor resuelve la ruta del adjunto a partir del ID.

La subida genérica actual escribe archivos desde el controller y no declara
autorización ni una lista de formatos de imagen. Para esta función agregar una
operación autorizada que delegue el almacenamiento/validación a Application e
Infrastructure. Antes de habilitar la edición de imágenes, revisar también los
accesos de modificación/eliminación de adjuntos referenciados para mantener la
protección de los recursos de la plantilla sin romper las subidas estudiantiles.

- Admitir PNG/JPEG, límite de 5 MiB y comprobación de contenido/formato en servidor.
- Usar nombres de archivo generados y rutas confinadas a una carpeta administrada.
- Conservar las imágenes en almacenamiento persistente de despliegue; el directorio
  temporal y los archivos empaquetados con la aplicación no sirven para subidas.
- El logo institucional existente puede usarse sin subida adicional.
- El frontend obtiene las imágenes mediante el adaptador HTTP autenticado y
  usa blobs para vista previa/PDF; el token no se incluye en una URL.
- Validar las imágenes antes de guardar la referencia en el JSON; si la subida
  falla, mantener la imagen anterior y el borrador del editor.

Pamela utiliza su rol y permisos de la sesión. No escribir condiciones por su
nombre, cédula ni identificador numérico. La comprobación de su acceso real debe
realizarse con RBAC en el entorno de integración.

## 6. Arquitectura y archivos que debe tocar el agente

### Backend

- `Domain/Entities/TitulPlantillaDocumento.cs`: entidad nueva, no generada.
- `Application/Features/PlantillasRequisicion/`: DTOs tipados de la definición,
  validador, casos de consulta/guardado y puerto de persistencia. Reutilizar
  FluentValidation, `System.Text.Json` y las excepciones existentes.
- `Infrastructure/Persistence/SigafiDbContext.PlantillasRequisicion.cs`: `DbSet`
  y configuración propia, llamada desde el único método parcial existente.
- `Infrastructure/Persistence/Repositories/RepositorioPlantillaRequisicion.cs`:
  lectura, serialización y actualización por revisión.
- Casos/adaptadores para imágenes, reutilizando el repositorio de adjuntos.
- `WebApi/Controllers/PlantillaRequisicionController.cs`: autorización, transporte
  HTTP y conversión de multipart; sin dependencia de EF Core.
- Registrar bindings en los archivos `DependencyInjection` correspondientes.
- Revisar/aplicar los scripts `05` y `06` ya preparados en `scripts/base-datos/`.

### Frontend

- `domain/models/plantilla-requisicion.model.ts`: tipos del contrato y registro de
  claves de campos/variables. Mantener `requisicion-personal.model.ts` para la precarga.
- `domain/repositories/plantilla-requisicion.repository.ts`: puerto de lectura,
  guardado y recursos de la plantilla.
- `infrastructure/http/plantilla-requisicion-http.repository.ts`: rutas y blobs.
- `application/complexivo/plantilla-requisicion.store.ts`: signals de definición,
  revisión, carga, guardado, errores y borrador del editor.
- `app.config.ts`: binding del puerto HTTP.
- `pages/dashboard/components/requisicion-personal/`: formulario con etiquetas y
  opciones del JSON, botón de configuración y renderer/vista previa comunes.
- Un componente presentacional de edición de plantilla junto a esta funcionalidad,
  usando estilos existentes; no convertir el dashboard en un editor de documentos.
- `shared/utils/requisicion-personal-pdf.ts`: generación del documento real,
  reutilizando jsPDF/AutoTable y las convenciones de la planificación.

Mantener las capas separadas, tipos explícitos, componentes standalone `OnPush`
y los interceptores de autorización actuales. Los componentes no llaman a
HttpClient directamente. No instalar otro motor de formularios o editor visual.

## 7. Interfaz y generación del PDF

En **Requisición de personal**, mostrar **Configurar plantilla** a usuarios con
permiso `editar`. Abrir un editor con controles de **Contenido**, **Apariencia**
y **Valores iniciales** y una vista previa A4 centrada.

Flujo del editor:

1. Cargar la definición vigente y copiarla al borrador; registrar su revisión.
2. Modificar textos/colores/imágenes sin cambiar las respuestas de la solicitud.
3. Actualizar la vista previa utilizando el borrador y los datos del formulario.
4. **Guardar plantilla** valida y persiste; devuelve la revisión nueva.
5. **Cancelar** conserva la plantilla vigente y las respuestas del formulario.
6. **Restaurar formato base** carga los valores base al borrador para revisar y
   guardar; no borra automáticamente la configuración compartida.

La UI muestra en lenguaje claro que guardar cambia el formato institucional
compartido. Usa controles nativos de color, inputs, selects y componentes del
proyecto, con etiquetas, navegación de teclado y estado de guardado visible.
En pantallas pequeñas, separar edición y vista previa mediante pestañas o una
disposición vertical que conserve el centrado del diálogo.

### Una sola salida para vista previa, impresión y descarga

Crear un único generador `crearRequisicionPdf(definicion, datos, recursos)`.
La vista previa y la impresión reciben el mismo PDF, mediante un blob. Al cambiar
el borrador, regenerar la vista previa con una espera breve de 300 ms y revocar
el blob reemplazado/al cerrar. Cancelar respuestas de render anteriores para que
una imagen de carga lenta no sustituya una vista previa más reciente.

- Dibujar título, secciones, valores, líneas y casillas con elementos vectoriales.
- Las casillas se dibujan con rectángulos y trazos, evitando depender del glifo
  `☑` de las fuentes estándar de PDF.
- Dibujar fondo y logo antes del contenido de cada página; restablecer opacidad
  para que el texto y los bordes no hereden transparencia.
- Conservar las tres áreas de firma manual juntas, y ajustar el texto largo
  con saltos de línea/página, sin truncar perfiles u observaciones.
- Mantener el formato A4, los márgenes y los bordes definidos por la plantilla.
- Generar una copia de definición/datos al iniciar el render para que la salida
  corresponda a una misma revisión durante todo el proceso.
- Si el fondo/logo no puede cargarse, informar el problema y permitir revisarlo;
  no emitir silenciosamente un PDF institucional con un recurso distinto.

La requisición actual aún imprime HTML. Migrarla al PDF común como parte de esta
implementación elimina la discrepancia de fondo/estilo entre la vista previa y
la impresión y los encabezados/pies automáticos del navegador.

## 8. Fases de trabajo y entregables

### Fase 0 — Precarga desde el curso: implementada

Revisar el botón, conteo, evento tipado, traslado del dashboard, edición manual
y limpieza de contexto. Conservar la autorización y los tests añadidos.

### Fase 1 — Confirmar y fijar el contrato

Leer `AGENTS.md`, documentación y archivos actuales. Revisar las tres imágenes
originales. Convertir el JSON de ejemplo en DTOs/TypeScript tipados y una definición
base compartida en el proceso de desarrollo. Revisar campos/opciones actuales
para conservar sus validaciones y reglas de «Otro». Definir las migraciones entre
versiones de estructura antes de aceptar un nuevo `schemaVersion`.

### Fase 2 — Persistencia y API

Conservar los resultados de `05` ya aportados por el usuario. Aplicar `06` en
pruebas y verificar el `SHOW CREATE TABLE` devuelto. Alinear la versión fija de
Pomelo a MySQL 5.7.21. Implementar GET/PUT y actualización por revisión, autorizar
las rutas y registrar DI. Probar
guardado/recarga y conflicto entre dos editores. No regenerar entidades legacy.

### Fase 3 — Recursos gráficos

Implementar la operación autorizada de imágenes reutilizando adjuntos; validar
formatos/tamaño/rutas, verificar permisos y archivos referenciados, asegurar
almacenamiento persistente y añadir el adaptador frontend de blobs.

### Fase 4 — Editor y formulario configurable

Agregar editor de plantilla, controles y estados. Cargar la plantilla antes de
inicializar respuestas; conectar catálogos por IDs y actualizar `Cargar ejemplo`
para elegir por dichos IDs. Resolver valores iniciales con el contexto del curso.
Mantener editable el cargo y las vacantes precargados y conservar el nombre de
la sesión actual. Revisar conflictos/cancelación sin perder datos del formulario.

### Fase 5 — PDF y vista previa

Implementar el generador único, enlazar el diálogo centrado y las acciones de
imprimir/descargar, probar fondo/logo/colores/títulos y documentos largos de
varias páginas. Mantener los bordes visibles y la salida sin metadatos del navegador.

### Fase 6 — Validación y cierre

Ejecutar las comprobaciones automáticas y pruebas manuales siguientes. Actualizar
`docs/02-backend-api/07-examen-complexivo.md`, el índice y los ejemplos. Entregar
el SQL aplicado, la plantilla base, los resultados y las limitaciones reales.
No marcar como terminada una fase cuya API/tabla o recorrido visual no se haya probado.

## 9. Pruebas y criterios de aceptación

### Automatizadas

- Backend: permisos `401`/`403`; GET base; persistencia tipada; rechazo de versión,
  campo, variable, color o tipo inválidos; opciones duplicadas; imagen incompatible;
  `409` por revisión antigua; autoría derivada del JWT.
- Integración en pruebas: guardar/editar la requisición utiliza únicamente la
  tabla propia; comparar las plantillas `HOJA_DE_VIDA*` antes y después y comprobar
  que no cambian cuerpo, nombre, estado ni fecha. No reutilizar el endpoint HDV.
- Frontend: tres módulos vacíos → tres vacantes; módulo cubierto/inactivo excluido;
  múltiples plazas pendientes; protección durante edición/guardado; navegación
  por evento y limpieza de precarga; cargo/vacantes editables.
- Plantilla: actualización de etiquetas con claves conservadas; selección por IDs;
  valores iniciales con/sin curso; respuestas manuales conservadas tras render o
  actualización de apariencia; errores/conflictos sin perder el borrador.
- PDF: textos completos, casillas/firmas, fondos por página, bordes vectoriales,
  A4, saltos correctos y ausencia de encabezados/pies de HTML.

### Recorrido manual con Pamela

1. Entrar con la cuenta real y comprobar permiso `ver`; comprobar `editar` para
   configurar la plantilla. Ninguna comprobación se basa en su nombre/cédula.
2. Seleccionar una cohorte y Diseño Gráfico con tres módulos activos vacíos.
3. Solicitar requisición: cargo correcto y tres vacantes, solicitante actual,
   fecha actual y campos adicionales disponibles para completar.
4. Cambiar vacantes a dos y editar cargo; comprobar que la vista previa respeta
   los valores editados.
5. Configurar título/color/fondo, revisar el documento y guardar la plantilla.
6. Recargar y abrir otro borrador: formato conservado y valores del curso actual.
7. Abrir dos sesiones de edición: guardar una y comprobar el `409` en la otra.
8. Imprimir/descargar una requisición y una planificación; ambas conservan sus
   tablas y colores y la requisición utiliza el formato configurado.
9. En integración con RRHH, comprobar que obtener y generar las cuatro plantillas
   de Hoja de Vida conserva su configuración tras guardar una plantilla de requisición.

### Comandos de validación

```bash
cd backend
dotnet build -warnaserror
dotnet test
dotnet format --verify-no-changes

cd ../frontend
npm run lint
npm test -- --watch=false
npm run build
```

La generación optimizada de producción intenta descargar fuentes externas;
en un entorno sin red hay que registrar esa limitación y verificar los scripts
y estilos con el inlining de fuentes desactivado, sin cambiar la configuración
productiva del repositorio solo por esta restricción.

### Verificación realizada en este traspaso

- Las 68 pruebas actuales del frontend pasan, incluidas las nuevas pruebas de
  conteo de vacantes, navegación, autorización y edición de los campos precargados.
- `npm run lint`, Prettier sobre los archivos de esta entrega y `git diff --check`
  pasan.
- La compilación optimizada pasa con scripts y estilos optimizados y con el
  inlining de fuentes desactivado durante la comprobación por falta de red.
  La configuración productiva no se modificó. Persisten los avisos CommonJS de
  las dependencias de jsPDF ya existentes.
- El JSON de referencia se ha parseado y contrastado con los 23 campos actuales,
  las seis secciones, tres firmas, IDs de opciones, variables y valores iniciales.
- El usuario ejecutó `05` en Workbench: servidor MySQL 5.7.21, FK de autoría
  compatible, Pamela activa con el rol y las cuatro operaciones del módulo.
  El DDL `06` está revisado contra esos resultados, pero aún no aplicado.
- Se revisaron las lecturas y el guardado HDV del proyecto de RRHH. El usuario
  aportó su esquema y consulta de triggers vacía; autorizó mantener separada la
  plantilla de Titulación mediante la tabla nueva de `06`.
- Estos resultados verifican la fase 0 y los artefactos del plan; la plantilla
  editable, su persistencia y el acceso real con la sesión de Pamela todavía
  necesitan las comprobaciones de las fases pendientes.

## 10. Precauciones para el traspaso

- Hay cambios sin commit del módulo complexivo y de requisición. Conservarlos y
  revisar `git status`/diff antes de trabajar; no reemplazar todo el dashboard.
- Las skills locales `titulacion-backend`, `titulacion-frontend` y
  `titulacion-ui-design` indicadas por `AGENTS.md` no estaban presentes al revisar
  este checkout. Localizarlas si existen en Antigravity y usar la documentación
  del repositorio y el criterio Ponytail para mantener la arquitectura y simplicidad.
- El esquema y los permisos se contrastaron con el mapeo EF y los resultados de
  Workbench aportados por el usuario. La conexión propia de Codex y los sockets
  de .NET/browser estaban restringidos: verificar la aplicación de `06` y el
  recorrido real con Pamela en Antigravity.
- Los scripts `05` y `06` nuevos no cambian RBAC. No volver a ejecutar todo
  `02_seed_rbac_titulacion.sql` para esta función: ese archivo modifica múltiples
  roles/permisos, la bandera de administrador y datos de la cuenta de Pamela;
  además su `email_institucional` difiere del `emailInstitucional` mapeado actualmente.
  Si la verificación detecta permisos faltantes, resolver solo las asignaciones
  necesarias mediante el flujo RBAC del proyecto y comprobar la sesión nueva.
- El único cambio funcional nuevo de este traspaso es la precarga/navegación.
  El JSON, tabla y rutas propuestas de plantilla deben implementarse y probarse
  antes de presentarlas a Pamela como disponibles.

**Finalización:** Pamela modifica y guarda la plantilla, la recupera al recargar,
solicita desde un curso sin docentes un formulario con datos reales editables y
genera el mismo PDF que ve en la vista previa, sin perder campos ni formato.
