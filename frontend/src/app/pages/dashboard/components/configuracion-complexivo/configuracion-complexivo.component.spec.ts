// @vitest-environment jsdom
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BrowserTestingModule, platformBrowserTesting } from '@angular/platform-browser/testing';
import { By } from '@angular/platform-browser';
import { NgForm } from '@angular/forms';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import {
  COMPLEXIVO_REPOSITORY,
  ComplexivoRepository,
} from '../../../../domain/repositories/complexivo.repository';
import {
  DocenteComplexivo,
  GuardarMallaComplexivo,
  MallaComplexivo,
} from '../../../../domain/models/complexivo.model';
import { crearPlanificacionPdf } from '../../../../shared/utils/complexivo-planning-pdf';
import { ConfiguracionComplexivoComponent } from './configuracion-complexivo.component';

try {
  TestBed.initTestEnvironment(BrowserTestingModule, platformBrowserTesting());
} catch {
  /* Inicializado por el runner. */
}

describe('Editor de módulos complexivos', () => {
  let fixture: ComponentFixture<ConfiguracionComplexivoComponent>;
  let repo: ComplexivoRepository;
  let form: NgForm;

  beforeEach(async () => {
    TestBed.resetTestingModule();
    repo = {
      cohortesCarreras: vi.fn().mockResolvedValue([
        {
          idCohorteCarrera: 1,
          idCohorte: 1,
          idCarrera: 1,
          cohorte: '2026',
          periodo: 'Abril',
          carrera: 'Diseño gráfico',
          modalidadEstudio: 'Presencial',
          esActivo: true,
        },
      ]),
      profesores: vi.fn().mockResolvedValue([
        { idProfesor: '0601234567', nombresCompletos: 'José Parra', activo: true },
        { idProfesor: '0607654321', nombresCompletos: 'Andrea Pérez', activo: true },
      ]),
      asignaturas: vi.fn().mockResolvedValue([{ idAsignatura: 10, nombre: 'Semiótica' }]),
      malla: vi.fn().mockResolvedValue([]),
      guardar: vi
        .fn()
        .mockImplementation(async (_id: number | null, datos: GuardarMallaComplexivo) => ({
          ...structuredClone(datos),
          idMallaComplexivo: 1,
          idModulosComplexivo: 1,
          temas: datos.temas.map((tema, i) => ({ ...tema, idTemas: tema.idTemas || i + 1 })),
          docentes: datos.docentes.map((docente, d) => ({
            ...docente,
            idMallaDocenteComplexivo: docente.idMallaDocenteComplexivo || d + 10,
            horarios: docente.horarios.map((horario, h) => ({
              ...horario,
              idModulosHorarios: horario.idModulosHorarios || d * 10 + h + 20,
            })),
          })),
        })),
    };
    TestBed.configureTestingModule({
      providers: [{ provide: COMPLEXIVO_REPOSITORY, useValue: repo }],
    });
    fixture = TestBed.createComponent(ConfiguracionComplexivoComponent);
    fixture.componentRef.setInput('puedeCrear', true);
    fixture.componentRef.setInput('puedeEditar', true);
    fixture.detectChanges();
    await vi.waitFor(() => expect(fixture.componentInstance.store.cargando()).toBe(false));
    await fixture.whenStable();
    fixture.detectChanges();
    (
      fixture.nativeElement.querySelector('.toolbar-actions .button-primary') as HTMLButtonElement
    ).click();
    Object.assign(fixture.componentInstance.editor!, {
      detalle: 'Módulo 1',
      detalleModulo: 'Fundamentos',
      modalidad: 'Mixta',
      fechaInicio: '2026-10-05',
      fechaFin: '2026-10-24',
      totalHoras: 32,
      temas: [{ idTemas: 0, idAsignatura: 10, detalle: 'Signos y símbolos', horasPorTema: 8 }],
    });
    fixture.componentRef.changeDetectorRef.markForCheck();
    fixture.detectChanges();
    await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('form')).not.toBeNull();
    form = fixture.debugElement.query(By.css('form')).injector.get(NgForm);
  });

  it('explica las 24 horas faltantes al guardar 8/32 y permite corregir el total explícitamente', async () => {
    const button = fixture.nativeElement.querySelector(
      'button[type="submit"]',
    ) as HTMLButtonElement;
    expect(button.disabled).toBe(false);
    await fixture.componentInstance.guardar(form);
    fixture.detectChanges();
    expect(repo.guardar).not.toHaveBeenCalled();
    expect(fixture.nativeElement.querySelector('.validation-summary').textContent).toContain(
      'Faltan 24 horas',
    );
    fixture.componentInstance.usarHorasTemas();
    fixture.changeDetectorRef.markForCheck();
    fixture.detectChanges();
    await fixture.whenStable();
    await fixture.componentInstance.guardar(form);
    expect(repo.guardar).toHaveBeenCalledWith(
      null,
      expect.objectContaining({ totalHoras: 8, fechaInicio: '2026-10-05', fechaFin: '2026-10-24' }),
    );
    expect(fixture.componentInstance.editor).toBeNull();
  });

  it('registra los calendarios en el formulario y rechaza fechas imposibles o anteriores al inicio', async () => {
    const input = fixture.nativeElement.querySelector('#modulo-fin') as HTMLInputElement;
    input.value = '31/02/2026';
    input.dispatchEvent(new Event('input', { bubbles: true }));
    await fixture.whenStable();
    expect(form.controls['fechaFin'].hasError('matDatepickerParse')).toBe(true);
    input.value = '04/10/2026';
    input.dispatchEvent(new Event('input', { bubbles: true }));
    await fixture.whenStable();
    expect(form.controls['fechaFin'].hasError('matDatepickerMin')).toBe(true);
    await fixture.componentInstance.guardar(form);
    expect(repo.guardar).not.toHaveBeenCalled();
    input.value = '24/10/2026';
    input.dispatchEvent(new Event('input', { bubbles: true }));
    await fixture.whenStable();
    expect(form.controls['fechaFin'].valid).toBe(true);
    expect(fixture.componentInstance.editor?.fechaFin).toBe('2026-10-24');
  });

  it('busca docentes sin tildes o por cédula y conserva el identificador al seleccionar un resultado', async () => {
    const component = fixture.componentInstance;
    component.agregarDocente();
    const docente = component.editor!.docentes[0];
    component.busquedasDocentes.set(docente, 'jose');
    expect(component.profesoresFiltrados(docente).map((p) => p.idProfesor)).toEqual(['0601234567']);
    component.busquedasDocentes.set(docente, '7654');
    expect(component.profesoresFiltrados(docente).map((p) => p.idProfesor)).toEqual(['0607654321']);
    expect(docente.idProfesor).toBeNull();
    component.busquedasDocentes.clear();
    fixture.componentRef.changeDetectorRef.markForCheck();
    fixture.detectChanges();
    await fixture.whenStable();
    const input = fixture.nativeElement.querySelector('#docente-0') as HTMLInputElement;
    input.focus();
    input.value = 'jose';
    input.dispatchEvent(new Event('input', { bubbles: true }));
    await fixture.whenStable();
    const opciones = Array.from(document.querySelectorAll<HTMLElement>('mat-option'));
    expect(opciones.some((opcion) => opcion.textContent?.includes('Andrea Pérez'))).toBe(false);
    expect(opciones.some((opcion) => opcion.textContent?.includes('Pendiente de asignación'))).toBe(
      true,
    );
    const resultado = opciones.find((opcion) => opcion.textContent?.includes('José Parra'));
    expect(resultado).toBeDefined();
    resultado?.click();
    await fixture.whenStable();
    expect(docente.idProfesor).toBe('0601234567');
  });

  it('guarda horarios sin docente y permite asignarlo después conservando la asignación y los horarios', async () => {
    const component = fixture.componentInstance;
    component.usarHorasTemas();
    fixture.componentRef.changeDetectorRef.markForCheck();
    fixture.detectChanges();
    await fixture.whenStable();
    const agregarHorario = fixture.nativeElement.querySelector(
      '.editor-section:last-of-type .section-toolbar button',
    ) as HTMLButtonElement;
    agregarHorario.click();
    agregarHorario.click();
    expect(component.editor!.docentes).toHaveLength(1);
    const pendiente = component.editor!.docentes[0];
    expect(pendiente.idProfesor).toBeNull();
    Object.assign(pendiente.horarios[0], {
      dias: 'Lunes a jueves',
      franjaHoraria: '19:00 a 21:00 (virtual)',
    });
    Object.assign(pendiente.horarios[1], {
      dias: 'Sábado 24/10/2026',
      franjaHoraria: '08:00 a 10:00 (presencial)',
    });
    fixture.componentRef.changeDetectorRef.markForCheck();
    fixture.detectChanges();
    await fixture.whenStable();
    await component.guardar(form);
    expect(repo.guardar).toHaveBeenCalledWith(
      null,
      expect.objectContaining({
        docentes: [
          expect.objectContaining({
            idProfesor: null,
            horarios: expect.arrayContaining([
              expect.objectContaining({
                dias: 'Lunes a jueves',
                franjaHoraria: '19:00 a 21:00 (virtual)',
              }),
            ]),
          }),
        ],
      }),
    );
    expect(component.editor).toBeNull();
    const guardado = component.store.malla()[0];
    component.editar(guardado);
    fixture.componentRef.changeDetectorRef.markForCheck();
    fixture.detectChanges();
    await fixture.whenStable();
    form = fixture.debugElement.query(By.css('form')).injector.get(NgForm);
    const input = fixture.nativeElement.querySelector('#docente-0') as HTMLInputElement;
    input.focus();
    input.value = 'jose';
    input.dispatchEvent(new Event('input', { bubbles: true }));
    await fixture.whenStable();
    const resultado = Array.from(document.querySelectorAll<HTMLElement>('mat-option')).find(
      (opcion) => opcion.textContent?.includes('José Parra'),
    );
    expect(resultado).toBeDefined();
    resultado!.click();
    await fixture.whenStable();
    await component.guardar(form);
    expect(repo.guardar).toHaveBeenLastCalledWith(
      1,
      expect.objectContaining({
        docentes: [
          {
            ...guardado.docentes[0],
            idProfesor: '0601234567',
          },
        ],
      }),
    );
    expect(component.store.malla()[0].docentes[0].horarios).toEqual(guardado.docentes[0].horarios);
  });

  const asignacion = (
    idProfesor: string | null,
    conHorario: boolean,
    esActivo = true,
  ): DocenteComplexivo => ({
    idMallaDocenteComplexivo: 10,
    idProfesor,
    fechaAsignacion: null,
    codigoAsignacion: null,
    esActivo,
    horarios: conHorario
      ? [{ idModulosHorarios: 20, dias: 'Lunes', franjaHoraria: '19:00 a 21:00' }]
      : [],
  });

  it.each([
    { docentes: [], sinDocentes: true, sinHorarios: true },
    { docentes: [asignacion(null, true)], sinDocentes: true, sinHorarios: false },
    { docentes: [asignacion('0601234567', false)], sinDocentes: false, sinHorarios: true },
    { docentes: [asignacion('0601234567', true, false)], sinDocentes: true, sinHorarios: true },
  ])(
    'muestra el total y avisos independientes según los docentes y horarios activos: %j',
    ({ docentes, sinDocentes, sinHorarios }) => {
      const component = fixture.componentInstance;
      const modulo: MallaComplexivo = {
        ...structuredClone(component.editor!),
        totalHoras: 8,
        idMallaComplexivo: 1,
        idModulosComplexivo: 1,
        docentes,
      };
      component.cerrarEditor();
      component.store.malla.set([modulo]);
      fixture.detectChanges();
      const card = fixture.nativeElement.querySelector('.module-card') as HTMLElement;
      expect(card.querySelector('.module-total')?.textContent).toContain(
        'Total de horas del módulo',
      );
      expect(card.querySelector('.module-total strong')?.textContent?.trim()).toBe('8 horas');
      const avisos = card.querySelector('.planning-status')?.textContent ?? '';
      expect(avisos.includes('No hay docentes activos asignados')).toBe(sinDocentes);
      expect(avisos.includes('No hay horarios configurados')).toBe(sinHorarios);
      expect(fixture.nativeElement.textContent).not.toContain('Crear módulos de ejemplo');
      expect(fixture.nativeElement.textContent.includes('Solicitar requisición de personal')).toBe(
        sinDocentes,
      );
    },
  );

  it('solicita tres vacantes para tres módulos sin docente y excluye módulos cubiertos o inactivos', () => {
    const component = fixture.componentInstance;
    const base: MallaComplexivo = {
      ...structuredClone(component.editor!),
      idMallaComplexivo: 1,
      idModulosComplexivo: 1,
    };
    component.cerrarEditor();
    component.store.malla.set([
      ...[1, 2, 3].map((id) => ({ ...base, idMallaComplexivo: id, idModulosComplexivo: id })),
      { ...base, idMallaComplexivo: 4, docentes: [asignacion('0601234567', true)] },
      { ...base, idMallaComplexivo: 5, esActivo: false },
    ]);
    const recibir = vi.fn();
    component.requisicionSolicitada.subscribe(recibir);
    fixture.detectChanges();
    const boton = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll('button'),
    ).find((elemento) => elemento.textContent?.trim() === 'Solicitar requisición de personal');
    expect(boton?.disabled).toBe(false);
    boton!.click();
    expect(recibir).toHaveBeenCalledWith({
      cargo: 'Docente para curso complexivo de la carrera de Diseño gráfico',
      numeroVacantes: 3,
      origen: {
        idCohorteCarrera: 1,
        carrera: 'Diseño gráfico',
        cohorte: '2026',
        idsMallaComplexivo: [1, 2, 3],
      },
    });
  });

  it('cuenta las asignaciones pendientes explícitas y bloquea la navegación mientras se edita o guarda', () => {
    const component = fixture.componentInstance;
    const base: MallaComplexivo = {
      ...structuredClone(component.editor!),
      idMallaComplexivo: 1,
      idModulosComplexivo: 1,
      docentes: [
        asignacion(null, true),
        { ...asignacion(null, false), idMallaDocenteComplexivo: 11 },
      ],
    };
    component.store.malla.set([base]);
    const recibir = vi.fn();
    component.requisicionSolicitada.subscribe(recibir);
    component.solicitarPersonal();
    expect(recibir).not.toHaveBeenCalled();
    component.cerrarEditor();
    expect(component.vacantesRequeridas()).toBe(2);
    component.store.guardando.set(true);
    component.solicitarPersonal();
    expect(recibir).not.toHaveBeenCalled();
    component.store.guardando.set(false);
    component.solicitarPersonal();
    expect(recibir).toHaveBeenCalledWith(expect.objectContaining({ numeroVacantes: 2 }));
    component.store.malla.set([]);
    component.solicitarPersonal();
    expect(recibir).toHaveBeenCalledTimes(1);
  });

  it('imprime las seis columnas con celdas combinadas, totales activos y horarios pendientes', () => {
    const component = fixture.componentInstance;
    const base: MallaComplexivo = {
      ...structuredClone(component.editor!),
      idMallaComplexivo: 1,
      idModulosComplexivo: 1,
      docentes: [asignacion(null, true)],
    };
    const modulos = [0, 1, 2].map((i) => ({
      ...structuredClone(base),
      idMallaComplexivo: i + 1,
      idModulosComplexivo: i + 1,
      detalle: `Módulo ${i + 1}`,
      docentes: i === 1 ? [] : base.docentes,
      temas: [
        { idTemas: 1, idAsignatura: 10, detalle: 'Signos y símbolos', horasPorTema: 4 },
        { idTemas: 2, idAsignatura: 10, detalle: 'Figuras retóricas', horasPorTema: 8 },
        { idTemas: 3, idAsignatura: 20, detalle: 'Composición', horasPorTema: 20 },
      ],
    }));
    component.cerrarEditor();
    component.store.asignaturas.set([
      { idAsignatura: 10, nombre: 'Semiótica' },
      { idAsignatura: 20, nombre: 'Fotografía' },
    ]);
    component.store.malla.set([
      ...modulos,
      { ...base, idMallaComplexivo: 99, detalle: 'Inactivo', esActivo: false },
    ]);
    fixture.detectChanges();
    const documento = fixture.nativeElement.querySelector('.complexivo-planning') as HTMLElement;
    const html = documento;
    const curriculum = html.querySelector('.curriculum-table')!;
    expect(
      Array.from(curriculum.querySelectorAll('thead th')).map((th) => th.textContent?.trim()),
    ).toEqual(['MÓDULO', 'TEMA', 'HORAS POR TEMA', 'MATERIAS', 'DOCENTES', 'TOTAL DE HORAS']);
    expect(curriculum.querySelector('.module-description')?.getAttribute('rowspan')).toBe('3');
    expect(curriculum.querySelector('td[rowspan="2"]')?.textContent?.trim()).toBe('Semiótica');
    expect(curriculum.querySelector('.course-total td')?.textContent).toContain('96');
    expect(curriculum.textContent).not.toContain('Inactivo');
    for (const color of [0, 1, 2])
      expect(curriculum.querySelector(`.module-tone-${color}`)).not.toBeNull();
    const horarios = html.querySelector('.schedules-table')!;
    expect(horarios.querySelectorAll('tbody tr')).toHaveLength(3);
    expect(horarios.textContent).toContain('Lunes · 19:00 a 21:00');
    expect(horarios.textContent).toContain('Sin docentes asignados');
    expect(horarios.textContent).toContain('No hay horarios configurados');
    const pdf = crearPlanificacionPdf(documento);
    expect(pdf.internal.pageSize.getWidth()).toBeCloseTo(210, 1);
    expect(pdf.internal.pageSize.getHeight()).toBeCloseTo(297, 1);
    const contenido = pdf.output();
    expect(contenido.startsWith('%PDF-')).toBe(true);
    expect(contenido).toContain('TOTAL DE HORAS DEL CURSO');
    expect(contenido).toContain('Figuras');
    expect(contenido).toContain('No hay horarios configurados');
    expect(contenido).not.toContain('about:srcdoc');
    expect(contenido).not.toContain('1 of 1');
    const grosores = [...contenido.matchAll(/([\d.]+) w/g)].map((m) => Number(m[1]));
    expect(grosores.some((grosor) => grosor > 0.9 && grosor < 1.1)).toBe(true);
    expect(contenido.match(/ re\s+B/g)?.length).toBeGreaterThan(20);
    const rellenos = [...contenido.matchAll(/([\d.]+) ([\d.]+) ([\d.]+) rg/g)].map((m) => [
      Number(m[1]),
      Number(m[2]),
      Number(m[3]),
    ]);
    for (const color of [
      [201, 229, 241],
      [242, 201, 208],
      [217, 232, 198],
    ]) {
      expect(
        rellenos.some((rgb) => rgb.every((canal, i) => Math.abs(canal - color[i] / 255) < 0.02)),
      ).toBe(true);
    }
  });
});
