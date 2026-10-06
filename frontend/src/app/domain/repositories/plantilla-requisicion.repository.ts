import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import {
  PlantillaRequisicionEnvelope,
  ActualizarPlantillaRequisicionRequest,
  AdjuntoImagenPlantilla,
} from '../models/plantilla-requisicion.model';

export interface PlantillaRequisicionRepository {
  obtenerPlantilla(): Observable<PlantillaRequisicionEnvelope>;
  actualizarPlantilla(
    request: ActualizarPlantillaRequisicionRequest,
  ): Observable<PlantillaRequisicionEnvelope>;
  subirImagen(archivo: File): Observable<AdjuntoImagenPlantilla>;
  obtenerImagenBlob(idAdjuntosImagenes: number): Observable<Blob>;
}

export const PLANTILLA_REQUISICION_REPOSITORY =
  new InjectionToken<PlantillaRequisicionRepository>(
    'PlantillaRequisicionRepository',
  );
