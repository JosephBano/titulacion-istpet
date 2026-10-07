# Examen Complexivo: requisición y planificación del curso

El dashboard incorpora **Examen Complexivo → Requisición de personal** y
**Configuración examen complexivo**. El acceso requiere el rol
`TITULACION_ADMIN` y el permiso `Examen Complexivo:ver`. Crear o editar una
configuración requiere además la operación correspondiente del mismo módulo.
La autorización se aplica en la API y en la navegación.

## Requisición de personal

El formulario reproduce las secciones del formato institucional: información
general, institución, vacante, motivo, perfil, competencias y observaciones.
La fecha actual y el solicitante autenticado se pueden editar. Los datos del
ejemplo fotografiado no se precargan.

**Imprimir / Guardar PDF** abre la impresión del navegador con un documento A4
que contiene solo el formato y tres espacios para firma manual. Seleccionar
«Guardar como PDF» conserva el documento. No hay persistencia de requisiciones;
salir de la pantalla o recargar descarta el formulario.

La vista previa se abre en un popup con el documento A4 actualizado, separado del
formulario, y permite imprimir o guardar como PDF desde su encabezado. Los
calendarios muestran fechas en español (`dd/mm/aaaa`) y conservan el contrato
`YYYY-MM-DD` al comunicarse con la API.

### Solicitar desde el curso de preparación

En la configuración de una cohorte/carrera activa, **Solicitar requisición de
personal** aparece cuando hay módulos activos sin docentes en asignaciones
activas. Abre el formulario con **Cargo solicitado**: «Docente para curso
complexivo de la carrera de [carrera seleccionada]» y **Número de vacantes**.
Cada módulo vacío aporta una vacante; si tiene varias asignaciones activas con
docente pendiente, aporta una por cada asignación. Los módulos cubiertos o
inactivos no se cuentan. Tres módulos vacíos generan tres vacantes, sin fijar
esa cantidad en código.

El usuario puede corregir ambos campos y completar la solicitud. La fecha y
el solicitante siguen procediendo de la sesión actual. La acción se deshabilita
durante la edición/guardado de un módulo. Salir de requisición limpia el contexto
precargado; una entrada posterior desde el sidebar abre un formulario nuevo.

### Plantilla configurable de requisición de personal

La plantilla de requisición es compartida institucionalmente y configurable por
administradores autorizados (`TITULACION_ADMIN` con `Examen Complexivo:editar`).
Su definición JSON se valida y persiste exclusivamente en la tabla propia
`titul_plantillas_documentos`, manteniendo completa independencia del módulo de
Hoja de Vida (`hdv_plantillas_documentos`).

- **Control de concurrencia optimista:** la API utiliza `revisionEsperada` en el
  cuerpo del `PUT`. Si otro usuario modificó la plantilla simultáneamente, la API
  responde `409 Conflict` (vía `DominioException`), permitiendo al usuario recargar
  la versión vigente sin perder sus modificaciones locales.
- **Definición base por defecto:** si aún no existe un registro en la base de datos,
  el `GET` retorna automáticamente la definición institucional predeterminada con
  `revision = 0` y `esPredeterminada = true`. El primer `PUT` autorizado inserta la
  fila con `revision = 1` y asocia el `actualizadoPor` extraído de los claims del JWT.
- **Recursos gráficos y almacenamiento administrado:** se admiten logotipos personalizados
  e imágenes de fondo en formato PNG o JPEG de hasta 5 MiB a través del endpoint
  `/api/v1/complexivo/plantilla-requisicion/imagenes`. Los metadatos se registran en
  `adjuntos_imagenes` y los archivos se protegen en el almacenamiento administrado.
- **Generación vectorial y única de PDF:** la vista previa, la impresión y la descarga
  comparten el mismo generador `crearRequisicionPdf` basado en jsPDF, garantizando
  fidelidad A4, bordes de 0,35 mm, casillas de verificación vectoriales, marcas de agua
  con opacidad controlada y tres bloques de firma manual sin encabezados HTML del navegador.

## Configuración por cohorte y carrera

Se reutilizan las tablas `titul_modulos_complexivo`, `titul_temas`,
`titul_malla_complexivo`, `titul_malla_docente_complexivo` y
`titul_modulos_horarios`. No se ejecutan migraciones ni se modifica el esquema.
El mapeo está en una extensión parcial del DbContext para preservar el archivo
generado por EF Core Power Tools.

Cada registro de malla agrupa un módulo con temas, asignaciones docentes y
horarios. El total declarado del módulo debe coincidir con la suma de horas
positivas enteras de sus temas. Las asignaturas deben pertenecer a la malla
académica de la carrera. Los nuevos docentes deben existir y estar activos;
`idProfesor = null` representa una asignación pendiente. Las fechas del curso
pueden ser diferentes de las fechas del período académico de la cohorte.

