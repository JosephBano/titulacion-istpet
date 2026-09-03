export interface HealthCheckResponse {
  status: 'Healthy' | 'Degraded' | 'Unhealthy';
  totalDurationMs: number;
  environment: string;
  timestampUtc: string;
  version: string;
  checks: HealthCheckItem[];
}

export interface HealthCheckItem {
  name: string;
  status: string;
  durationMs: number;
  description: string | null;
  error: string | null;
}

// ----------------------------------------------------
// Modelos del Portal del Estudiante
// ----------------------------------------------------
export interface PortalEstudiante {
  convocatoria: ConvocatoriaPortal;
  estudiante: EstudiantePortal;
  postulacionActiva: PostulacionDetalle | null;
  modalidadesDisponibles: ModalidadOfertada[];
}

export interface ConvocatoriaPortal {
  estaAbierta: boolean;
  periodo: string | null;
  detalle: string | null;
  fechaInicio: string | null;
  fechaCierre: string | null;
  diasRestantes: number | null;
  mensaje: string;
}

export interface EstudiantePortal {
  idAlumno: string;
  cedula: string;
  nombreCompleto: string;
  email: string | null;
  celular: string | null;
  idCarrera: number | null;
  nombreCarrera: string | null;
  idMatricula: number | null;
  esElegible: boolean;
  mensajeElegibilidad: string;
}

export interface ModalidadOfertada {
  idModalidadTitulacionCarrera: number;
  idModalidadTitulacion: number;
  modalidadTitulacion: string;
  esComplexivo: string | null;
  esArticuloCientifico: string | null;
  generaTesis: string | null;
  requisitos: RequisitoModalidadOfertada[];
}

export interface RequisitoModalidadOfertada {
  idRequisitoModalidad: number;
  idRequisitos: number;
  nombreRequisito: string;
  esAdjunto: boolean;
  esBool: boolean;
  subeAlumno: boolean;
  subeColaborador: boolean;
  esRequisitoFinal: boolean;
}

export interface PostulacionDetalle {
  idPostulacionAlumnos: number;
  idMatricula: number;
  idAlumno: string;
  nombreAlumno: string;
  cedulaAlumno: string;
  emailAlumno: string;
  telefonoAlumno: string;
  idCarrera: number;
  nombreCarrera: string;
  idCohorte: number;
  detalleCohorte: string;
  idModalidadTitulacionCarrera: number;
  modalidadTitulacion: string;
  idPostulacionEstado: number;
  nombreEstado: string;
  esCambioModalidad: boolean | null;
  requisitos: PostulacionRequisitoDetalle[];
  observacionDictamen?: string | null;
}

export interface PostulacionRequisitoDetalle {
  idPostulacionAlumnoRequisitoModalidad: number;
  idPostulacionAlumnos: number;
  idRequisitoModalidad: number;
  idRequisitos: number;
  nombreRequisito: string;
  esAdjunto: boolean;
  esBool: boolean;
  subeAlumno: boolean;
  idAdjuntosImagenes: number | null;
  nombreArchivoAdjunto: string | null;
  rutaArchivoAdjunto: string | null;
  valorBool: boolean | null;
  estadoValidacion?: string;
  observaciones?: string | null;
  nombreEvaluador?: string | null;
  cedulaEvaluador?: string | null;
  fechaEvaluacion?: string | Date | null;
}

export interface ResponsableRequisito {
  idResponsableEvidencias: number;
  idRequisitos: number;
  nombreRequisito: string;
  idProfesor: string;
  nombreProfesor: string;
  emailProfesor: string;
  activo: boolean;
}

export interface ProfesorCandidato {
  idProfesor: string;
  nombresCompletos: string;
  email: string;
  celular: string;
  activo: boolean;
}

export interface RequisitoEvaluacionDocente {
  idPostulacionAlumnos: number;
  idPostulacionAlumnoRequisitoModalidad: number;
  idResponsableEvidencias: number;
  idRequisitos: number;
  nombreRequisito: string;
  idAlumno: string;
  nombreAlumno: string;
  cedulaAlumno: string;
  carrera: string;
  modalidad: string;
  estadoEvaluacion?: string;
  observaciones?: string | null;
  idAdjuntosImagenes?: number | null;
  nombreArchivoAdjunto?: string | null;
  rutaArchivoAdjunto?: string | null;
  aprobado: boolean;
}

export interface EvaluarRequisitoDocenteRequest {
  idPostulacionAlumnoRequisitoModalidad: number;
  idResponsableEvidencias: number;
  aprobado: boolean;
  observaciones?: string;
  idAdjuntosImagenes?: number | null;
}

export interface EvaluacionDocenteItem {
  idTitulResponsableEvidencia: number;
  idPostulacionAlumnoRequisitoModalidad: number;
  idResponsableEvidencias: number;
  estado: string;
  observaciones?: string | null;
  actualizado?: string | null;
  evaluadorNombre?: string | null;
}

export interface PostulacionResumen {
  idPostulacionAlumnos: number;
  idMatricula: number;
  idAlumno: string;
  nombreAlumno: string;
  cedulaAlumno: string;
  idCarrera: number;
  nombreCarrera: string;
  idCohorte: number;
  detalleCohorte: string;
  idModalidadTitulacionCarrera: number;
  modalidadTitulacion: string;
  idPostulacionEstado: number;
  nombreEstado: string;
  esActivo: boolean;
  esCambioModalidad: boolean | null;
  totalRequisitos: number;
  totalRequisitosCompletados: number;
}

