import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { API_BASE_URL } from '../../core/config/api.config';
import { Estudiante, NuevoEstudiante } from '../../domain/models/estudiante.model';
import { EstudianteRepository } from '../../domain/repositories/estudiante.repository';

/** Adaptador HTTP para el repositorio de estudiantes en Titán. */
@Injectable({ providedIn: 'root' })
export class EstudianteHttpRepository implements EstudianteRepository {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  private get base(): string {
    return `${this.baseUrl}/api/estudiantes`;
  }

  listar(): Promise<readonly Estudiante[]> {
    return firstValueFrom(this.http.get<readonly Estudiante[]>(this.base));
  }

  async crear(nuevo: NuevoEstudiante): Promise<number> {
    const creado = await firstValueFrom(this.http.post<{ id: number }>(this.base, nuevo));
    return creado.id;
  }
}
