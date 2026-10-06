// @vitest-environment jsdom
import { TestBed } from '@angular/core/testing';
import { BrowserTestingModule, platformBrowserTesting } from '@angular/platform-browser/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import {
  CohorteCarreraComplexivo,
  GuardarMallaComplexivo,
  MallaComplexivo,
} from '../../domain/models/complexivo.model';
import {
  COMPLEXIVO_REPOSITORY,
  ComplexivoRepository,
} from '../../domain/repositories/complexivo.repository';
import { ComplexivoStore } from './complexivo.store';

try {
  TestBed.initTestEnvironment(BrowserTestingModule, platformBrowserTesting());
} catch {
  /* Inicializado por el runner. */
}

const contexto = (id: number): CohorteCarreraComplexivo => ({
  idCohorteCarrera: id,
  idCohorte: 1,
  idCarrera: id,
  cohorte: 'Cohorte',
  periodo: 'ABR2026',
  carrera: `Carrera ${id}`,
  modalidadEstudio: 'Presencial',
  esActivo: true,
});
const datos: GuardarMallaComplexivo = {
  idCohorteCarrera: 1,
  detalle: 'Módulo 1',
  detalleModulo: 'Contenidos',
  fechaInicio: '2026-10-05',
  fechaFin: '2026-10-24',
  modalidad: 'Mixta',
  totalHoras: 32,
  esActivo: true,
  temas: [],
  docentes: [],
};
const modulo: MallaComplexivo = { ...datos, idMallaComplexivo: 7, idModulosComplexivo: 8 };

describe('ComplexivoStore', () => {
  let repo: ComplexivoRepository;
  let store: ComplexivoStore;

  beforeEach(() => {
    TestBed.resetTestingModule();
    repo = {
      cohortesCarreras: vi.fn().mockResolvedValue([contexto(1)]),
      profesores: vi.fn().mockResolvedValue([]),
      asignaturas: vi.fn().mockResolvedValue([]),
      malla: vi.fn().mockResolvedValue([modulo]),
      guardar: vi.fn().mockResolvedValue(modulo),
    };
    TestBed.configureTestingModule({
      providers: [ComplexivoStore, { provide: COMPLEXIVO_REPOSITORY, useValue: repo }],
    });
    store = TestBed.inject(ComplexivoStore);
  });

  it('ignora una respuesta antigua cuando se cambia de carrera', async () => {
    let resolver: (datos: MallaComplexivo[]) => void = () => undefined;
    vi.mocked(repo.malla).mockImplementation((id) =>
      id === 1
        ? new Promise((r) => {
            resolver = r;
          })
        : Promise.resolve([]),
    );
    const primeraCarga = store.seleccionar(contexto(1));
    await store.seleccionar(contexto(2));
    resolver([modulo]);
    await primeraCarga;
    expect(store.contexto()?.idCarrera).toBe(2);
    expect(store.malla()).toEqual([]);
    expect(store.cargando()).toBe(false);
  });

  it('conserva la planificación y expone la validación cuando falla un guardado', async () => {
    await store.seleccionar(contexto(1));
    vi.mocked(repo.guardar).mockRejectedValue({
      estado: 400,
      titulo: 'Validación',
      errores: { TotalHoras: ['Las horas no coinciden.'] },
    });
    expect(await store.guardar(7, datos)).toBe(false);
    expect(store.malla()).toEqual([modulo]);
    expect(store.error()).toBe('Las horas no coinciden.');
    expect(store.guardando()).toBe(false);
  });

  it('actualiza el módulo con los identificadores guardados sin duplicarlo', async () => {
    await store.seleccionar(contexto(1));
    vi.mocked(repo.guardar).mockResolvedValue({ ...modulo, totalHoras: 24 });
    expect(await store.guardar(7, { ...datos, totalHoras: 24 })).toBe(true);
    expect(store.malla()).toHaveLength(1);
    expect(store.totalHoras()).toBe(24);
  });
});
