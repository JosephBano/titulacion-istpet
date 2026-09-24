export type FormatoReporte = 'PDF' | 'EXCEL' | 'CSV';

export type CategoriaReporte =
  | 'Convocatorias y Postulaciones'
  | 'Estudiantes y Egresados'
  | 'Validación y Docencia'
  | 'Actas y Dictámenes';

export interface ReporteCatalogoItem {
  id: string;
  titulo: string;
  descripcion: string;
  categoria: CategoriaReporte;
  icono: string;
  formatosDisponibles: FormatoReporte[];
  requierePeriodo: boolean;
  requiereCarrera: boolean;
  requiereEstado: boolean;
  badge?: string;
  disponible: boolean;
}

export interface FiltrosGeneracionReporte {
  idPeriodo?: string;
  nombrePeriodo?: string;
  idCarrera?: number;
  nombreCarrera?: string;
  idModalidad?: number;
  soloConPostulacion?: boolean | null;
  cedulaOrNombre?: string;
}

export interface MetadatosReporteInstitucional {
  institucion: string;
  sistema: string;
  tituloReporte: string;
  periodoAcademico: string;
  carreraFiltro: string;
  fechaEmision: Date;
  usuarioEmisor: string;
  rolUsuario: string;
  totalRegistros: number;
}
