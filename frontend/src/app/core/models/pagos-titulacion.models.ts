export interface ConfiguracionPagoModalidad {
  idConfiguracionPago: number;
  idModalidadTitulacion: number;
  modalidadNombre: string;
  idPeriodo: string | null;
  valorBase: number;
  maximoInversion: number;
  totalTitulacion: number;
  idEspecie: number | null;
  especieNombre: string | null;
  activo: boolean;
  fechaRegistro: string;
  fechaModificacion: string | null;
}

export interface GuardarConfiguracionPagoModalidad {
  idModalidadTitulacion: number;
  idPeriodo?: string | null;
  valorBase: number;
  maximoInversion: number;
  totalTitulacion?: number;
  idEspecie?: number | null;
  activo?: boolean;
}

export interface PagoTitulacion {
  idPagoTitulacion: number;
  idPostulacionAlumno: number;
  estudianteCedula: string;
  estudianteNombre: string;
  carreraNombre: string;
  modalidadNombre: string;
  idMatricula: number;
  tipoRubro: 'BASE' | 'INVERSION';
  numeroFactura: string;
  valor: number;
  estado: 'REGISTRADO' | 'APROBADO' | 'RECHAZADO';
  fechaRegistro: string;
  idUsuarioRegistro: number;
  usuarioRegistroNombre: string;
  observacion: string | null;
  motivoRechazo: string | null;
  fechaRevision: string | null;
  idUsuarioRevision: number | null;
  usuarioRevisionNombre: string | null;
  comprobanteArchivo: string | null;
}

export interface RegistrarPagoTitulacion {
  idPostulacionAlumno: number;
  tipoRubro: 'BASE' | 'INVERSION';
  numeroFactura: string;
  valor: number;
  observacion?: string | null;
  comprobanteArchivo?: string | null;
}

export interface RevisionPago {
  aprobar: boolean;
  motivoRechazo?: string | null;
  observacion?: string | null;
}

export interface ResumenFinancieroEstudiante {
  idPostulacionAlumno: number;
  idMatricula: number;
  estudianteCedula: string;
  estudianteNombre: string;
  carreraNombre: string;
  modalidadNombre: string;
  idModalidadTitulacion: number;
  idPeriodo: string | null;
  valorBaseMaximo: number;
  inversionMaxima: number;
  totalTitulacion: number;
  totalBasePagado: number;
  totalInversionPagado: number;
  totalPagado: number;
  saldoBaseDisponible: number;
  saldoInversionDisponible: number;
  saldoTotalPendiente: number;
  pagos: PagoTitulacion[];
}

export interface HistorialCambioEstadoPago {
  idPagoHistorial: number;
  idPagoTitulacion: number;
  estadoAnterior: string | null;
  estadoNuevo: string;
  idUsuario: number;
  usuarioNombre: string;
  motivo: string | null;
  fechaCambio: string;
}
