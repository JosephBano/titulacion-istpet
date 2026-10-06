import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../core/config/api.config';
import { PlantillaRequisicionRepository } from '../../domain/repositories/plantilla-requisicion.repository';
import {
  PlantillaRequisicionEnvelope,
  ActualizarPlantillaRequisicionRequest,
  AdjuntoImagenPlantilla,
} from '../../domain/models/plantilla-requisicion.model';

@Injectable({
  providedIn: 'root',
})
export class PlantillaRequisicionHttpRepository
  implements PlantillaRequisicionRepository
{
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  private readonly endpoint = `${this.baseUrl}/api/v1/complexivo/plantilla-requisicion`;

  obtenerPlantilla(): Observable<PlantillaRequisicionEnvelope> {
    return this.http.get<PlantillaRequisicionEnvelope>(this.endpoint);
  }

  actualizarPlantilla(
    request: ActualizarPlantillaRequisicionRequest,
  ): Observable<PlantillaRequisicionEnvelope> {
    return this.http.put<PlantillaRequisicionEnvelope>(this.endpoint, request);
  }

  subirImagen(archivo: File): Observable<AdjuntoImagenPlantilla> {
    const formData = new FormData();
    formData.append('archivo', archivo);
    return this.http.post<AdjuntoImagenPlantilla>(
      `${this.endpoint}/imagenes`,
      formData,
    );
  }

  obtenerImagenBlob(idAdjuntosImagenes: number): Observable<Blob> {
    return this.http.get(`${this.endpoint}/imagenes/${idAdjuntosImagenes}`, {
      responseType: 'blob',
    });
  }
}
