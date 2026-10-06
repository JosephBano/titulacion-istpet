import { Injectable, computed, inject, signal } from '@angular/core';
import {
  AsignaturaComplexivo,
  CohorteCarreraComplexivo,
  GuardarMallaComplexivo,
  MallaComplexivo,
  ProfesorComplexivo,
} from '../../domain/models/complexivo.model';
import { ErrorApi } from '../../domain/models/error-api.model';
import { COMPLEXIVO_REPOSITORY } from '../../domain/repositories/complexivo.repository';

@Injectable()
export class ComplexivoStore {
  private readonly repo = inject(COMPLEXIVO_REPOSITORY);
  private revision = 0;
  readonly cohortesCarreras = signal<CohorteCarreraComplexivo[]>([]);
  readonly profesores = signal<ProfesorComplexivo[]>([]);
  readonly asignaturas = signal<AsignaturaComplexivo[]>([]);
  readonly malla = signal<MallaComplexivo[]>([]);
  readonly contexto = signal<CohorteCarreraComplexivo | null>(null);
  readonly cargando = signal(false);
  readonly guardando = signal(false);
  readonly error = signal<string | null>(null);
  readonly totalHoras = computed(() =>
    this.malla()
      .filter((m) => m.esActivo)
      .reduce((total, m) => total + m.totalHoras, 0),
  );

  async iniciar(): Promise<void> {
    this.cargando.set(true);
    this.error.set(null);
    try {
      const [contextos, profesores] = await Promise.all([
        this.repo.cohortesCarreras(),
        this.repo.profesores(),
      ]);
      this.cohortesCarreras.set(contextos);
      this.profesores.set(profesores);
    } catch (error) {
      this.error.set(this.mensaje(error));
    } finally {
      this.cargando.set(false);
    }
  }

  async seleccionar(contexto: CohorteCarreraComplexivo): Promise<void> {
    const revision = ++this.revision;
    this.contexto.set(contexto);
    this.malla.set([]);
    this.asignaturas.set([]);
    this.cargando.set(true);
    this.error.set(null);
    try {
      const [malla, asignaturas] = await Promise.all([
        this.repo.malla(contexto.idCohorteCarrera),
        this.repo.asignaturas(contexto.idCarrera),
      ]);
      if (revision === this.revision) {
        this.malla.set(malla);
        this.asignaturas.set(asignaturas);
      }
    } catch (error) {
      if (revision === this.revision) this.error.set(this.mensaje(error));
    } finally {
      if (revision === this.revision) this.cargando.set(false);
    }
  }

  async guardar(id: number | null, datos: GuardarMallaComplexivo): Promise<boolean> {
    if (this.guardando()) return false;
    this.guardando.set(true);
    this.error.set(null);
    try {
      const guardado = await this.repo.guardar(id, datos);
      this.actualizarMalla(guardado);
      return true;
    } catch (error) {
      this.error.set(this.mensaje(error));
      return false;
    } finally {
      this.guardando.set(false);
    }
  }

  private actualizarMalla(guardado: MallaComplexivo): void {
    if (guardado.idCohorteCarrera !== this.contexto()?.idCohorteCarrera) return;
    this.malla.update((lista) =>
      [...lista.filter((m) => m.idMallaComplexivo !== guardado.idMallaComplexivo), guardado].sort(
        (a, b) =>
          a.fechaInicio.localeCompare(b.fechaInicio) || a.idMallaComplexivo - b.idMallaComplexivo,
      ),
    );
  }

  private mensaje(error: unknown): string {
    const problema = error as Partial<ErrorApi> | null;
    return problema?.errores
      ? Object.values(problema.errores).flat().join(' ')
      : problema?.detalle ||
          problema?.titulo ||
          'No se pudo completar la operación. Intenta nuevamente.';
  }
}
