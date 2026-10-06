import { InjectionToken } from '@angular/core';
import {
  AsignaturaComplexivo,
  CohorteCarreraComplexivo,
  GuardarMallaComplexivo,
  MallaComplexivo,
  ProfesorComplexivo,
} from '../models/complexivo.model';

export interface ComplexivoRepository {
  cohortesCarreras(): Promise<CohorteCarreraComplexivo[]>;
  profesores(): Promise<ProfesorComplexivo[]>;
  asignaturas(idCarrera: number): Promise<AsignaturaComplexivo[]>;
  malla(idCohorteCarrera: number): Promise<MallaComplexivo[]>;
  guardar(id: number | null, datos: GuardarMallaComplexivo): Promise<MallaComplexivo>;
}

export const COMPLEXIVO_REPOSITORY = new InjectionToken<ComplexivoRepository>(
  'COMPLEXIVO_REPOSITORY',
);
