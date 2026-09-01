# 05. Configuración General, Convocatorias y Flujo Automatizado de Titulación

## 1. Visión General de la Arquitectura

El módulo de Titulación Académica ISTPET está diseñado bajo una arquitectura de dos niveles desacoplados:
1. **Configuración Maestra / Catálogos Base (`ConfiguracionGeneralController`)**: Catálogo institucional permanente de modalidades de titulación, requisitos documentales y la matriz de asociación entre ambos.
2. **Operación Periódica por Convocatoria (`ConvocatoriasController`)**: Apertura automatizada de períodos lectivos con fechas de corte estrictas y habilitación masiva de carreras.
3. **Flujo Unificado de Postulación y Dictamen (`PostulacionesController`)**: Portal consolidado para el estudiante y dictamen en un solo clic para el gestor.

---

## 2. Configuración General del Sistema (`/api/v1/configuracion`)

### 2.1. Modalidades Maestras (`titul_modalidades`)
Permite registrar y administrar las opciones de titulación reconocidas por la institución:
- **`GET /api/v1/configuracion/modalidades`**: Lista todas las modalidades registradas y el número total de requisitos asociados.
- **`POST /api/v1/configuracion/modalidades`**: Crea una nueva modalidad (Complexivo, Artículo Científico, TIC).
- **`PUT /api/v1/configuracion/modalidades/{id}`**: Modifica una modalidad existente.
- **`PATCH /api/v1/configuracion/modalidades/{id}/estado`**: Activa o desactiva la modalidad en el catálogo maestro.

#### Payload de Creación (`CrearModalidadMaestraDto`):
```json
{
  "modalidadTitulacion": "Examen Complexivo Teórico-Práctico",
  "esComplexivo": "SI",
  "esArticuloCientifico": "NO",
  "generaTesis": "NO",
  "cantidadMinima": 1
}
```

### 2.2. Requisitos Maestros (`titul_requisitos`)
Catálogo de documentos y condiciones institucionales:
- **`GET /api/v1/configuracion/requisitos`**: Lista los requisitos maestros.
- **`POST /api/v1/configuracion/requisitos`**: Crea un nuevo requisito.
- **`PUT /api/v1/configuracion/requisitos/{id}`**: Modifica los parámetros del requisito.
- **`PATCH /api/v1/configuracion/requisitos/{id}/estado`**: Cambia el estado de activación.

#### Payload de Creación (`CrearRequisitoMaestroDto`):
```json
{
  "requisito": "Certificado de Suficiencia de Inglés B1",
  "esAdjunto": true,
  "esBool": false,
  "subeAlumno": true,
  "subeColaborador": false
}
```

### 2.3. Matriz Requisito - Modalidad (`titul_requisito_modalidad`)
- **`GET /api/v1/configuracion/modalidades/{id}/requisitos`**: Muestra los requisitos aplicables a una modalidad.
- **`POST /api/v1/configuracion/modalidades/{id}/requisitos/{idReq}`**: Asocia un requisito a la modalidad.
- **`DELETE /api/v1/configuracion/modalidades/requisitos/{idRel}`**: Desasocia el requisito.

### 2.4. Resumen General del Sistema (`GET /api/v1/configuracion/resumen-general`)
Endpoint unificado de alto rendimiento para el Dashboard administrativo:
- Consolida en **1 sola llamada HTTP**:
  * Código de período y **nombre humano amigable** (ej. *Abril – Septiembre 2026*).
  * Estado operativo de corte (`CONVOCATORIA_VIGENTE`, `CONVOCATORIA_CERRADA`).
  * Total de carreras habilitadas en el período.
  * Contadores de modalidades y requisitos maestros activos.
  * Métricas en tiempo real de postulaciones (Aprobadas, En Revisión, Observadas, Rechazadas).

#### Respuesta de Ejemplo:
```json
{
  "periodoCodigo": "ABR2026",
  "periodoNombreHumano": "ABRIL - SEPTIEMBRE 2026",
  "convocatoriaDetalle": "Convocatoria Ordinaria ABR2026",
  "fechaInicioCorte": "2026-08-31T17:48:21Z",
  "fechaFinCorte": "2026-10-15T17:48:21Z",
  "diasRestantesCorte": 45,
  "estaVigenteCorte": true,
  "totalCarrerasHabilitadas": 14,
  "totalModalidadesActivas": 3,
  "totalRequisitosActivos": 5,
  "totalPostulaciones": 0,
  "totalAprobadas": 0,
  "totalEnRevision": 0,
  "totalObservadas": 0,
  "totalRechazadas": 0,
  "estadoOperativo": "CONVOCATORIA_VIGENTE"
}
```

---

## 3. Convocatorias y Fechas de Corte (`/api/v1/convocatorias`)

### 3.1. Apertura Masiva de Periodo (`POST /api/v1/convocatorias/aperturar`)
Permite al Gestor aperturar todo el período lectivo institucional en **un solo paso atómico**:
1. Desactiva la cohorte anterior.
2. Crea la nueva cohorte (`titul_cohortes`) con las fechas de corte (`FechaInicio`, `FechaFin`).
3. Consulta todas las carreras activas del instituto y crea los registros en `titul_cohortes_carreras`.
4. Asocia automáticamente las modalidades y sus plantillas de requisitos estándar preconfigurados (`titul_modalidades_titulacion_carreras`).

#### Payload de Apertura:
```json
{
  "idPeriodo": "2026-I",
  "detalleConvocatoria": "Convocatoria Ordinaria de Titulación 2026-I",
  "fechaInicioCorte": "2026-09-01T08:00:00Z",
  "fechaFinCorte": "2026-09-30T23:59:59Z",
  "diasPermitidos": 90,
  "diasExtension": 30,
  "habilitarTodasLasCarreras": true
}
```

### 3.2. Consulta de Convocatoria Activa (`GET /api/v1/convocatorias/activa`)
Retorna la cohorte vigente, el estado de la ventana de corte (`EstaVigenteCorte`), días restantes y el árbol completo de carreras y modalidades habilitadas.

### 3.3. Ajuste de Fechas de Corte (`PATCH /api/v1/convocatorias/{id}/fechas-corte`)
Permite extender el plazo o cerrar la convocatoria anticipadamente:
```json
{
  "idCohorte": 1,
  "fechaFin": "2026-10-15T23:59:59Z",
  "diasExtension": 15
}
```

---

## 4. Experiencia Unificada del Estudiante y Dictamen del Gestor

### 4.1. Portal del Alumno (`GET /api/v1/postulaciones/mi-portal`)
Endpoint consolidado que en una sola petición entrega:
1. **Estado de la Convocatoria**: Si está abierta, fecha de cierre, días restantes y mensaje institucional.
2. **Diagnóstico Académico**: Cédula, carrera, estado de aptitud y elegibilidad.
3. **Catálogo de Modalidades y Requisitos**: Opciones ofertadas para su carrera con los campos que debe adjuntar.
4. **Expediente en Vivo**: Si ya postuló, incluye el estado actual, documentos cargados y observaciones del evaluador.

### 4.2. Dictamen del Gestor (`POST /api/v1/postulaciones/{id}/dictamen`)
Permite calificar el expediente en un solo clic:
```json
{
  "idPostulacionAlumnos": 1,
  "decision": "OBSERVAR", // "APROBAR", "OBSERVAR", "RECHAZAR"
  "observaciones": "El certificado de votación adjunto está borroso. Por favor volver a subirlo escaneado.",
  "idsRequisitosObservados": [101]
}
```
