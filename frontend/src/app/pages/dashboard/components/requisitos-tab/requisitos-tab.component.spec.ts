// @vitest-environment jsdom
import '@angular/compiler';
import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { BrowserTestingModule, platformBrowserTesting } from '@angular/platform-browser/testing';
import { RequisitosTabComponent } from './requisitos-tab.component';
import { RequisitoMaestro } from '../../../../core/models/titulacion.models';

try {
  TestBed.initTestEnvironment(BrowserTestingModule, platformBrowserTesting());
} catch {
  // Entorno ya inicializado
}

describe('RequisitosTabComponent Unit Tests', () => {
  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({});
  });

  it('debe inicializarse con filtros y paginación por defecto', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new RequisitosTabComponent();
      expect(comp.busqueda()).toBe('');
      expect(comp.filtroEstado()).toBe('TODOS');
      expect(comp.filtroTipo()).toBe('TODOS');
      expect(comp.paginaActual()).toBe(1);
      expect(comp.tamanoPagina()).toBe(10);
      expect(comp.skeletonRows.length).toBe(5);
    });
  });

  it('debe calcular correctamente activos e inactivos', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new RequisitosTabComponent();
      const mockReqs: RequisitoMaestro[] = [
        {
          idRequisitos: 1,
          requisito: 'Certificado de Inglés B1',
          esAdjunto: true,
          esBool: false,
          subeAlumno: true,
          subeColaborador: false,
          esActivo: true,
        },
        {
          idRequisitos: 2,
          requisito: 'Certificado de Vinculación',
          esAdjunto: false,
          esBool: true,
          subeAlumno: false,
          subeColaborador: true,
          esActivo: false,
        },
      ];

      (comp as unknown as { requisitos: () => RequisitoMaestro[] }).requisitos = () => mockReqs;

      expect(comp.totalActivos()).toBe(1);
      expect(comp.totalInactivos()).toBe(1);
      expect(comp.totalFiltrados()).toBe(2);
    });
  });

  it('debe filtrar correctamente por búsqueda de texto y tipo', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new RequisitosTabComponent();
      const mockReqs: RequisitoMaestro[] = [
        {
          idRequisitos: 1,
          requisito: 'Certificado de Inglés B1',
          esAdjunto: true,
          esBool: false,
          subeAlumno: true,
          subeColaborador: false,
          esActivo: true,
        },
        {
          idRequisitos: 2,
          requisito: 'Certificado de Vinculación',
          esAdjunto: false,
          esBool: true,
          subeAlumno: false,
          subeColaborador: true,
          esActivo: false,
        },
      ];

      (comp as unknown as { requisitos: () => RequisitoMaestro[] }).requisitos = () => mockReqs;

      comp.onSearchChange('Inglés');
      expect(comp.requisitosFiltrados().length).toBe(1);
      expect(comp.requisitosFiltrados()[0].idRequisitos).toBe(1);

      comp.onSearchChange('');
      comp.setFiltroEstado('INACTIVOS');
      expect(comp.requisitosFiltrados().length).toBe(1);
      expect(comp.requisitosFiltrados()[0].idRequisitos).toBe(2);
    });
  });
});
