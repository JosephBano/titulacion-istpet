export interface PeriodoItem {
  idPeriodo: string;
  nombre: string;
  fechaInicio?: string;
  fechaFin?: string;
  activo: boolean;
}

export interface CarreraItem {
  idCarrera: number;
  nombre: string;
  alias?: string;
  codigoCases?: string;
  directorCarrera?: string;
  activa: boolean;
}

export interface ModalidadItem {
  idModalidad: number;
  nombre: string;
}

export interface EstudiantePendienteEgreso {
  idAlumno: string;
  nombres: string;
  apellidos: string;
  nombreCompleto: string;
  email?: string;
  celular?: string;
  idCarrera: number;
  carrera: string;
  idMalla: number;
  malla: string;
  idModalidad?: number;
  modalidad?: string;
  ultimoPeriodo: string;
  nivelesAprobados: number;
  totalNivelesMalla: number;
  materiasAprobadas: number;
  totalMateriasMalla: number;
  promedioGeneral: number;
  esEgresadoTitulado: boolean;
  tienePostulacionTitulacion: boolean;
  estadoPostulacion?: string;
  noAdeuda?: boolean;
  saldoPendiente?: number;
  tieneRestricciones?: boolean;
}

export interface FiltroPendientesEgreso {
  idPeriodo?: string;
  idCarrera?: number;
  idModalidad?: number;
  cedulaOrNombre?: string;
  pagina: number;
  tamanoPagina: number;
}

export interface PagedResult<T> {
  items: T[];
  totalRegistros: number;
  pagina: number;
  tamanoPagina: number;
  totalPaginas: number;
}

export interface ExpedientePeriodoCursado {
  idPeriodo: string;
  nombrePeriodo: string;
  idNivel: number;
  nombreNivel?: string;
  orden?: number;
  paralelo?: string;
  fechaMatricula?: string;
}

export interface ExpedienteAsignatura {
  idAsignatura: number;
  nombreAsignatura: string;
  nivel?: number;
  nombreNivel?: string;
  creditos?: number;
  horas?: number;
  idPeriodo?: string;
  ef1?: number;
  ep1?: number;
  nota1?: number;
  ef2?: number;
  ep2?: number;
  nota2?: number;
  examen?: number;
  promedioFinal?: number;
  notaFinal?: number;
  aprobado: boolean;
  observacion?: string;
}

export interface ExpedientePagoRealizado {
  idPago: number;
  fechaPago?: string;
  numeroFactura?: string;
  numeroDeposito?: string;
  valorPagado: number;
  descuento?: number;
}

export interface ExpedienteRubroFinanciero {
  idCredito: number;
  idEspecie: number;
  rubro: string;
  codigoReferencia?: string;
  valorInicial: number;
  totalAbonado: number;
  saldoPendiente: number;
  estado: string;
  pagos: ExpedientePagoRealizado[];
}

export interface ExpedienteSemestreFinanciero {
  idMatricula: number;
  idPeriodo: string;
  nombrePeriodo: string;
  idNivel: number;
  nombreNivel?: string;
  fechaMatricula?: string;
  totalSemestre: number;
  totalAbonado: number;
  saldoPendiente: number;
  estadoSemestre: string;
  rubros: ExpedienteRubroFinanciero[];
}

export interface ExpedienteFinanciero {
  saldoTotalPendiente: number;
  totalCreditoCargado: number;
  totalAbonado: number;
  estaAlDia: boolean;
  semestres: ExpedienteSemestreFinanciero[];
}

export interface ExpedienteAcademico {
  idAlumno: string;
  nombres: string;
  apellidos: string;
  nombreCompleto: string;
  email?: string;
  emailInstitucional?: string;
  celular?: string;
  telefono?: string;
  direccion?: string;
  ciudadResidencia?: string;
  idCarrera: number;
  carrera: string;
  idMalla: number;
  malla: string;
  idModalidad?: number;
  modalidad?: string;
  nivelesAprobados: number;
  totalNivelesMalla: number;
  materiasAprobadas: number;
  totalMateriasMalla: number;
  promedioGeneral: number;
  esEgresadoTitulado: boolean;
  numeroActaGrado?: string;
  fechaActaGrado?: string;
  periodosCursados: ExpedientePeriodoCursado[];
  asignaturas: ExpedienteAsignatura[];
  financiero?: ExpedienteFinanciero;
}