El guardado completo usa una única transacción de EF Core. En edición se
preservan los identificadores de temas, asignaciones y horarios; los elementos
quitados se eliminan dentro de esa misma transacción. Si un módulo está
compartido con otra malla, se copia para que la edición afecte únicamente la
cohorte seleccionada. La casilla de módulo activo controla su inclusión en el
resumen de horas y en la planificación imprimible.

Los días y las franjas son texto libre, con límites de 250 y 150 caracteres.
Permiten describir fechas concretas, modalidades y excepciones, pero no
calculan automáticamente sesiones, feriados ni conflictos de horario. El
nombre físico `franja_horarioa` se conserva; la API lo presenta como
`franjaHoraria`. Las listas se ordenan por fecha e identificador, sin un campo
de orden adicional en la base.

El selector docente permite buscar por nombre o cédula y mantener una asignación
pendiente. El botón de guardado muestra las validaciones de los campos y las horas
que faltan o sobran. La acción «Usar … horas como total» ajusta explícitamente el
total declarado a la suma de los temas; no guarda una planificación con horas
inconsistentes.

Cada tarjeta muestra explícitamente el total de horas del módulo y avisos
independientes cuando no hay docentes activos asignados o no hay horarios en
asignaciones activas. **Agregar horario** permite comenzar sin seleccionar un
docente: crea o reutiliza una asignación pendiente (`idProfesor = null`). Al
editar se puede seleccionar el docente conservando los identificadores de la
asignación y sus horarios.

**Imprimir planificación** genera un PDF vectorial A4 y lo abre en otra pestaña
con la acción de impresión. El visor permite imprimirlo o descargarlo. Al
generarse directamente con jsPDF y AutoTable, no incluye el título, URL, fecha
ni paginación que el navegador agrega al imprimir una página HTML. Los bordes
de todas las celdas tienen un grosor de 0,35 mm y un color oscuro.

El PDF utiliza el formato del documento institucional:
tabla de seis columnas (módulo, tema, horas por tema, materias, docentes y total
de horas), celdas combinadas por módulo y asignatura, encabezados azules y
marcas azul, rosada y verde para distinguir los módulos. La segunda tabla
presenta las fechas y horarios por módulo con la misma paleta. Solo se imprimen
módulos y asignaciones activos; los datos pendientes se indican explícitamente.

## Endpoints

| Método | Ruta `/api/v1/complexivo`                     | Permiso  |
| ------ | --------------------------------------------- | -------- |
| GET    | `/cohortes-carreras`                          | `ver`    |
| GET    | `/malla?idCohorteCarrera=…`                   | `ver`    |
| GET    | `/malla/{id}`                                 | `ver`    |
| POST   | `/malla`                                      | `crear`  |
| PUT    | `/malla/{id}`                                 | `editar` |
| GET    | `/plantilla-requisicion`                      | `ver`    |
| PUT    | `/plantilla-requisicion`                      | `editar` |
| POST   | `/plantilla-requisicion/imagenes`             | `editar` |
| GET    | `/plantilla-requisicion/imagenes/{id}`        | `ver`    |

POST y PUT reciben `idCohorteCarrera`, `detalle` (nombre/número de módulo),
`detalleModulo`, `fechaInicio`, `fechaFin`, `modalidad`, `totalHoras`, `esActivo`,
`temas` y `docentes`. Cada docente incluye su lista `horarios`. Los nuevos
elementos de estas listas llevan identificador `0`; al editar se envían los
identificadores devueltos por GET. Los identificadores que no pertenecen al
agregado se rechazan. Las fechas se envían como `YYYY-MM-DD`.

Los catálogos se reutilizan desde `Academico/carreras/{id}/asignaturas` y
`Actores/docentes`. La API devuelve `400` con errores por campo para validaciones,
`404` para configuraciones inexistentes y `401`/`403` para accesos no autorizados.

## Prueba manual

Para rellenar una requisición y cargar módulos de ejemplo, consultar
[Ejemplos de Diseño Gráfico](../ejemplos/README.md).

1. Iniciar sesión con una cuenta administradora que tenga permisos de Examen
   Complexivo, como Pamela.
2. Completar una requisición, revisar su vista previa y guardar el PDF; comprobar
   que las casillas elegidas y los espacios de firma aparezcan correctamente.
3. Seleccionar una cohorte/carrera, crear un módulo, agregar temas cuya suma
   coincida con el total y usar **Agregar horario** sin seleccionar un docente.
4. Guardar y recargar. Editar el módulo para asignar un docente y comprobar que
   se conserven los horarios. Editar temas y horarios, volver a guardar y comprobar
   que los cambios persistan. Desactivar un módulo y verificar que quede fuera
   de las horas activas y de la planificación imprimible.
5. Imprimir la planificación y comprobar las seis columnas, celdas combinadas,
   colores por módulo, totales y avisos de docentes u horarios pendientes.
6. Con módulos activos sin docente, usar **Solicitar requisición de personal**;
   comprobar carrera, vacantes y edición manual. Volver a configuración y entrar
   a requisición desde el sidebar; comprobar que no se repita la precarga anterior.
