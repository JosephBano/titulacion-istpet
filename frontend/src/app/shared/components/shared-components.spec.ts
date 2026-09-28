// @vitest-environment jsdom
import '@angular/compiler';
import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { BrowserTestingModule, platformBrowserTesting } from '@angular/platform-browser/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NetworkBannerComponent } from './network-banner/network-banner.component';
import { ConvocatoriaCardComponent } from './convocatoria-card/convocatoria-card.component';
import { StepperComponent } from './stepper/stepper.component';
import { KpiCardComponent } from './kpi-card/kpi-card.component';
import { DictamenModalComponent } from './dictamen-modal/dictamen-modal.component';
import { AperturaPeriodoModalComponent } from './apertura-periodo-modal/apertura-periodo-modal.component';
import { DrawerComponent } from './drawer/drawer.component';
import { TitulacionService } from '../../core/services/titulacion.service';
import { API_BASE_URL } from '../../core/config/api.config';

try {
  TestBed.initTestEnvironment(BrowserTestingModule, platformBrowserTesting());
} catch {
  // Entorno ya inicializado
}

describe('Shared UI Components Unit Tests', () => {
  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        TitulacionService,
        { provide: API_BASE_URL, useValue: 'http://localhost:5000' },
      ],
    });
  });

  it('NetworkBannerComponent: debe instanciarse con valores de red por defecto', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new NetworkBannerComponent();
      expect(comp.isOnline()).toBe(true);
      expect(comp.isLowBandwidth()).toBe(false);
      expect(comp.connectionType()).toBe('4g');
    });
  });

  it('ConvocatoriaCardComponent: debe instanciarse con inputs por defecto', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new ConvocatoriaCardComponent();
      expect(comp.estaAbierta()).toBe(false);
      expect(comp.diasRestantes()).toBeNull();
      expect(comp.detalle()).toBe('Período Ordinario de Titulación');
    });
  });

  it('StepperComponent: debe instanciarse con etapa 1 por defecto', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new StepperComponent();
      expect(comp.etapaActual()).toBe(1);
      expect(comp.tienePostulacion()).toBe(false);
    });
  });

  it('KpiCardComponent: debe instanciarse con tamaños y valores por defecto', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new KpiCardComponent();
      expect(comp.valueFontSize()).toBe('1.75rem');
    });
  });

  it('DictamenModalComponent: debe emitir evento confirm con datos estructurados al llamar onConfirm', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new DictamenModalComponent();
      let emittedValue: unknown = null;
      comp.confirm.subscribe((val) => (emittedValue = val));

      (comp as unknown as { data: () => unknown }).data = () => ({
        idPostulacion: 101,
        decision: 'APROBAR',
        observaciones: 'Cumple con todos los requisitos',
      });

      comp.onConfirm();
      expect(emittedValue).toBeDefined();
      const result = emittedValue as { idPostulacionAlumnos: number; decision: string };
      expect(result.idPostulacionAlumnos).toBe(101);
      expect(result.decision).toBe('APROBAR');
    });
  });

  it('AperturaPeriodoModalComponent: debe emitir evento confirm con payload institucional al llamar onConfirm', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new AperturaPeriodoModalComponent();
      let emittedValue: unknown = null;
      comp.confirm.subscribe((val) => (emittedValue = val));

      comp.onConfirm();
      expect(emittedValue).toBeDefined();
      const result = emittedValue as { idPeriodo: string; habilitarTodasLasCarreras: boolean };
      expect(result.idPeriodo).toBe('ABR2026');
      expect(result.habilitarTodasLasCarreras).toBe(true);
    });
  });

  it('AperturaPeriodoModalComponent: debe guardar y recuperar borrador en sessionStorage y limpiarlo al cancelar', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new AperturaPeriodoModalComponent();
      comp.form.set({
        idPeriodo: 'OCT2026',
        detalleConvocatoria: 'Convocatoria Test Persistencia',
        fechaInicioStr: '2026-10-01',
        fechaFinStr: '2026-10-20',
        diasPermitidos: 100,
        diasExtension: 45,
        habilitarTodasLasCarreras: false,
      });
      comp.pasoActual.set(3);
      comp.guardarBorrador();

      // Instanciar nuevo componente y recuperar
      const comp2 = new AperturaPeriodoModalComponent();
      const recuperado = comp2.recuperarBorrador();
      expect(recuperado).toBe(true);
      expect(comp2.form().idPeriodo).toBe('OCT2026');
      expect(comp2.form().detalleConvocatoria).toBe('Convocatoria Test Persistencia');
      expect(comp2.pasoActual()).toBe(3);

      // Cancelar debe limpiar el borrador
      comp2.cancelarApertura();
      expect(comp2.pasoActual()).toBe(1);
      const raw = sessionStorage.getItem('istpet_convocatoria_apertura_draft');
      expect(raw).toBeNull();
    });
  });

  describe('DrawerComponent', () => {
    it('debe instanciarse con valores por defecto', () => {
      TestBed.runInInjectionContext(() => {
        const comp = new DrawerComponent();
        expect(comp.isOpen()).toBe(false);
        expect(comp.size()).toBe('md');
        expect(comp.hasDirtyData()).toBe(false);
        expect(comp.showDiscardModal()).toBe(false);
      });
    });

    it('debe cerrarse directamente al llamar requestClose() si hasDirtyData es false', () => {
      TestBed.runInInjectionContext(() => {
        const comp = new DrawerComponent();
        let closedEmitted = false;
        comp.closed.subscribe(() => (closedEmitted = true));

        comp.requestClose();

        expect(comp.showDiscardModal()).toBe(false);
        expect(closedEmitted).toBe(true);
      });
    });

    it('debe desplegar el modal de advertencia al llamar requestClose() si hasDirtyData es true', () => {
      TestBed.runInInjectionContext(() => {
        const comp = new DrawerComponent();
        (comp as unknown as { hasDirtyData: () => boolean }).hasDirtyData = () => true;

        let closedEmitted = false;
        comp.closed.subscribe(() => (closedEmitted = true));

        comp.requestClose();

        expect(comp.showDiscardModal()).toBe(true);
        expect(closedEmitted).toBe(false);

        // Cancelar descarte
        comp.cancelDiscard();
        expect(comp.showDiscardModal()).toBe(false);
        expect(closedEmitted).toBe(false);
      });
    });

    it('debe emitir discardConfirmed y cerrar cuando el usuario confirma descartar datos', () => {
      TestBed.runInInjectionContext(() => {
        const comp = new DrawerComponent();
        (comp as unknown as { hasDirtyData: () => boolean }).hasDirtyData = () => true;

        let discardEmitted = false;
        let closedEmitted = false;
        comp.discardConfirmed.subscribe(() => (discardEmitted = true));
        comp.closed.subscribe(() => (closedEmitted = true));

        comp.requestClose();
        expect(comp.showDiscardModal()).toBe(true);

        comp.confirmDiscard();
        expect(comp.showDiscardModal()).toBe(false);
        expect(discardEmitted).toBe(true);
        expect(closedEmitted).toBe(true);
      });
    });
  });
});
