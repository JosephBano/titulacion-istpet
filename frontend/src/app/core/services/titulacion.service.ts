import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap, catchError, of, throwError } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import { NetworkStatusService } from './network-status.service';
import {
  HealthCheckResponse,
  PortalEstudiante,
  PostularRequest,
  DictamenPostulacionRequest,
  ConvocatoriaDetalle,
  ConvocatoriaResumen,
  AperturarPeriodoRequest,
  AjustarFechasCorteRequest,
  ModalidadMaestra,
  RequisitoMaestro,
  RequisitoModalidadMatriz,
  PostulacionDetalle,
  PaginaPostulaciones,
  EstadoPostulacion,
  ResumenGeneralSistema,
  ModalidadCarreraDto,
  ResponsableRequisito,
  ProfesorCandidato,
  RequisitoEvaluacionDocente,
  EvaluarRequisitoDocenteRequest,
  EvaluacionDocenteItem,
} from '../models/titulacion.models';

@Injectable({
  providedIn: 'root',
})
export class TitulacionService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);
  private readonly network = inject(NetworkStatusService);

  private readonly API_URL = `${this.apiBaseUrl}/api/v1`;

  // ----------------------------------------------------
  // 1. Health Checks & Diagnóstico
  // ----------------------------------------------------
  public getHealthStatus(): Observable<HealthCheckResponse> {
    return this.http.get<HealthCheckResponse>(`${this.apiBaseUrl}/health`);
  }

  // ----------------------------------------------------
  // 2. Portal Unificado del Estudiante
  // ----------------------------------------------------
  public getMiPortal(): Observable<PortalEstudiante> {
    const cacheKey = 'portal_estudiante_cache';

    // Si está offline, retornar inmediatamente de caché si existe
    if (!this.network.isOnline()) {
      const cached = this.network.getCachedData<PortalEstudiante>(cacheKey);
      if (cached) return of(cached);
    }

    return this.http.get<PortalEstudiante>(`${this.API_URL}/postulaciones/mi-portal`).pipe(
      tap((data) => this.network.setCachedData(cacheKey, data, 15)),
      catchError((error) => {
        // En caso de error de red (poca conectividad), intentar resolver con caché
        const cached = this.network.getCachedData<PortalEstudiante>(cacheKey);
        if (cached) {
          return of(cached);
        }
        return throwError(() => error);
      }),
    );
  }

  public postular(request: PostularRequest): Observable<PostulacionDetalle> {
    return this.http.post<PostulacionDetalle>(`${this.API_URL}/postulaciones`, request);
  }

  public getPostulaciones(
    pagina = 1,
    tamanoPagina = 20,
    idCarrera?: number,
    idCohorte?: number,
    idEstado?: number,
    busqueda?: string,
  ): Observable<PaginaPostulaciones> {
    let params = new HttpParams()
      .set('pagina', pagina.toString())
      .set('tamanoPagina', tamanoPagina.toString());

    if (idCarrera) params = params.set('idCarrera', idCarrera.toString());
    if (idCohorte) params = params.set('idCohorte', idCohorte.toString());
    if (idEstado) params = params.set('idEstado', idEstado.toString());
    if (busqueda && busqueda.trim()) params = params.set('busqueda', busqueda.trim());

    return this.http.get<PaginaPostulaciones>(`${this.API_URL}/postulaciones`, { params });
  }

  public getTotalPostulaciones(): Observable<{ totalPostulaciones: number }> {
    return this.http.get<{ totalPostulaciones: number }>(`${this.API_URL}/postulaciones/total`);
  }

  public getEstadosPostulacion(): Observable<EstadoPostulacion[]> {
    return this.http.get<EstadoPostulacion[]>(`${this.API_URL}/postulaciones/estados`);
  }

  public getPostulacionPorId(id: number): Observable<PostulacionDetalle> {
    return this.http.get<PostulacionDetalle>(this.API_URL + '/postulaciones/' + id);
  }

  public dictaminarPostulacion(request: DictamenPostulacionRequest): Observable<void> {
    return this.http.post<void>(
      `${this.API_URL}/postulaciones/${request.idPostulacionAlumnos}/dictamen`,
      request,
    );
  }

  // ----------------------------------------------------
  // 3. Convocatorias y Fechas de Corte
  // ----------------------------------------------------
  public getConvocatoriaActiva(): Observable<ConvocatoriaDetalle> {
    const cacheKey = 'convocatoria_activa_cache';
    return this.http.get<ConvocatoriaDetalle>(`${this.API_URL}/convocatorias/activa`).pipe(
      tap((data) => this.network.setCachedData(cacheKey, data, 30)),
      catchError((error) => {
        const cached = this.network.getCachedData<ConvocatoriaDetalle>(cacheKey);
        if (cached) return of(cached);
        return throwError(() => error);
      }),
    );
  }

  public getConvocatorias(): Observable<ConvocatoriaResumen[]> {
    return this.http.get<ConvocatoriaResumen[]>(`${this.API_URL}/convocatorias`);
  }

  public aperturarPeriodo(request: AperturarPeriodoRequest): Observable<ConvocatoriaDetalle> {
    return this.http.post<ConvocatoriaDetalle>(`${this.API_URL}/convocatorias/aperturar`, request);
  }

  public ajustarFechasCorte(
    idCohorte: number,
    request: AjustarFechasCorteRequest,
  ): Observable<void> {
    return this.http.patch<void>(
      `${this.API_URL}/convocatorias/${idCohorte}/fechas-corte`,
      request,
    );
  }

  public conmutarModalidadCarrera(
    idModalidadTitulacionCarrera: number,
    activo: boolean,
  ): Observable<void> {
    const params = new HttpParams().set('activo', activo.toString());
    return this.http.patch<void>(
      `${this.API_URL}/convocatorias/modalidades-carrera/${idModalidadTitulacionCarrera}/estado`,
      null,
      { params },
    );
  }

  // ----------------------------------------------------
  // 4. Configuración General (Modalidades y Requisitos)
  // ----------------------------------------------------
  public getModalidadesMaestras(soloActivas = false): Observable<ModalidadMaestra[]> {
    const params = new HttpParams().set('soloActivas', soloActivas.toString());
    return this.http.get<ModalidadMaestra[]>(`${this.API_URL}/configuracion/modalidades`, {
      params,
    });
  }

  public crearModalidadMaestra(modalidad: Partial<ModalidadMaestra>): Observable<number> {
    return this.http.post<number>(`${this.API_URL}/configuracion/modalidades`, modalidad);
  }

  public cambiarEstadoModalidad(idModalidad: number, activo: boolean): Observable<void> {
    const params = new HttpParams().set('activo', activo.toString());
    return this.http.patch<void>(
      `${this.API_URL}/configuracion/modalidades/${idModalidad}/estado`,
      null,
      { params },
    );
  }

  public getRequisitosMaestros(soloActivos = false): Observable<RequisitoMaestro[]> {
    const params = new HttpParams().set('soloActivos', soloActivos.toString());
    return this.http.get<RequisitoMaestro[]>(`${this.API_URL}/configuracion/requisitos`, {
      params,
    });
  }

  public crearRequisitoMaestro(requisito: Partial<RequisitoMaestro>): Observable<number> {
    return this.http.post<number>(`${this.API_URL}/configuracion/requisitos`, requisito);
  }

  public cambiarEstadoRequisito(idRequisito: number, activo: boolean): Observable<void> {
    const params = new HttpParams().set('activo', activo.toString());
    return this.http.patch<void>(
      `${this.API_URL}/configuracion/requisitos/${idRequisito}/estado`,
      null,
      { params },
    );
  }

  public getRequisitosPorModalidad(idModalidad: number): Observable<RequisitoModalidadMatriz[]> {
    return this.http.get<RequisitoModalidadMatriz[]>(
      `${this.API_URL}/configuracion/modalidades/${idModalidad}/requisitos`,
    );
  }

  public asignarRequisitoAModalidad(
    idModalidad: number,
    idRequisito: number,
    esRequisitoFinal = false,
  ): Observable<{ idRequisitoModalidad: number; message: string }> {
    const params = new HttpParams().set('esRequisitoFinal', esRequisitoFinal.toString());
    return this.http.post<{ idRequisitoModalidad: number; message: string }>(
      `${this.API_URL}/configuracion/modalidades/${idModalidad}/requisitos/${idRequisito}`,
      null,
      { params },
    );
  }

  public desasignarRequisitoDeModalidad(idRequisitoModalidad: number): Observable<void> {
    return this.http.delete<void>(
      `${this.API_URL}/configuracion/modalidades/requisitos/${idRequisitoModalidad}`,
    );
  }

  public getPeriodosAcademicos(soloVigentes = true): Observable<
    {
      idPeriodo: string;
      nombre: string;
      esActivo?: boolean;
      fechaInicial?: string;
      fechaFinal?: string;
    }[]
  > {
    const params = new HttpParams().set('soloVigentes', soloVigentes.toString());
    return this.http.get<
      {
        idPeriodo: string;
        nombre: string;
        esActivo?: boolean;
        fechaInicial?: string;
        fechaFinal?: string;
      }[]
    >(`${this.API_URL}/academico/periodos`, { params });
  }

  public getResumenGeneral(): Observable<ResumenGeneralSistema> {
    return this.http.get<ResumenGeneralSistema>(`${this.API_URL}/configuracion/resumen-general`);
  }

  public getModalidadesCarreras(soloActivas = true): Observable<ModalidadCarreraDto[]> {
    const params = new HttpParams().set('soloActivas', soloActivas.toString());
    return this.http.get<ModalidadCarreraDto[]>(`${this.API_URL}/academico/modalidades-carreras`, {
      params,
    });
  }

  // ----------------------------------------------------
  // 5. Responsables y Validación de Requisitos
  // ----------------------------------------------------
  public getResponsablesPorRequisito(idRequisito: number): Observable<ResponsableRequisito[]> {
    return this.http.get<ResponsableRequisito[]>(
      `${this.API_URL}/responsables-requisitos/requisito/${idRequisito}`,
    );
  }

  public getProfesoresCandidatos(busqueda?: string): Observable<ProfesorCandidato[]> {
    let params = new HttpParams();
    if (busqueda && busqueda.trim()) {
      params = params.set('busqueda', busqueda.trim());
    }
    return this.http.get<ProfesorCandidato[]>(
      `${this.API_URL}/responsables-requisitos/profesores-candidatos`,
      { params },
    );
  }

  public asignarProfesorRequisito(
    idRequisitos: number,
    idProfesor: string,
  ): Observable<{ idResponsableEvidencias: number; message: string }> {
    return this.http.post<{ idResponsableEvidencias: number; message: string }>(
      `${this.API_URL}/responsables-requisitos/asignar`,
      { idRequisitos, idProfesor },
    );
  }

  public desasignarProfesorRequisito(idResponsableEvidencias: number): Observable<void> {
    return this.http.delete<void>(
      `${this.API_URL}/responsables-requisitos/${idResponsableEvidencias}`,
    );
  }

  public getMisPendientesDocente(): Observable<RequisitoEvaluacionDocente[]> {
    return this.http.get<RequisitoEvaluacionDocente[]>(
      `${this.API_URL}/responsables-requisitos/docente/mis-pendientes`,
    );
  }

  public evaluarRequisitoDocente(request: EvaluarRequisitoDocenteRequest): Observable<void> {
    return this.http.post<void>(`${this.API_URL}/responsables-requisitos/evaluar`, request);
  }

  public getEvaluacionesRequisitoPostulacion(
    idPostulacionAlumnoRequisitoModalidad: number,
  ): Observable<EvaluacionDocenteItem[]> {
    return this.http.get<EvaluacionDocenteItem[]>(
      `${this.API_URL}/responsables-requisitos/requisito-postulacion/${idPostulacionAlumnoRequisitoModalidad}/evaluaciones`,
    );
  }

  public subirAdjunto(
    archivo: File,
  ): Observable<{ idAdjuntosImagenes: number; nombreArchivos: string }> {
    const comando = {
      nombreArchivos: archivo.name.substring(0, 85),
      extension: archivo.name.split('.').pop() || '',
      mimeTypes: archivo.type || 'application/octet-stream',
      tamanioBytes: archivo.size,
      ruta: `/evidencias/${archivo.name}`,
    };
    return this.http.post<{ idAdjuntosImagenes: number; nombreArchivos: string }>(
      `${this.apiBaseUrl}/api/adjuntos-imagenes`,
      comando,
    );
  }
}
