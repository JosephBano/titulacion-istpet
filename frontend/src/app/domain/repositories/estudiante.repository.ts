import { InjectionToken } from '@angular/core';
import { Estudiante, NuevoEstudiante } from '../models/estudiante.model';

/**
 * Puerto. La capa de aplicacion depende de esta interfaz, nunca de HttpClient,
 * asi que los tests inyectan un doble en vez de interceptar la red.
 */
export interface EstudianteRepository {
  listar(): Promise<readonly Estudiante[]>;
  crear(nuevo: NuevoEstudiante): Promise<number>;
}

export const ESTUDIANTE_REPOSITORY = new InjectionToken<EstudianteRepository>(
  'ESTUDIANTE_REPOSITORY',
);
