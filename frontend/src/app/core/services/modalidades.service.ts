import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export interface ModalidadDto {
  idModalidad: number;
  modalidad: string;
  modalidadImpresion?: string;
}

export interface ModalidadCarreraDto {
  idModalidadCarrera: number;
  idCarrera: number;
  carrera: string;
  idModalidad: number;
  modalidad: string;
  esActivo: boolean;
}

export interface SistemaTitulacionDto {
  codigoSistema: number;
  detalle: string;
  activo: boolean;
}

export interface EstudianteModalidadContextDto {
  idCarrera: number;
  nombreCarrera: string;
  idModalidadEstudio: number;
  nombreModalidadEstudio: string;
  modalidadesDisponiblesCarrera: ModalidadDto[];
  opcionesTitulacionDisponibles: SistemaTitulacionDto[];
}

@Injectable({
  providedIn: 'root',
})
export class ModalidadesService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  private readonly apiUrl = `${this.apiBaseUrl}/api/v1/Modalidades`;

  getModalidades(): Observable<ModalidadDto[]> {
    return this.http.get<ModalidadDto[]>(this.apiUrl);
  }

  getSistemasTitulacion(): Observable<SistemaTitulacionDto[]> {
    return this.http.get<SistemaTitulacionDto[]>(`${this.apiUrl}/sistemas-titulacion`);
  }

  getModalidadesPorCarrera(idCarrera: number): Observable<ModalidadCarreraDto[]> {
    return this.http.get<ModalidadCarreraDto[]>(`${this.apiUrl}/carreras/carrera/${idCarrera}`);
  }

  getMiContextoModalidades(): Observable<EstudianteModalidadContextDto> {
    return this.http.get<EstudianteModalidadContextDto>(`${this.apiUrl}/mi-contexto`);
  }
}
