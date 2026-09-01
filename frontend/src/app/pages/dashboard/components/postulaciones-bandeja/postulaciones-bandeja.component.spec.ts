// @vitest-environment jsdom
import '@angular/compiler';
import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { BrowserTestingModule, platformBrowserTesting } from '@angular/platform-browser/testing';
import { PostulacionesBandejaComponent } from './postulaciones-bandeja.component';

try {
  TestBed.initTestEnvironment(BrowserTestingModule, platformBrowserTesting());
} catch {
  // Entorno ya inicializado
}

describe('PostulacionesBandejaComponent Unit Tests', () => {
  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({});
  });

  it('debe inicializarse con tamaño de página 10 y página 1 por defecto', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new PostulacionesBandejaComponent();
      expect(comp.tamanoPagina()).toBe(10);
      expect(comp.paginaActual()).toBe(1);
      expect(comp.filtroBusqueda()).toBe('');
      expect(comp.filtroEstado()).toBeNull();
      expect(comp.filtroCarrera()).toBeNull();
    });
  });

  it('debe calcular correctamente el total de páginas y rangos para 25 registros con tamaño 10', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new PostulacionesBandejaComponent();
      (comp as unknown as { total: () => number }).total = () => 25;
      (comp as unknown as { tamanoPagina: () => number }).tamanoPagina = () => 10;
      (comp as unknown as { paginaActual: () => number }).paginaActual = () => 2;

      expect(comp.totalPaginas()).toBe(3);
      expect(comp.rangoInicio()).toBe(11);
      expect(comp.rangoFin()).toBe(20);
    });
  });

  it('debe emitir evento de dictamen con ID y decisión al invocar emitirDictamen', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new PostulacionesBandejaComponent();
      let emittedValue: unknown = null;
      comp.dictamen.subscribe((val) => (emittedValue = val));

      comp.emitirDictamen(45, 'APROBAR');
      expect(emittedValue).toEqual({ idPostulacion: 45, decision: 'APROBAR' });
    });
  });

  it('debe mapear correctamente las clases de estado y dot institucional', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new PostulacionesBandejaComponent();
      expect(comp.getEstadoClass('Aprobada')).toBe('estado-aprobado');
      expect(comp.getEstadoClass('En Revisión')).toBe('estado-observado');
      expect(comp.getEstadoClass('Rechazada')).toBe('estado-rechazado');
      expect(comp.getEstadoClass('Registrada')).toBe('estado-registrado');

      expect(comp.getEstadoDotClass('Aprobada')).toBe('status-dot--success');
      expect(comp.getEstadoDotClass('Observada')).toBe('status-dot--warning');
      expect(comp.getEstadoDotClass('Rechazada')).toBe('status-dot--danger');
    });
  });

  it('debe emitir nuevo estado o alternar a null al invocar seleccionarEstado', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new PostulacionesBandejaComponent();
      let emittedEstado: number | null = -1;
      comp.estadoChange.subscribe((val) => (emittedEstado = val));

      comp.seleccionarEstado(3);
      expect(emittedEstado).toBe(3);

      // Simular que el estado actual ya es 3
      (comp as unknown as { filtroEstado: () => number | null }).filtroEstado = () => 3;
      comp.seleccionarEstado(3);
      expect(emittedEstado).toBeNull();
    });
  });

  it('debe restablecer los filtros al invocar limpiarTodosFiltros', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new PostulacionesBandejaComponent();
      let busquedaLimpiada = false;
      let estadoLimpiado = false;
      let carreraLimpiada = false;

      comp.busquedaChange.subscribe((val) => {
        if (val === '') busquedaLimpiada = true;
      });
      comp.estadoChange.subscribe((val) => {
        if (val === null) estadoLimpiado = true;
      });
      comp.carreraChange.subscribe((val) => {
        if (val === null) carreraLimpiada = true;
      });

      comp.limpiarTodosFiltros();
      expect(busquedaLimpiada).toBe(true);
      expect(estadoLimpiado).toBe(true);
      expect(carreraLimpiada).toBe(true);
    });
  });
});
