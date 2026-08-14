// @vitest-environment jsdom
import { TestBed } from '@angular/core/testing';
import { BrowserTestingModule, platformBrowserTesting } from '@angular/platform-browser/testing';
import { beforeEach, describe, expect, it } from 'vitest';
import { ErrorApi } from '../../domain/models/error-api.model';
import { Estudiante, NuevoEstudiante } from '../../domain/models/estudiante.model';
import {
  ESTUDIANTE_REPOSITORY,
  EstudianteRepository,
} from '../../domain/repositories/estudiante.repository';
import { EstudiantesStore } from './estudiantes.store';

try {
  TestBed.initTestEnvironment(BrowserTestingModule, platformBrowserTesting());
} catch {
  // Entorno ya inicializado
}

const estudiante = (id: number, apellidos: string, cedula: string): Estudiante => ({
  id,
  cedula,
  nombres: 'Ana',
  apellidos,
  correoInstitucional: `a${id}@istpet.edu.ec`,
  estado: 'Borrador',
});

class RepositorioFalso implements EstudianteRepository {
  datos: readonly Estudiante[] = [];
  fallaCon: ErrorApi | null = null;
  creados: NuevoEstudiante[] = [];

  async listar(): Promise<readonly Estudiante[]> {
    if (this.fallaCon) throw this.fallaCon;
    return this.datos;
  }

  async crear(nuevo: NuevoEstudiante): Promise<number> {
    if (this.fallaCon) throw this.fallaCon;
    this.creados.push(nuevo);
    return this.creados.length;
  }
}

describe('EstudiantesStore', () => {
  let repositorio: RepositorioFalso;
  let store: EstudiantesStore;

  beforeEach(() => {
    TestBed.resetTestingModule();
    repositorio = new RepositorioFalso();
    TestBed.configureTestingModule({
      providers: [{ provide: ESTUDIANTE_REPOSITORY, useValue: repositorio }],
    });
    store = TestBed.inject(EstudiantesStore);
  });

  it('empieza vacio y sin cargar', () => {
    expect(store.total()).toBe(0);
    expect(store.cargando()).toBe(false);
    expect(store.vacio()).toBe(true);
  });

  it('carga la lista y apaga el indicador de carga', async () => {
    repositorio.datos = [estudiante(1, 'Perez', '1712345678')];

    await store.cargar();

    expect(store.total()).toBe(1);
    expect(store.cargando()).toBe(false);
    expect(store.vacio()).toBe(false);
  });

  it('expone el error y deja la lista intacta cuando falla la carga', async () => {
    repositorio.fallaCon = { titulo: 'Sin conexion', estado: 0 };

    await store.cargar();

    expect(store.error()?.titulo).toBe('Sin conexion');
    expect(store.total()).toBe(0);
    expect(store.cargando()).toBe(false);
  });

  it('filtra por apellido y por cedula, ignorando mayusculas', async () => {
    repositorio.datos = [
      estudiante(1, 'Perez', '1712345678'),
      estudiante(2, 'Gomez', '1798765432'),
    ];
    await store.cargar();

    store.filtrar('  gOmEz ');
    expect(store.visibles().map((e) => e.id)).toEqual([2]);

    store.filtrar('171234');
    expect(store.visibles().map((e) => e.id)).toEqual([1]);

    store.filtrar('');
    expect(store.visibles()).toHaveLength(2);
  });

  it('recarga tras crear y reporta exito', async () => {
    const nuevo: NuevoEstudiante = {
      cedula: '1712345678',
      nombres: 'Ana',
      apellidos: 'Perez',
      correoInstitucional: 'ana@istpet.edu.ec',
    };

    const ok = await store.crear(nuevo);

    expect(ok).toBe(true);
    expect(repositorio.creados).toEqual([nuevo]);
  });

  it('devuelve false y conserva los errores por campo si crear falla', async () => {
    repositorio.fallaCon = {
      titulo: 'Error de validacion.',
      estado: 400,
      errores: { Cedula: ['La cedula debe tener 10 digitos.'] },
    };

    const ok = await store.crear({
      cedula: '17',
      nombres: 'Ana',
      apellidos: 'Perez',
      correoInstitucional: 'ana@istpet.edu.ec',
    });

    expect(ok).toBe(false);
    expect(store.error()?.errores?.['Cedula']).toHaveLength(1);
  });
});
