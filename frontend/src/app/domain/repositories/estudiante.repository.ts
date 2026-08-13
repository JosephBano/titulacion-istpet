import { InjectionToken } from '@angular/core';
import { Estudiante, NuevoEstudiante } from '../models/estudiante.model';

/**
 * Puerto de repositorio de estudiantes. La capa de aplicación depende de esta interfaz.
 */
export interface EstudianteRepository {
  listar(): Promise<readonly Estudiante[]>;
  crear(nuevo: NuevoEstudiante): Promise<number>;
}

export const ESTUDIANTE_REPOSITORY = new InjectionToken<EstudianteRepository>(
  'ESTUDIANTE_REPOSITORY',
);
