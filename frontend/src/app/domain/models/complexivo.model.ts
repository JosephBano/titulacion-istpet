export interface CohorteCarreraComplexivo {
  idCohorteCarrera: number;
  idCohorte: number;
  idCarrera: number;
  cohorte: string;
  periodo: string;
  carrera: string;
  modalidadEstudio: string;
  esActivo: boolean;
}

export interface AsignaturaComplexivo {
  idAsignatura: number;
  nombre: string;
}

export interface ProfesorComplexivo {
  idProfesor: string;
  nombresCompletos: string;
  activo: boolean;
}

export interface TemaComplexivo {
  idTemas: number;
  idAsignatura: number;
  detalle: string;
  horasPorTema: number;
}

export interface HorarioComplexivo {
  idModulosHorarios: number;
  dias: string;
  franjaHoraria: string;
}

export interface DocenteComplexivo {
  idMallaDocenteComplexivo: number;
  idProfesor: string | null;
  fechaAsignacion: string | null;
  codigoAsignacion: string | null;
  esActivo: boolean;
  horarios: HorarioComplexivo[];
}

export interface GuardarMallaComplexivo {
  idCohorteCarrera: number;
  detalle: string;
  detalleModulo: string;
  fechaInicio: string;
  fechaFin: string;
  modalidad: string;
  totalHoras: number;
  esActivo: boolean;
  temas: TemaComplexivo[];
  docentes: DocenteComplexivo[];
}

export interface MallaComplexivo extends GuardarMallaComplexivo {
  idMallaComplexivo: number;
  idModulosComplexivo: number;
}
