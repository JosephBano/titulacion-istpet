// @vitest-environment jsdom
import '@angular/compiler';
import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { BrowserTestingModule, platformBrowserTesting } from '@angular/platform-browser/testing';
import { CronogramaTabComponent } from './cronograma-tab.component';
import { ConvocatoriaDetalle } from '../../../../core/models/titulacion.models';

try {
  TestBed.initTestEnvironment(BrowserTestingModule, platformBrowserTesting());
} catch {
  // Entorno ya inicializado
}

describe('CronogramaTabComponent Unit Tests', () => {
  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [CronogramaTabComponent],
    });
  });

  it('debe crearse correctamente sin datos de BD y reflejar estado vacío', () => {
    const fixture = TestBed.createComponent(CronogramaTabComponent);
    fixture.detectChanges();
    const comp = fixture.componentInstance;

    expect(comp).toBeTruthy();
    const data = comp.cronogramaData();
    expect(data.tieneDatosReales).toBe(false);
    expect(data.etapas.length).toBe(0);
    expect(data.resumenEstado).toBeNull();
  });

  it('calcula las etapas reales de BD cuando se asigna una convocatoria con fechas válidas', () => {
    const fixture = TestBed.createComponent(CronogramaTabComponent);
    const mockConv: ConvocatoriaDetalle = {
      idCohorte: 1,
      idPeriodo: '2026-P1',
      detalle: 'Cohorte Titulación 2026',
      fechaInicio: '2026-11-01',
      fechaFin: '2026-11-30',
      diasPermitidos: 60,
      diasExtension: 15,
      esActivo: true,
      estaVigenteCorte: true,
      carrerasHabilitadas: [],
    };

    fixture.componentRef.setInput('convocatoria', mockConv);
    fixture.detectChanges();
    const comp = fixture.componentInstance;

    const data = comp.cronogramaData();
    expect(data.tieneDatosReales).toBe(true);
    expect(data.periodo).toBe('2026-P1');
    expect(data.detalleConvocatoria).toBe('Cohorte Titulación 2026');

    // Deben existir 3 etapas reales: Postulación, Desarrollo y Prórroga (porque diasExtension > 0)
    expect(data.etapas.length).toBe(3);

    const etapa1 = data.etapas[0];
    expect(etapa1.numero).toBe(1);
    expect(etapa1.titulo).toContain('Postulaciones');
    expect(etapa1.duracionDias).toBe(30);

    const etapa2 = data.etapas[1];
    expect(etapa2.numero).toBe(2);
    expect(etapa2.titulo).toContain('Desarrollo');
    expect(etapa2.duracionDias).toBe(60);

    const etapa3 = data.etapas[2];
    expect(etapa3.numero).toBe(3);
    expect(etapa3.titulo).toContain('Prórroga');
    expect(etapa3.duracionDias).toBe(15);

    expect(data.resumenEstado).toBeTruthy();
  });

  it('no genera etapa de prórroga si diasExtension es 0', () => {
    const fixture = TestBed.createComponent(CronogramaTabComponent);
    const mockConv: ConvocatoriaDetalle = {
      idCohorte: 2,
      idPeriodo: '2026-P2',
      detalle: 'Cohorte Sin Prórroga',
      fechaInicio: '2026-12-01',
      fechaFin: '2026-12-31',
      diasPermitidos: 45,
      diasExtension: 0,
      esActivo: true,
      estaVigenteCorte: true,
      carrerasHabilitadas: [],
    };

    fixture.componentRef.setInput('convocatoria', mockConv);
    fixture.detectChanges();
    const comp = fixture.componentInstance;

    const data = comp.cronogramaData();
    expect(data.tieneDatosReales).toBe(true);
    expect(data.etapas.length).toBe(2);
    expect(data.etapas[0].numero).toBe(1);
    expect(data.etapas[1].numero).toBe(2);
  });
});
