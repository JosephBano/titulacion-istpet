import { describe, it, expect } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NetworkBannerComponent } from './network-banner/network-banner.component';
import { ConvocatoriaCardComponent } from './convocatoria-card/convocatoria-card.component';
import { StepperComponent } from './stepper/stepper.component';
import { KpiCardComponent } from './kpi-card/kpi-card.component';
import { DictamenModalComponent } from './dictamen-modal/dictamen-modal.component';
import { AperturaPeriodoModalComponent } from './apertura-periodo-modal/apertura-periodo-modal.component';

import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

describe('Shared UI Components Tests', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
  });

  it('NetworkBannerComponent: debe renderizar alerta de modo sin conexión cuando isOnline es false', () => {
    const fixture: ComponentFixture<NetworkBannerComponent> = TestBed.createComponent(NetworkBannerComponent);
    fixture.componentRef.setInput('isOnline', false);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.network-banner.offline')).toBeDefined();
    expect(compiled.textContent).toContain('Modo Local Desconectado');
  });

  it('ConvocatoriaCardComponent: debe mostrar estado activo y cuenta regresiva de días', () => {
    const fixture: ComponentFixture<ConvocatoriaCardComponent> = TestBed.createComponent(ConvocatoriaCardComponent);
    fixture.componentRef.setInput('estaAbierta', true);
    fixture.componentRef.setInput('detalle', 'Convocatoria Ordinaria 2026-I');
    fixture.componentRef.setInput('diasRestantes', 15);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.status-dot.active')).toBeDefined();
    expect(compiled.textContent).toContain('Convocatoria Activa');
    expect(compiled.textContent).toContain('15');
    expect(compiled.textContent).toContain('Días Restantes');
  });

  it('StepperComponent: debe marcar las etapas completadas según la etapa actual', () => {
    const fixture: ComponentFixture<StepperComponent> = TestBed.createComponent(StepperComponent);
    fixture.componentRef.setInput('etapaActual', 3);
    fixture.componentRef.setInput('estadoNombre', 'Modalidad Asignada');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const completedSteps = compiled.querySelectorAll('.stepper-step.completed');
    expect(completedSteps.length).toBe(3);
  });

  it('KpiCardComponent: debe renderizar correctamente el valor y el texto de contexto', () => {
    const fixture: ComponentFixture<KpiCardComponent> = TestBed.createComponent(KpiCardComponent);
    fixture.componentRef.setInput('eyebrow', 'POSTULACIONES ACTIVAS');
    fixture.componentRef.setInput('value', 42);
    fixture.componentRef.setInput('subtext', 'Período 2026-I');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('POSTULACIONES ACTIVAS');
    expect(compiled.textContent).toContain('42');
    expect(compiled.textContent).toContain('Período 2026-I');
  });

  it('DictamenModalComponent: debe emitir evento confirm al confirmar dictamen', () => {
    const fixture: ComponentFixture<DictamenModalComponent> = TestBed.createComponent(DictamenModalComponent);
    fixture.componentRef.setInput('data', {
      idPostulacion: 101,
      decision: 'APROBAR',
      observaciones: 'Cumple con todos los requisitos',
    });
    fixture.detectChanges();

    let emittedValue: unknown = null;
    fixture.componentInstance.confirm.subscribe((val) => (emittedValue = val));

    fixture.componentInstance.onConfirm();
    expect(emittedValue).toBeDefined();
    const result = emittedValue as { idPostulacionAlumnos: number; decision: string };
    expect(result.idPostulacionAlumnos).toBe(101);
    expect(result.decision).toBe('APROBAR');
  });

  it('AperturaPeriodoModalComponent: debe emitir evento confirm con datos estructurados', () => {
    const fixture: ComponentFixture<AperturaPeriodoModalComponent> = TestBed.createComponent(AperturaPeriodoModalComponent);
    fixture.componentRef.setInput('visible', true);
    fixture.detectChanges();

    let emittedValue: unknown = null;
    fixture.componentInstance.confirm.subscribe((val) => (emittedValue = val));

    fixture.componentInstance.onConfirm();
    expect(emittedValue).toBeDefined();
    const result = emittedValue as { idPeriodo: string; habilitarTodasLasCarreras: boolean };
    expect(result.idPeriodo).toBe('ABR2026');
    expect(result.habilitarTodasLasCarreras).toBe(true);
  });
});
