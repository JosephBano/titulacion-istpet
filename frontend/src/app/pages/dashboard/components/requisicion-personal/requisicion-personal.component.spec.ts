// @vitest-environment jsdom
import { TestBed } from '@angular/core/testing';
import { BrowserTestingModule, platformBrowserTesting } from '@angular/platform-browser/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { RequisicionPersonalComponent } from './requisicion-personal.component';

try {
  TestBed.initTestEnvironment(BrowserTestingModule, platformBrowserTesting());
} catch {
  /* Inicializado por el runner. */
}

describe('Requisición de personal', () => {
  beforeEach(() => {
    TestBed.resetTestingModule();
  });

  it('genera un documento con datos actuales y espacios de firma', async () => {
    const fixture = TestBed.createComponent(RequisicionPersonalComponent);
    fixture.componentRef.setInput('nombreSolicitante', 'Solicitante actual');
    fixture.detectChanges();
    await fixture.whenStable();
    const cargo = fixture.nativeElement.querySelector('#cargo') as HTMLInputElement;
    cargo.value = 'Docente de preparación';
    cargo.dispatchEvent(new Event('input', { bubbles: true }));
    const competencia = Array.from(fixture.nativeElement.querySelectorAll('.check-option')).find(
      (element) => (element as HTMLElement).textContent?.includes('Trabajo en equipo'),
    ) as HTMLElement;
    const checkbox = competencia.querySelector('input') as HTMLInputElement;
    checkbox.checked = true;
    checkbox.dispatchEvent(new Event('change', { bubbles: true }));
    await fixture.whenStable();
    fixture.detectChanges();
    const documento = fixture.nativeElement.querySelector('.print-document') as HTMLElement;
    expect(documento.textContent).toContain('Solicitante actual');
    expect(documento.textContent).toContain('Docente de preparación');
    expect(documento.textContent).toContain('☑ Trabajo en equipo');
    expect(documento.querySelectorAll('.signature-line')).toHaveLength(3);
  });

  it('empieza nuevamente vacío cuando se abre otra instancia', () => {
    const anterior = TestBed.createComponent(RequisicionPersonalComponent);
    anterior.detectChanges();
    anterior.componentInstance.datos.cargo = 'Solicitud anterior';
    anterior.destroy();
    const nuevo = TestBed.createComponent(RequisicionPersonalComponent);
    nuevo.detectChanges();
    expect(nuevo.componentInstance.datos.cargo).toBe('');
    expect(nuevo.componentInstance.numeroVacantes).toBeNull();
  });

  it('precarga el cargo y las vacantes del curso manteniendo editables los campos y el solicitante actual', async () => {
    const fixture = TestBed.createComponent(RequisicionPersonalComponent);
    fixture.componentRef.setInput('nombreSolicitante', 'Pamela Parra');
    fixture.componentRef.setInput('datosIniciales', {
      cargo: 'Docente para curso complexivo de la carrera de Diseño gráfico',
      numeroVacantes: 3,
      origen: {
        idCohorteCarrera: 1,
        carrera: 'Diseño gráfico',
        cohorte: 'OCT2026',
        idsMallaComplexivo: [1, 2, 3],
      },
    });
    fixture.detectChanges();
    await fixture.whenStable();
    const cargo = fixture.nativeElement.querySelector('#cargo') as HTMLInputElement;
    const vacantes = fixture.nativeElement.querySelector('#vacantes') as HTMLInputElement;
    expect(cargo.value).toBe('Docente para curso complexivo de la carrera de Diseño gráfico');
    expect(vacantes.value).toBe('3');
    expect(fixture.componentInstance.datos.solicitante).toBe('Pamela Parra');
    expect(fixture.componentInstance.datos.area).toBe('Unidad de Titulación');
    expect(fixture.componentInstance.valorTotal).toBeNull();
    cargo.value = 'Docente de comunicación visual';
    cargo.dispatchEvent(new Event('input', { bubbles: true }));
    vacantes.value = '2';
    vacantes.dispatchEvent(new Event('input', { bubbles: true }));
    await fixture.whenStable();
    fixture.detectChanges();
    expect(fixture.componentInstance.datos.cargo).toBe('Docente de comunicación visual');
    expect(fixture.componentInstance.numeroVacantes).toBe(2);
    expect(fixture.componentInstance.datosIniciales()?.numeroVacantes).toBe(3);
    expect(fixture.nativeElement.querySelector('.print-document').textContent).toContain(
      'Docente de comunicación visual',
    );
  });

  it('abre la vista previa en un popup A4 con los datos actuales y mantiene oculto el documento del formulario', async () => {
    const fixture = TestBed.createComponent(RequisicionPersonalComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    const cargo = fixture.nativeElement.querySelector('#cargo') as HTMLInputElement;
    cargo.value = 'Requisición actualizada';
    cargo.dispatchEvent(new Event('input', { bubbles: true }));
    await fixture.whenStable();
    const dialog = fixture.nativeElement.querySelector('dialog') as HTMLDialogElement;
    const frame = dialog.querySelector('iframe') as HTMLIFrameElement;
    const documento = fixture.nativeElement.querySelector('.print-document') as HTMLElement;
    dialog.showModal = vi.fn();
    fixture.componentInstance.abrirVistaPrevia(dialog, frame, documento);
    expect(dialog.showModal).toHaveBeenCalledOnce();
    expect(documento.hidden).toBe(true);
    expect(frame.srcdoc).toContain('Requisición actualizada');
    expect(frame.srcdoc).toContain('size: A4');
    expect(frame.srcdoc).not.toContain('<form');
  });

  it('carga un ejemplo completo para tres docentes y 96 horas, listo para imprimir', async () => {
    const fixture = TestBed.createComponent(RequisicionPersonalComponent);
    fixture.componentRef.setInput('nombreSolicitante', 'Responsable de prueba');
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.componentInstance.competenciasSeleccionadas.add('Otro');
    const root = fixture.nativeElement as HTMLElement;
    const boton = Array.from(root.querySelectorAll<HTMLButtonElement>('button')).find(
      (elemento) => elemento.textContent?.trim() === 'Cargar ejemplo',
    );
    boton?.click();
    await fixture.whenStable();
    fixture.detectChanges();
    expect(fixture.componentInstance.datos.solicitante).toBe('Responsable de prueba');
    expect(fixture.componentInstance.numeroVacantes).toBe(3);
    expect(fixture.componentInstance.valorRemuneracion).toBe(20);
    expect(fixture.componentInstance.valorTotal).toBe(96 * 20);
    expect(fixture.componentInstance.competenciasSeleccionadas.has('Otro')).toBe(false);
    const imprimir = fixture.nativeElement.querySelector(
      'button[type="submit"]',
    ) as HTMLButtonElement;
    expect(imprimir.disabled).toBe(false);
    const documento = fixture.nativeElement.querySelector('.print-document') as HTMLElement;
    expect(documento.textContent).toContain('96 horas en total');
    expect(documento.textContent).toContain('DATOS DE EJEMPLO');
  });
});
