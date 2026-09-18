export interface AlumnoApto {
  idAlumno: string;
  nombresCompletos: string;
  emailInstitucional: string;
  celular: string;
  idCarrera?: number;
  carrera: string;
  idModalidad?: number;
  modalidad: string;
  idPeriodo: string;
  estadoTitulacion: 'DISPONIBLE' | 'EN_PROCESO' | 'TITULADO';
}

export interface GraduadoHistorico {
  idAlumno: string;
  nombresCompletos: string;
  idTitulo: number;
  numeroActa: string;
  fechaActa?: string;
  notaFinal?: number;
  promedioEstudios?: number;
  tituloTesis: string;
}

export interface FiltroAlumnosParams {
  idCarrera?: number;
  idModalidad?: number;
  busqueda?: string;
}

export interface DetalleDeuda {
  idMatricula: number;
  periodo: string;
  especie: string;
  creditoInicial: number;
  saldo: number;
  saldoBeca: number;
}

export interface EstadoFinancieroAlumno {
  idAlumno: string;
  nombreAlumno: string;
  noAdeuda: boolean;
  saldoPendienteTotal: number;
  tieneRestricciones: boolean;
  restriccionesActivas: string[];
  deudasPendientes: DetalleDeuda[];
  mensaje: string;
}

