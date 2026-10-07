import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { API_BASE_URL } from '../../core/config/api.config';
import {
  AsignaturaComplexivo,
  CohorteCarreraComplexivo,
  GuardarMallaComplexivo,
  MallaComplexivo,
  ProfesorComplexivo,
} from '../../domain/models/complexivo.model';
import { ComplexivoRepository } from '../../domain/repositories/complexivo.repository';

@Injectable({ providedIn: 'root' })
export class ComplexivoHttpRepository implements ComplexivoRepository {
  private readonly http = inject(HttpClient);
  private readonly base = `${inject(API_BASE_URL)}/api/v1`;

  cohortesCarreras(): Promise<CohorteCarreraComplexivo[]> {
    return firstValueFrom(
      this.http.get<CohorteCarreraComplexivo[]>(`${this.base}/complexivo/cohortes-carreras`),
    );
  }

  profesores(): Promise<ProfesorComplexivo[]> {
    return firstValueFrom(this.http.get<ProfesorComplexivo[]>(`${this.base}/Actores/docentes`));
  }

  async asignaturas(idCarrera: number): Promise<AsignaturaComplexivo[]> {
    const lista = await firstValueFrom(
      this.http.get<AsignaturaComplexivo[]>(
        `${this.base}/Academico/carreras/${idCarrera}/asignaturas`,
      ),
    );
    return [...new Map(lista.map((a) => [a.idAsignatura, a])).values()];
  }

  malla(idCohorteCarrera: number): Promise<MallaComplexivo[]> {
    return firstValueFrom(
      this.http.get<MallaComplexivo[]>(`${this.base}/complexivo/malla`, {
        params: new HttpParams().set('idCohorteCarrera', idCohorteCarrera),
      }),
    );
  }

  guardar(id: number | null, datos: GuardarMallaComplexivo): Promise<MallaComplexivo> {
    return firstValueFrom(
      id === null
        ? this.http.post<MallaComplexivo>(`${this.base}/complexivo/malla`, datos)
        : this.http.put<MallaComplexivo>(`${this.base}/complexivo/malla/${id}`, datos),
    );
  }
}
