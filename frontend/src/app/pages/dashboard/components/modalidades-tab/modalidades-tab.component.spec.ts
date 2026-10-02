// @vitest-environment jsdom
import '@angular/compiler';
import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { BrowserTestingModule, platformBrowserTesting } from '@angular/platform-browser/testing';
import { ModalidadesTabComponent } from './modalidades-tab.component';
import { ModalidadMaestra } from '../../../../core/models/titulacion.models';

try {
  TestBed.initTestEnvironment(BrowserTestingModule, platformBrowserTesting());
} catch {
  // Entorno ya inicializado
}

describe('ModalidadesTabComponent Unit Tests', () => {
  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({});
  });

  it('debe inicializarse con filtros y paginación por defecto', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new ModalidadesTabComponent();
      expect(comp.busqueda()).toBe('');
      expect(comp.filtroEstado()).toBe('TODAS');
      expect(comp.paginaActual()).toBe(1);
      expect(comp.tamanoPagina()).toBe(10);
      expect(comp.skeletonRows.length).toBe(5);
    });
  });

  it('debe calcular correctamente activas e inactivas', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new ModalidadesTabComponent();
      const mockModalidades: ModalidadMaestra[] = [
        {
          idModalidadTitulacion: 1,
          modalidadTitulacion: 'Examen Complexivo',
          cantidadMinima: 1,
          esComplexivo: 'SI',
          esArticuloCientifico: 'NO',
          generaTesis: 'NO',
          totalRequisitosAsociados: 3,
          esActivo: true,
        },
        {
          idModalidadTitulacion: 2,
          modalidadTitulacion: 'Artículo Científico',
          cantidadMinima: 1,
          esComplexivo: 'NO',
          esArticuloCientifico: 'SI',
          generaTesis: 'NO',
          totalRequisitosAsociados: 4,
          esActivo: false,
        },
      ];

      (comp as unknown as { modalidades: () => ModalidadMaestra[] }).modalidades = () =>
        mockModalidades;

      expect(comp.totalActivas()).toBe(1);
      expect(comp.totalInactivas()).toBe(1);
      expect(comp.totalFiltrados()).toBe(2);
    });
  });

  it('debe filtrar correctamente por búsqueda de texto y estado', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new ModalidadesTabComponent();
      const mockModalidades: ModalidadMaestra[] = [
        {
          idModalidadTitulacion: 1,
          modalidadTitulacion: 'Examen Complexivo',
          cantidadMinima: 1,
          esComplexivo: 'SI',
          esArticuloCientifico: 'NO',
          generaTesis: 'NO',
          totalRequisitosAsociados: 3,
          esActivo: true,
        },
        {
          idModalidadTitulacion: 2,
          modalidadTitulacion: 'Artículo Científico',
          cantidadMinima: 1,
          esComplexivo: 'NO',
          esArticuloCientifico: 'SI',
          generaTesis: 'NO',
          totalRequisitosAsociados: 4,
          esActivo: false,
        },
      ];

      (comp as unknown as { modalidades: () => ModalidadMaestra[] }).modalidades = () =>
        mockModalidades;

      comp.onSearchChange('Complexivo');
      expect(comp.modalidadesFiltradas().length).toBe(1);
      expect(comp.modalidadesFiltradas()[0].idModalidadTitulacion).toBe(1);

      comp.onSearchChange('');
      comp.setFiltroEstado('INACTIVAS');
      expect(comp.modalidadesFiltradas().length).toBe(1);
      expect(comp.modalidadesFiltradas()[0].idModalidadTitulacion).toBe(2);
    });
  });
});
