import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import { AlumnoApto, GraduadoHistorico, FiltroAlumnosParams } from '../models/alumno-filtro.model';

@Injectable({
  providedIn: 'root',
})
export class AlumnosTitulacionService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  private readonly apiUrl = `${this.apiBaseUrl}/api/v1/actores/alumnos`;

  getAlumnosAptos(params?: FiltroAlumnosParams): Observable<AlumnoApto[]> {
    let httpParams = new HttpParams();
    if (params?.idCarrera) httpParams = httpParams.set('idCarrera', params.idCarrera.toString());
    if (params?.idModalidad)
      httpParams = httpParams.set('idModalidad', params.idModalidad.toString());
    if (params?.busqueda) httpParams = httpParams.set('q', params.busqueda);

    return this.http.get<AlumnoApto[]>(`${this.apiUrl}/aptos-titulacion`, { params: httpParams });
  }

  getAlumnosGraduados(params?: FiltroAlumnosParams): Observable<GraduadoHistorico[]> {
    let httpParams = new HttpParams();
    if (params?.idCarrera) httpParams = httpParams.set('idCarrera', params.idCarrera.toString());
    if (params?.busqueda) httpParams = httpParams.set('q', params.busqueda);

    return this.http.get<GraduadoHistorico[]>(`${this.apiUrl}/graduados`, { params: httpParams });
  }
}
