import { describe, expect, it, vi, beforeEach } from 'vitest';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { PlantillaRequisicionStore } from './plantilla-requisicion.store';
import {
  PLANTILLA_REQUISICION_REPOSITORY,
  PlantillaRequisicionRepository,
} from '../../domain/repositories/plantilla-requisicion.repository';
import {
  PLANTILLA_REQUISICION_BASE,
  PlantillaRequisicionEnvelope,
} from '../../domain/models/plantilla-requisicion.model';

describe('PlantillaRequisicionStore', () => {
  let store: PlantillaRequisicionStore;
  let repoMock: Partial<PlantillaRequisicionRepository>;

  const mockEnvelope: PlantillaRequisicionEnvelope = {
    codigo: 'requisicion-personal',
    revision: 2,
    esPredeterminada: false,
    fechaActualizacion: '2026-10-06T14:00:00Z',
    definicion: {
      ...PLANTILLA_REQUISICION_BASE,
      documento: {
        ...PLANTILLA_REQUISICION_BASE.documento,
        titulo: 'FORMATO MODIFICADO',
      },
    },
  };

  beforeEach(() => {
    repoMock = {
      obtenerPlantilla: vi.fn().mockReturnValue(of(mockEnvelope)),
      actualizarPlantilla: vi.fn().mockReturnValue(of(mockEnvelope)),
    };

    TestBed.configureTestingModule({
      providers: [
        PlantillaRequisicionStore,
        { provide: PLANTILLA_REQUISICION_REPOSITORY, useValue: repoMock },
      ],
    });

    store = TestBed.inject(PlantillaRequisicionStore);
  });

  it('cargarPlantilla actualiza la definición vigente y la revisión', async () => {
    await store.cargarPlantilla();

    expect(store.plantillaVigente().documento.titulo).toBe('FORMATO MODIFICADO');
    expect(store.revisionVigente()).toBe(2);
    expect(store.esPredeterminada()).toBe(false);
  });

  it('iniciarEdicion crea una copia independiente en el borrador', async () => {
    await store.cargarPlantilla();
    store.iniciarEdicion();

    expect(store.borrador()).not.toBeNull();
    expect(store.borrador()?.documento.titulo).toBe('FORMATO MODIFICADO');

    // Modificar borrador no debe afectar plantillaVigente
    store.borrador()!.documento.titulo = 'TITULO EN EDICION';
    expect(store.plantillaVigente().documento.titulo).toBe('FORMATO MODIFICADO');
  });

  it('restaurarBase carga la definición predeterminada en el borrador', () => {
    store.iniciarEdicion();
    store.borrador()!.documento.titulo = 'ALGO';
    store.restaurarBase();

    expect(store.borrador()?.documento.titulo).toBe(PLANTILLA_REQUISICION_BASE.documento.titulo);
  });

  it('guardarPlantilla exitoso actualiza estado y limpia borrador', async () => {
    await store.cargarPlantilla();
    store.iniciarEdicion();

    const exito = await store.guardarPlantilla();

    expect(exito).toBe(true);
    expect(store.borrador()).toBeNull();
    expect(store.error()).toBeNull();
  });

  it('guardarPlantilla con 409 detecta conflicto y conserva el borrador', async () => {
    await store.cargarPlantilla();
    store.iniciarEdicion();

    const error409 = new HttpErrorResponse({
      status: 409,
      statusText: 'Conflict',
    });
    repoMock.actualizarPlantilla = vi.fn().mockReturnValue(throwError(() => error409));

    const exito = await store.guardarPlantilla();

    expect(exito).toBe(false);
    expect(store.conflicto()).toBe(true);
    expect(store.error()).toContain('Conflicto de concurrencia');
    expect(store.borrador()).not.toBeNull();
  });
});
