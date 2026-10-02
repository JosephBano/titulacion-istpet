// @vitest-environment jsdom
import '@angular/compiler';
import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { BrowserTestingModule, platformBrowserTesting } from '@angular/platform-browser/testing';
import { EvaluacionDocenteTabComponent } from './evaluacion-docente-tab.component';
import { RequisitoEvaluacionDocente } from '../../../../core/models/titulacion.models';

try {
  TestBed.initTestEnvironment(BrowserTestingModule, platformBrowserTesting());
} catch {
  // Entorno ya inicializado
}

describe('EvaluacionDocenteTabComponent Unit Tests', () => {
  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({});
  });

  it('debe inicializarse con filtros y skeletonRows por defecto', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new EvaluacionDocenteTabComponent();
      expect(comp.busqueda()).toBe('');
      expect(comp.filtroCumplimiento()).toBe('TODOS');
      expect(comp.filtroCarrera()).toBe('TODAS');
      expect(comp.skeletonRows.length).toBe(5);
    });
  });

  it('debe calcular correctamente pendientes y aprobados', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new EvaluacionDocenteTabComponent();
      const mockItems: RequisitoEvaluacionDocente[] = [
        {
          idPostulacionAlumnoRequisitoModalidad: 1,
          idPostulacionAlumnos: 10,
          idResponsableEvidencias: 1,
          idRequisitos: 100,
          nombreRequisito: 'Certificado de Inglés B1',
          idAlumno: 'A001',
          nombreAlumno: 'Juan Pérez',
          cedulaAlumno: '1723456789',
          carrera: 'Desarrollo de Software',
          modalidad: 'Examen Complexivo',
          aprobado: true,
        },
        {
          idPostulacionAlumnoRequisitoModalidad: 2,
          idPostulacionAlumnos: 11,
          idResponsableEvidencias: 1,
          idRequisitos: 101,
          nombreRequisito: 'Certificado de Vinculación',
          idAlumno: 'A002',
          nombreAlumno: 'María López',
          cedulaAlumno: '1723456780',
          carrera: 'Desarrollo de Software',
          modalidad: 'Examen Complexivo',
          aprobado: false,
        },
      ];

      (comp as unknown as { items: () => RequisitoEvaluacionDocente[] }).items = () => mockItems;

      expect(comp.totalAprobados()).toBe(1);
      expect(comp.totalPendientes()).toBe(1);
      expect(comp.totalFiltrados()).toBe(2);
    });
  });
});
