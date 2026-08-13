import { TestBed } from '@angular/core/testing';
import { EstudiantesStore } from './estudiantes.store';
import {
  ESTUDIANTE_REPOSITORY,
  EstudianteRepository,
} from '../../domain/repositories/estudiante.repository';
import { Estudiante } from '../../domain/models/estudiante.model';
import { ErrorApi } from '../../domain/models/error-api.model';

describe('EstudiantesStore', () => {
  let store: EstudiantesStore;
  let repoSpy: jasmine.SpyObj<EstudianteRepository>;

  const mockEstudiantes: Estudiante[] = [
    {
      id: 1,
      cedula: '1712345678',
      nombres: 'Juan',
      apellidos: 'Pérez',
      correoInstitucional: 'juan.perez@istpet.edu.ec',
      estado: 'Borrador',
    },
    {
      id: 2,
      cedula: '0602959553',
      nombres: 'María',
      apellidos: 'Gómez',
      correoInstitucional: 'maria.gomez@istpet.edu.ec',
      estado: 'Aprobado',
    },
  ];

  beforeEach(() => {
    repoSpy = jasmine.createSpyObj<EstudianteRepository>('EstudianteRepository', [
      'listar',
      'crear',
    ]);
    repoSpy.listar.and.resolveTo(mockEstudiantes);
    repoSpy.crear.and.resolveTo(3);

    TestBed.configureTestingModule({
      providers: [EstudiantesStore, { provide: ESTUDIANTE_REPOSITORY, useValue: repoSpy }],
    });

    store = TestBed.inject(EstudiantesStore);
  });

  it('debe inicializarse con lista vacía', () => {
    expect(store.estudiantes()).toEqual([]);
    expect(store.cargando()).toBe(false);
    expect(store.error()).toBeNull();
  });

  it('debe cargar estudiantes correctamente', async () => {
    await store.cargar();
    expect(store.estudiantes()).toEqual(mockEstudiantes);
    expect(store.total()).toBe(2);
    expect(store.cargando()).toBe(false);
  });

  it('debe filtrar estudiantes por término', async () => {
    await store.cargar();
    store.filtrar('Gómez');
    expect(store.visibles().length).toBe(1);
    expect(store.visibles()[0].nombres).toBe('María');
  });

  it('debe manejar errores de carga', async () => {
    const errorSimulado: ErrorApi = { estado: 500, titulo: 'Error en servidor' };
    repoSpy.listar.and.rejectWith(errorSimulado);

    await store.cargar();
    expect(store.error()).toEqual(errorSimulado);
    expect(store.cargando()).toBe(false);
  });
});
