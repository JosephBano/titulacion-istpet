import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import {
  EstudiantePendienteEgreso,
  FiltroPendientesEgreso,
  PagedResult,
  ExpedienteAcademico,
  PeriodoItem,
  CarreraItem,
  ModalidadItem,
} from '../models/egresados.models';

@Injectable({
  providedIn: 'root',
})
export class EgresadosService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  private readonly API_EGRESADOS = `${this.apiBaseUrl}/api/v1/egresados`;
  private readonly API_ACADEMICO = `${this.apiBaseUrl}/api/v1/academico`;

  public getPeriodos(): Observable<PeriodoItem[]> {
    return this.http.get<PeriodoItem[]>(`${this.API_ACADEMICO}/periodos`);
  }

  public getCarreras(): Observable<CarreraItem[]> {
    return this.http.get<CarreraItem[]>(`${this.API_ACADEMICO}/carreras`);
  }

  public getModalidades(): Observable<ModalidadItem[]> {
    return this.http.get<ModalidadItem[]>(`${this.API_ACADEMICO}/modalidades`);
  }

  public getPendientesEgreso(
    filtro: FiltroPendientesEgreso,
  ): Observable<PagedResult<EstudiantePendienteEgreso>> {
    let params = new HttpParams()
      .set('pagina', filtro.pagina.toString())
      .set('tamanoPagina', filtro.tamanoPagina.toString());

    if (filtro.idPeriodo) {
      params = params.set('idPeriodo', filtro.idPeriodo);
    }
    if (filtro.idCarrera) {
      params = params.set('idCarrera', filtro.idCarrera.toString());
    }
    if (filtro.idModalidad) {
      params = params.set('idModalidad', filtro.idModalidad.toString());
    }
    if (filtro.cedulaOrNombre) {
      params = params.set('cedulaOrNombre', filtro.cedulaOrNombre);
    }

    return this.http.get<PagedResult<EstudiantePendienteEgreso>>(
      `${this.API_EGRESADOS}/pendientes`,
      { params },
    );
  }

  public getExpedienteAcademico(
    idAlumno: string,
    idCarrera?: number,
  ): Observable<ExpedienteAcademico> {
    let params = new HttpParams();
    if (idCarrera) {
      params = params.set('idCarrera', idCarrera.toString());
    }

    return this.http.get<ExpedienteAcademico>(
      `${this.API_EGRESADOS}/expediente/${encodeURIComponent(idAlumno)}`,
      { params },
    );
  }
}
