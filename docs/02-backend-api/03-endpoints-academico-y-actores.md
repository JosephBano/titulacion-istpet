# Especificación de Endpoints (Académico y Actores) — Sistema Titulación ISTPET

## 1. Módulo Académico (`/api/v1/academico`)

El `AcademicoController` expone los servicios de consulta del catálogo de carreras, períodos lectivos vigentes, modalidades de estudio y asignaturas registradas en el sistema SIGAFI.

### 1.1. `GET /api/v1/academico/carreras`
Obtiene el listado de carreras universitarias/tecnológicas activas en el ISTPET.

- **Respuesta (`200 OK` - Array de `CarreraResponseDto`):**
```json
[
  {
    "idCarrera": 1,
    "carrera": "Desarrollo de Software",
    "nombreCorto": "DS",
    "activo": 1
  },
  {
    "idCarrera": 2,
    "carrera": "Electrónica",
    "nombreCorto": "EL",
    "activo": 1
  }
]
```

---

### 1.2. `GET /api/v1/academico/carreras/{idCarrera}`
Obtiene el detalle de una carrera específica por su ID.

---

### 1.3. `GET /api/v1/academico/periodos`
Retorna los períodos lectivos vigentes registrados en la tabla `periodos`.

---

### 1.4. `GET /api/v1/academico/carreras/{idCarrera}/asignaturas`
Lista las asignaturas asociadas a las mallas curriculares activas de una carrera.

---

### 1.5. `GET /api/v1/academico/modalidades`
Retorna el catálogo de modalidades de estudio (Presencial, Semipresencial, En Línea).

---

## 2. Módulo de Actores del Sistema (`/api/v1/actores`)

El `ActoresController` ofrece endpoints para la búsqueda y validación de los expedientes de estudiantes, profesores evaluadores y verificación de aptitud para proceso de titulación.

### 2.1. `GET /api/v1/actores/alumnos`
Busca estudiantes registrados por término de búsqueda (`q`: número de cédula, nombres o correo institucional).

- **Parámetros:** `q` (string, opcional).
- **Respuesta (`200 OK` - Array de `AlumnoResponseDto`):**
```json
[
  {
    "idAlumno": 1042,
    "cedula": "1723456789",
    "nombres": "Juan Carlos",
    "apellidos": "Pérez Gómez",
    "emailInstitucional": "juan.perez@institutotraversari.edu.ec"
  }
]
```

---

### 2.2. `GET /api/v1/actores/alumnos/{cedula}`
Obtiene la información detallada del estudiante filtrado por su número de cédula de identidad.

---

### 2.3. `GET /api/v1/actores/docentes`
Obtiene la nómina de profesores y docentes evaluadores activos para asignaciones académicas o de tribunales.

---

### 2.4. `GET /api/v1/actores/alumnos/{cedula}/matriculas`
Lista el historial de matrículas registradas para un estudiante en las carreras del ISTPET.

---

### 2.5. `GET /api/v1/academico/alumnos/{cedula}/aptitud/{idCarrera}`
Servicio de validación de aptitud de titulación. Analiza el estado del expediente del estudiante para determinar si cumple con los créditos, materias aprobadas y estado legal para postular a titulación.

- **Respuesta (`200 OK` - `AptitudTitulacionResponseDto`):**
```json
{
  "cedula": "1723456789",
  "idCarrera": 1,
  "esApto": true,
  "creditosAprobados": 120,
  "creditosRequeridos": 120,
  "observaciones": "Estudiante apto para proceso de titulación."
}
```


