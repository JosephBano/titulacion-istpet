import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CarreraDto {
  idCarrera: number;
  nombreCarrera: string;
  aliasCarrera?: string;
  codigoCases?: string;
  activa: boolean;
  idModalidad?: number;
  nombreModalidad?: string;
}

export interface EstudianteCarreraDto {
  idCarrera: number;
  nombreCarrera: string;
  aliasCarrera?: string;
  estaTitulado: boolean;
  codigoSistemaTitulacion?: string;
  tieneMatriculaVigente: boolean;
  idModalidad?: number;
  nombreModalidad?: string;
}

export interface ProfesorCarreraDto {
  idCarrera: number;
  nombreCarrera: string;
  aliasCarrera?: string;
  asignadoEnTodasLasCarreras: boolean;
  periodoAcademico?: string;
  idModalidad?: number;
  nombreModalidad?: string;
}

export interface UsuarioCarrerasResponseDto {
  idSigafi: string;
  nombreUsuario: string;
  tipoUsuario: 'ESTUDIANTE' | 'DOCENTE' | 'AMBOS' | 'ADMINISTRADOR';
  carrerasEstudiante: EstudianteCarreraDto[];
  carrerasDocente: ProfesorCarreraDto[];
}

@Injectable({
  providedIn: 'root',
})
export class CarrerasService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5032/api/v1/Carreras';

  getCarrerasTodas(): Observable<CarreraDto[]> {
    return this.http.get<CarreraDto[]>(this.apiUrl);
  }

  getCarrerasPorEstudiante(idAlumno: string): Observable<EstudianteCarreraDto[]> {
    return this.http.get<EstudianteCarreraDto[]>(`${this.apiUrl}/estudiante/${idAlumno}`);
  }

  getCarrerasPorProfesor(idProfesor: string): Observable<ProfesorCarreraDto[]> {
    return this.http.get<ProfesorCarreraDto[]>(`${this.apiUrl}/docente/${idProfesor}`);
  }

  getMisCarreras(): Observable<UsuarioCarrerasResponseDto> {
    return this.http.get<UsuarioCarrerasResponseDto>(`${this.apiUrl}/mis-carreras`);
  }
}
