/**
 * Modelo de dominio para Estudiantes en Titán.
 * Deliberadamente libre de dependencias con Angular o HTTP.
 */
export type EstadoTitulacion = 'Borrador' | 'EnRevision' | 'Aprobado' | 'Rechazado' | 'Titulado';

export interface Estudiante {
  readonly id: number;
  readonly cedula: string;
  readonly nombres: string;
  readonly apellidos: string;
  readonly correoInstitucional: string;
  readonly estado: EstadoTitulacion;
}

export interface NuevoEstudiante {
  readonly cedula: string;
  readonly nombres: string;
  readonly apellidos: string;
  readonly correoInstitucional: string;
}

export const nombreCompleto = (e: Estudiante): string => `${e.apellidos} ${e.nombres}`.trim();
