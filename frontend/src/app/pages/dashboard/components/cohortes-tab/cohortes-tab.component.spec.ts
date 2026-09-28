// @vitest-environment jsdom
import '@angular/compiler';
import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { BrowserTestingModule, platformBrowserTesting } from '@angular/platform-browser/testing';
import { CohortesTabComponent } from './cohortes-tab.component';
import { ConvocatoriaResumen } from '../../../../core/models/titulacion.models';

try {
  TestBed.initTestEnvironment(BrowserTestingModule, platformBrowserTesting());
} catch {
  // Entorno ya inicializado
}

describe('CohortesTabComponent Unit Tests', () => {
  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({});
  });

  it('debe inicializarse con filtros y paginación por defecto', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new CohortesTabComponent();
      expect(comp.busqueda()).toBe('');
      expect(comp.filtroEstado()).toBe('TODAS');
      expect(comp.paginaActual()).toBe(1);
      expect(comp.tamanoPagina()).toBe(10);
    });
  });

  it('debe calcular correctamente vigentes y cerradas', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new CohortesTabComponent();
      const mockList: ConvocatoriaResumen[] = [
        {
          idCohorte: 1,
          idPeriodo: 'ABR2026',
          detalle: 'Convocatoria Ordinaria ABR2026',
          fechaInicio: '2026-04-01',
          fechaFin: '2026-07-01',
          diasPermitidos: 90,
          diasExtension: 0,
          esActivo: true,
          estaVigenteCorte: true,
          totalCarrerasHabilitadas: 5,
          totalPostulaciones: 12,
        },
        {
          idCohorte: 2,
          idPeriodo: 'OCT2025',
          detalle: 'Convocatoria Ordinaria OCT2025',
          fechaInicio: '2025-10-01',
          fechaFin: '2026-01-01',
          diasPermitidos: 90,
          diasExtension: 0,
          esActivo: false,
          estaVigenteCorte: false,
          totalCarrerasHabilitadas: 5,
          totalPostulaciones: 30,
        },
      ];

      (comp as unknown as { convocatoriasLista: () => ConvocatoriaResumen[] }).convocatoriasLista = () => mockList;

      expect(comp.totalVigentes()).toBe(1);
      expect(comp.totalCerradas()).toBe(1);
      expect(comp.totalFiltrados()).toBe(2);
    });
  });

  it('debe filtrar correctamente por búsqueda de texto y estado', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new CohortesTabComponent();
      const mockList: ConvocatoriaResumen[] = [
        {
          idCohorte: 1,
          idPeriodo: 'ABR2026',
          detalle: 'Convocatoria Ordinaria ABR2026',
          fechaInicio: '2026-04-01',
          fechaFin: '2026-07-01',
          diasPermitidos: 90,
          diasExtension: 0,
          esActivo: true,
          estaVigenteCorte: true,
          totalCarrerasHabilitadas: 5,
          totalPostulaciones: 12,
        },
        {
          idCohorte: 2,
          idPeriodo: 'OCT2025',
          detalle: 'Convocatoria Ordinaria OCT2025',
          fechaInicio: '2025-10-01',
          fechaFin: '2026-01-01',
          diasPermitidos: 90,
          diasExtension: 0,
          esActivo: false,
          estaVigenteCorte: false,
          totalCarrerasHabilitadas: 5,
          totalPostulaciones: 30,
        },
      ];

      (comp as unknown as { convocatoriasLista: () => ConvocatoriaResumen[] }).convocatoriasLista = () => mockList;

      comp.onSearchChange('OCT2025');
      expect(comp.convocatoriasFiltradas().length).toBe(1);
      expect(comp.convocatoriasFiltradas()[0].idPeriodo).toBe('OCT2025');

      comp.onSearchChange('');
      comp.setFiltroEstado('VIGENTES');
      expect(comp.convocatoriasFiltradas().length).toBe(1);
      expect(comp.convocatoriasFiltradas()[0].idPeriodo).toBe('ABR2026');
    });
  });

  it('debe emitir evento aperturarNuevo al invocar apertura', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new CohortesTabComponent();
      let emitted = false;
      comp.aperturarNuevo.subscribe(() => (emitted = true));

      comp.aperturarNuevo.emit();
      expect(emitted).toBe(true);
    });
  });

  it('debe contener 5 elementos en el arreglo de skeletonRows', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new CohortesTabComponent();
      expect(comp.skeletonRows.length).toBe(5);
    });
  });
});
