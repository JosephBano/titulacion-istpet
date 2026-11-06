import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import {
  ConfiguracionPagoModalidad,
  GuardarConfiguracionPagoModalidad,
  PagoTitulacion,
  RegistrarPagoTitulacion,
  RevisionPago,
  ResumenFinancieroEstudiante,
  HistorialCambioEstadoPago,
} from '../models/pagos-titulacion.models';

@Injectable({
  providedIn: 'root',
})
export class PagosTitulacionService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);
  private readonly baseUrl = `${this.apiBaseUrl}/api/v1/pagos-titulacion`;

  getConfiguraciones(idPeriodo?: string): Observable<ConfiguracionPagoModalidad[]> {
    let params = new HttpParams();
    if (idPeriodo) {
      params = params.set('idPeriodo', idPeriodo);
    }
    return this.http.get<ConfiguracionPagoModalidad[]>(`${this.baseUrl}/configuraciones`, { params });
  }

  getConfiguracionPorModalidad(
    idModalidad: number,
    idPeriodo?: string,
  ): Observable<ConfiguracionPagoModalidad> {
    let params = new HttpParams();
    if (idPeriodo) {
      params = params.set('idPeriodo', idPeriodo);
    }
    return this.http.get<ConfiguracionPagoModalidad>(
      `${this.baseUrl}/configuraciones/modalidad/${idModalidad}`,
      { params },
    );
  }

  guardarConfiguracion(
    dto: GuardarConfiguracionPagoModalidad,
  ): Observable<ConfiguracionPagoModalidad> {
    return this.http.post<ConfiguracionPagoModalidad>(`${this.baseUrl}/configuraciones`, dto);
  }

  getResumenFinanciero(idPostulacionAlumno: number): Observable<ResumenFinancieroEstudiante> {
    return this.http.get<ResumenFinancieroEstudiante>(
      `${this.baseUrl}/postulacion/${idPostulacionAlumno}/resumen`,
    );
  }

  registrarPago(dto: RegistrarPagoTitulacion): Observable<PagoTitulacion> {
    return this.http.post<PagoTitulacion>(`${this.baseUrl}/registrar`, dto);
  }

  getBandejaContabilidad(estado?: string): Observable<PagoTitulacion[]> {
    let params = new HttpParams();
    if (estado) {
      params = params.set('estado', estado);
    }
    return this.http.get<PagoTitulacion[]>(`${this.baseUrl}/contabilidad/bandeja`, { params });
  }

  procesarRevisionContabilidad(
    idPagoTitulacion: number,
    revision: RevisionPago,
  ): Observable<PagoTitulacion> {
    return this.http.put<PagoTitulacion>(
      `${this.baseUrl}/contabilidad/${idPagoTitulacion}/revision`,
      revision,
    );
  }

  getHistorial(idPagoTitulacion: number): Observable<HistorialCambioEstadoPago[]> {
    return this.http.get<HistorialCambioEstadoPago[]>(`${this.baseUrl}/${idPagoTitulacion}/historial`);
  }
}
