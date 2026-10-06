# Ejemplos de Examen Complexivo — Diseño Gráfico

Ejemplos basados en los documentos proporcionados, con marcas `[EJEMPLO]` para
identificarlos. Los valores de remuneración son ilustrativos.

## Requisición de personal

En **Examen Complexivo → Requisición de personal**, pulsa **Cargar ejemplo**.
Esta acción reemplaza los campos del formulario actual; después puedes editarlos,
abrir el popup centrado de **Vista previa** y usar **Imprimir / Guardar PDF**.
La requisición sigue sin persistirse en la base de datos.

| Campo              | Ejemplo                                                                            |
| ------------------ | ---------------------------------------------------------------------------------- |
| Fecha de solicitud | 05/10/2026                                                                         |
| Área               | Unidad de Titulación                                                               |
| Solicitante        | Usuario autenticado                                                                |
| Cargo              | Docentes del curso de examen complexivo de Diseño Gráfico                          |
| Vacantes           | 3                                                                                  |
| Contratación       | Servicios Profesionales                                                            |
| Jornada            | Horas                                                                              |
| Remuneración       | Factura, USD 20 por hora                                                           |
| Valor total        | USD 1.920 por 96 horas                                                             |
| Lugar              | ISTPET — EVA y aulas de Diseño Gráfico                                             |
| Ingreso            | 05/10, 26/10 y 18/11/2026, según el módulo                                         |
| Motivo             | Proyecto temporal                                                                  |
| Formación          | Tercer nivel en Diseño Gráfico, Publicidad, Comunicación Visual o áreas afines     |
| Experiencia        | Mínimo dos años de experiencia profesional y docente                               |
| Competencias       | Orientación a resultados, trabajo en equipo, comunicación efectiva y adaptabilidad |

## Planificación de tres módulos

El archivo [complexivo-diseno-grafico.json](complexivo-diseno-grafico.json) contiene
los 19 temas, nueve horarios y los nombres de asignaturas que se buscan en la
malla académica real. Cada módulo es activo, mixto y tiene una asignación docente.

| Módulo                                 | Fechas               | Distribución de contenidos                                                        |  Horas |
| -------------------------------------- | -------------------- | --------------------------------------------------------------------------------- | -----: |
| 1. Comunicación y producción gráfica   | 05–24/10/2026        | Semiótica: 12; imagen corporativa: 12; packaging: 8                               |     32 |
| 2. Aplicaciones del diseño gráfico     | 26/10–17/11/2026     | Illustrator: 4; Photoshop: 4; InDesign: 4; fotografía: 4; diseño web: 16          |     32 |
| 3. Multimedia y visualización avanzada | 18/11–08/12/2026     | Animación: 4; After Effects / Multimedia II: 12; modelado 3D: 12; Multimedia I: 4 |     32 |
| **Total**                              | **05/10–08/12/2026** | **19 temas**                                                                      | **96** |

Los nombres Juan Carlos Valladares y Diego Mediavilla se buscan entre los docentes
activos. Se asignan únicamente si existe una coincidencia única; si no, la
asignación queda **pendiente**. El módulo 3 queda pendiente como en el documento
original, para probar la búsqueda y asignación desde la interfaz.

### Horarios

Cada módulo tiene 24 horas virtuales, de lunes a jueves de 19:00 a 21:00, y ocho
horas presenciales repartidas en dos sábados de tres horas y un sábado de dos horas.
Las franjas se guardan como texto, igual que en el formulario.

| Módulo | Sábados 08:00–11:00 | Sábado 08:00–10:00 | Excepciones virtuales        |
| ------ | ------------------- | ------------------ | ---------------------------- |
| 1      | 10 y 17/10/2026     | 24/10/2026         | Ninguna en los días de clase |
| 2      | 31/10 y 07/11/2026  | 14/11/2026         | 02 y 03/11/2026              |
| 3      | 21 y 28/11/2026     | 05/12/2026         | Ninguna en los días de clase |

## Cargar los módulos en MySQL

La carga automática de ejemplos se realiza desde la consola. En la pantalla de
configuración se utiliza **Nuevo módulo** para ingresar una planificación propia.
Los ejemplos que ya estén guardados se conservan y pueden editarse.

### Desde la consola

Desde la raíz del repositorio:

```bash
# Verificar contenido y horas sin conectar a MySQL:
dotnet run --project backend/tools/ComplexivoEjemplos -- --validar

# Consultar las cohortes/carreras activas de Diseño Gráfico:
dotnet run --project backend/tools/ComplexivoEjemplos -- --listar
```

Usa el `idCohorteCarrera` que muestre el listado en el siguiente comando,
reemplazando `ID_DEL_LISTADO`:

```bash
dotnet run --project backend/tools/ComplexivoEjemplos -- --cohorte-carrera ID_DEL_LISTADO
```

La herramienta usa `ConnectionStrings:SigafiDb` de la configuración local de la
API; también admite la variable `ConnectionStrings__SigafiDb`. Resuelve los IDs
desde los catálogos reales y utiliza el mismo validador y repositorio del sistema.
Si falta una asignatura o hay coincidencias ambiguas, detiene la carga; ajusta el
nombre correspondiente en el JSON al nombre de la malla académica.

La herramienta de consola guarda los tres módulos en una transacción y los vuelve a leer antes del
commit. Al repetir el comando, los módulos que conservan el mismo nombre de
ejemplo no se duplican ni se sobrescriben. No se crean carreras, cohortes,
asignaturas, profesores ni tablas.

Después, abre **Examen Complexivo → Configuración examen complexivo**, selecciona
la cohorte/carrera utilizada y recarga. Verás los módulos `[EJEMPLO]`, 96 horas
planificadas, docentes, contenidos y horarios. Puedes editar el tercer módulo
para probar la asignación de un docente y volver a guardar.
