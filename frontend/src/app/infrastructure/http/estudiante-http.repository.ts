import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { API_BASE_URL } from '../../core/config/api.config';
import { Estudiante, NuevoEstudiante } from '../../domain/models/estudiante.model';
import { EstudianteRepository } from '../../domain/repositories/estudiante.repository';

/** Adaptador: unico punto del frontend que conoce las rutas del backend. */
@Injectable({ providedIn: 'root' })
export class EstudianteHttpRepository implements EstudianteRepository {
  private readonly http = inject(HttpClient);
  private readonly base = `${inject(API_BASE_URL)}/api/estudiantes`;

  listar(): Promise<readonly Estudiante[]> {
    return firstValueFrom(this.http.get<readonly Estudiante[]>(this.base));
  }

  async crear(nuevo: NuevoEstudiante): Promise<number> {
    const creado = await firstValueFrom(this.http.post<{ id: number }>(this.base, nuevo));
    return creado.id;
  }
}
