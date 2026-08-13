import { Injectable, computed, inject, signal } from '@angular/core';
import { ErrorApi } from '../../domain/models/error-api.model';
import { Estudiante, NuevoEstudiante, nombreCompleto } from '../../domain/models/estudiante.model';
import { ESTUDIANTE_REPOSITORY } from '../../domain/repositories/estudiante.repository';

/**
 * Store de la feature. Todo el estado es signals; los componentes solo leen
 * las señales publicas de solo lectura y llaman a los metodos de caso de uso.
 */
@Injectable({ providedIn: 'root' })
export class EstudiantesStore {
  private readonly repositorio = inject(ESTUDIANTE_REPOSITORY);

  private readonly _estudiantes = signal<readonly Estudiante[]>([]);
  private readonly _cargando = signal(false);
  private readonly _error = signal<ErrorApi | null>(null);
  private readonly _filtro = signal('');

  readonly estudiantes = this._estudiantes.asReadonly();
  readonly cargando = this._cargando.asReadonly();
  readonly error = this._error.asReadonly();
  readonly filtro = this._filtro.asReadonly();

  readonly total = computed(() => this._estudiantes().length);

  readonly visibles = computed(() => {
    const termino = this._filtro().trim().toLowerCase();
    if (!termino) return this._estudiantes();

    return this._estudiantes().filter(
      (e) => nombreCompleto(e).toLowerCase().includes(termino) || e.cedula.includes(termino),
    );
  });

  readonly vacio = computed(
    () => !this._cargando() && this._error() === null && this.total() === 0,
  );

  filtrar(termino: string): void {
    this._filtro.set(termino);
  }

  async cargar(): Promise<void> {
    this._cargando.set(true);
    this._error.set(null);
    try {
      this._estudiantes.set(await this.repositorio.listar());
    } catch (error) {
      this._error.set(error as ErrorApi);
    } finally {
      this._cargando.set(false);
    }
  }

  /** Devuelve true si se creo; el error queda expuesto en la señal `error`. */
  async crear(nuevo: NuevoEstudiante): Promise<boolean> {
    this._error.set(null);
    try {
      await this.repositorio.crear(nuevo);
      await this.cargar();
      return true;
    } catch (error) {
      this._error.set(error as ErrorApi);
      return false;
    }
  }
}