export interface PaginaPostulaciones {
  items: PostulacionResumen[];
  pagina: number;
  tamanoPagina: number;
  total: number;
}

export interface EstadoPostulacion {
  idPostulacionEstado: number;
  nombre: string;
  orden: number;
  esFinal: boolean;
  esActivo: boolean;
}

// ----------------------------------------------------
// Modelos de Convocatorias y Gestión de Cortes
// ----------------------------------------------------
export interface ModalidadCarreraDto {
  idModalidadCarrera: number;
  idCarrera: number;
  nombreCarrera: string;
  aliasCarrera?: string;
  idModalidadEstudio: number;
  nombreModalidadEstudio: string;
  activa: boolean;
}

export interface AperturarPeriodoRequest {
  idPeriodo: string;
  detalleConvocatoria: string;
  fechaInicioCorte: string;
  fechaFinCorte: string;
  diasPermitidos?: number;
  diasExtension?: number;
  habilitarTodasLasCarreras?: boolean;
  idsModalidadesCarrerasHabilitadas?: number[];
  idsCarrerasHabilitadas?: number[];
  idsModalidadesHabilitadas?: number[];
}

export interface AjustarFechasCorteRequest {
  idCohorte: number;
  fechaInicio?: string;
  fechaFin?: string;
  diasPermitidos?: number;
  diasExtension?: number;
  esActivo?: boolean;
}

export interface ConvocatoriaResumen {
  idCohorte: number;
  idPeriodo: string;
  detalle: string;
  fechaInicio: string | null;
  fechaFin: string | null;
  diasPermitidos: number | null;
  diasExtension: number | null;
  esActivo: boolean;
  estaVigenteCorte: boolean;
  totalCarrerasHabilitadas: number;
  totalPostulaciones: number;
}

export interface ConvocatoriaDetalle {
  idCohorte: number;
  idPeriodo: string;
  detalle: string;
  fechaInicio: string | null;
  fechaFin: string | null;
  diasPermitidos: number | null;
  diasExtension: number | null;
  esActivo: boolean;
  estaVigenteCorte: boolean;
  carrerasHabilitadas: CarreraConvocatoria[];
}

export interface CarreraConvocatoria {
  idCohorteCarrera: number;
  idModalidadCarrera: number;
  idCarrera: number;
  nombreCarrera: string;
  idModalidadEstudio: number;
  nombreModalidadEstudio: string;
  esActivo: boolean;
  modalidadesTitulacion: ModalidadTitulacionHabilitada[];
}

export interface ModalidadTitulacionHabilitada {
  idModalidadTitulacionCarrera: number;
  idModalidadTitulacion: number;
  nombreModalidadTitulacion: string;
  esActivo: boolean;
  totalRequisitosConfigurados: number;
}

// ----------------------------------------------------
// Modelos de Configuración General (Maestros)
// ----------------------------------------------------
export interface ModalidadMaestra {
  idModalidadTitulacion: number;
  modalidadTitulacion: string;
  esComplexivo: string | null;
  esArticuloCientifico: string | null;
  generaTesis: string | null;
  cantidadMinima: number | null;
  esActivo: boolean;
  totalRequisitosAsociados: number;
}

export interface RequisitoMaestro {
  idRequisitos: number;
  requisito: string;
  esAdjunto: boolean;
  esBool: boolean;
  subeAlumno: boolean;
  subeColaborador: boolean;
  esActivo: boolean;
}

export interface RequisitoModalidadMatriz {
  idRequisitoModalidad: number;
  idModalidadTitulacion: number;
  modalidadTitulacion: string;
  idRequisitos: number;
  nombreRequisito: string;
  esAdjunto: boolean;
  esBool: boolean;
  subeAlumno: boolean;
  subeColaborador: boolean;
  esRequisitoFinal: boolean;
  esActivo: boolean;
}

// ----------------------------------------------------
// Modelos de Postulación y Dictamen
// ----------------------------------------------------
export interface PostularRequest {
  idMatricula: number;
  idModalidadTitulacionCarrera: number;
  requisitos?: {
    idRequisitoModalidad: number;
    idAdjuntosImagenes?: number;
    valorBool?: boolean;
  }[];
}

export interface DictamenPostulacionRequest {
  idPostulacionAlumnos: number;
  decision: 'APROBAR' | 'OBSERVAR' | 'RECHAZAR';
  observaciones?: string;
  idsRequisitosObservados?: number[];
}

export interface ResumenGeneralSistema {
  periodoCodigo: string | null;
  periodoNombreHumano: string | null;
  convocatoriaDetalle: string | null;
  fechaInicioCorte: string | null;
  fechaFinCorte: string | null;
  diasRestantesCorte: number | null;
  estaVigenteCorte: boolean;
  totalCarrerasHabilitadas: number;
  totalModalidadesActivas: number;
  totalRequisitosActivos: number;
  totalPostulaciones: number;
  totalAprobadas: number;
  totalEnRevision: number;
  totalObservadas: number;
  totalRechazadas: number;
  estadoOperativo: string;
}
