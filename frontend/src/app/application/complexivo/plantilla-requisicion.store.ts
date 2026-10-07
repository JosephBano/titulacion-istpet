import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { PLANTILLA_REQUISICION_REPOSITORY } from '../../domain/repositories/plantilla-requisicion.repository';
import {
  PlantillaRequisicionDefinicion,
  PLANTILLA_REQUISICION_BASE,
  AdjuntoImagenPlantilla,
} from '../../domain/models/plantilla-requisicion.model';

@Injectable({
  providedIn: 'root',
})
export class PlantillaRequisicionStore {
  private readonly repository = inject(PLANTILLA_REQUISICION_REPOSITORY, {
    optional: true,
  });

  readonly plantillaVigente = signal<PlantillaRequisicionDefinicion>(
    structuredClone(PLANTILLA_REQUISICION_BASE),
  );
  readonly revisionVigente = signal<number>(0);
  readonly esPredeterminada = signal<boolean>(true);
  readonly fechaActualizacion = signal<string>('');

  readonly cargando = signal<boolean>(false);
  readonly guardando = signal<boolean>(false);
  readonly error = signal<string | null>(null);
  readonly conflicto = signal<boolean>(false);

  readonly borrador = signal<PlantillaRequisicionDefinicion | null>(null);

  async cargarPlantilla(): Promise<void> {
    if (!this.repository) {
      this.plantillaVigente.set(structuredClone(PLANTILLA_REQUISICION_BASE));
      return;
    }

    this.cargando.set(true);
    this.error.set(null);
    this.conflicto.set(false);

    try {
      const res = await firstValueFrom(this.repository.obtenerPlantilla());
      this.plantillaVigente.set(res.definicion);
      this.revisionVigente.set(res.revision);
      this.esPredeterminada.set(res.esPredeterminada);
      this.fechaActualizacion.set(res.fechaActualizacion);
    } catch (err: unknown) {
      const mensaje = err instanceof Error ? err.message : 'Error al cargar la plantilla.';
      this.error.set(mensaje);
      // Fallback seguro a la plantilla base
      this.plantillaVigente.set(structuredClone(PLANTILLA_REQUISICION_BASE));
    } finally {
      this.cargando.set(false);
    }
  }

  iniciarEdicion(): void {
    this.borrador.set(structuredClone(this.plantillaVigente()));
    this.error.set(null);
    this.conflicto.set(false);
  }

  actualizarBorrador(nuevaDefinicion: PlantillaRequisicionDefinicion): void {
    this.borrador.set(nuevaDefinicion);
  }

  restaurarBase(): void {
    this.borrador.set(structuredClone(PLANTILLA_REQUISICION_BASE));
  }

  cancelarEdicion(): void {
    this.borrador.set(null);
    this.error.set(null);
    this.conflicto.set(false);
  }

  async guardarPlantilla(): Promise<boolean> {
    const borradorActual = this.borrador();
    if (!borradorActual) return false;

    this.guardando.set(true);
    this.error.set(null);
    this.conflicto.set(false);

    if (!this.repository) {
      this.error.set('Repositorio no disponible.');
      return false;
    }

    try {
      const res = await firstValueFrom(
        this.repository.actualizarPlantilla({
          revisionEsperada: this.revisionVigente(),
          definicion: borradorActual,
        }),
      );

      this.plantillaVigente.set(res.definicion);
      this.revisionVigente.set(res.revision);
      this.esPredeterminada.set(res.esPredeterminada);
      this.fechaActualizacion.set(res.fechaActualizacion);
      this.borrador.set(null);
      return true;
    } catch (err: unknown) {
      if (err instanceof HttpErrorResponse && err.status === 409) {
        this.conflicto.set(true);
        this.error.set(
          'Conflicto de concurrencia: la plantilla fue modificada por otro usuario. Puede recargar la versión actual sin perder su borrador.',
        );
      } else {
        const mensaje =
          err instanceof Error ? err.message : 'Error al guardar la plantilla en el servidor.';
        this.error.set(mensaje);
      }
      return false;
    } finally {
      this.guardando.set(false);
    }
  }

  async subirImagen(archivo: File): Promise<AdjuntoImagenPlantilla> {
    if (!this.repository) {
      throw new Error('Repositorio no disponible.');
    }
    return await firstValueFrom(this.repository.subirImagen(archivo));
  }

  async obtenerImagenBlob(idAdjuntosImagenes: number): Promise<Blob> {
    if (!this.repository) {
      throw new Error('Repositorio no disponible.');
    }
    return await firstValueFrom(this.repository.obtenerImagenBlob(idAdjuntosImagenes));
  }
}
