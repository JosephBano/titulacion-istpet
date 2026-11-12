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

  it('debe calcular correctamente pendientes, rechazados y aprobados', () => {
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
          estadoEvaluacion: 'APROBADO',
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
          estadoEvaluacion: 'RECHAZADO',
          observaciones: 'Falta firma del director',
        },
        {
          idPostulacionAlumnoRequisitoModalidad: 3,
          idPostulacionAlumnos: 12,
          idResponsableEvidencias: 1,
          idRequisitos: 102,
          nombreRequisito: 'Pago de Modalidad',
          idAlumno: 'A003',
          nombreAlumno: 'Carlos Gómez',
          cedulaAlumno: '1723456781',
          carrera: 'Desarrollo de Software',
          modalidad: 'Examen Complexivo',
          aprobado: false,
          estadoEvaluacion: 'PENDIENTE',
        },
      ];

      (comp as unknown as { items: () => RequisitoEvaluacionDocente[] }).items = () => mockItems;

      expect(comp.totalAprobados()).toBe(1);
      expect(comp.totalRechazados()).toBe(1);
      expect(comp.totalPendientes()).toBe(1);
      expect(comp.totalFiltrados()).toBe(3);

      // Filtro PENDIENTES
      comp.setFiltro('PENDIENTES');
      expect(comp.totalFiltrados()).toBe(1);
      expect(comp.itemsFiltrados()[0].idPostulacionAlumnoRequisitoModalidad).toBe(3);

      // Filtro RECHAZADOS
      comp.setFiltro('RECHAZADOS');
      expect(comp.totalFiltrados()).toBe(1);
      expect(comp.itemsFiltrados()[0].idPostulacionAlumnoRequisitoModalidad).toBe(2);

      // Filtro APROBADOS
      comp.setFiltro('APROBADOS');
      expect(comp.totalFiltrados()).toBe(1);
      expect(comp.itemsFiltrados()[0].idPostulacionAlumnoRequisitoModalidad).toBe(1);
    });
  });

  it('debe exigir observaciones al marcar o guardar como no cumple y cerrar el drawer al guardar', async () => {
    TestBed.runInInjectionContext(async () => {
      const comp = new EvaluacionDocenteTabComponent();
      const mockItem: RequisitoEvaluacionDocente = {
        idPostulacionAlumnoRequisitoModalidad: 5,
        idPostulacionAlumnos: 20,
        idResponsableEvidencias: 1,
        idRequisitos: 200,
        nombreRequisito: 'Certificado B2',
        idAlumno: 'A005',
        nombreAlumno: 'Pedro Picapiedra',
        cedulaAlumno: '1799999999',
        carrera: 'Desarrollo de Software',
        modalidad: 'Examen Complexivo',
        aprobado: false,
        estadoEvaluacion: 'PENDIENTE',
      };

      comp.abrirRevision(mockItem);
      expect(comp.drawerAbierto()).toBe(true);
      expect(comp.casoSeleccionado()).toBe(mockItem);
      // Al abrir no debe empezar premarcado ni aprobado ni rechazado
      expect(comp.getAprobado(mockItem)).toBeNull();

      // Si se intenta guardar sin haber seleccionado opción, no avanza
      await comp.guardarCasoActual();
      expect(comp.drawerAbierto()).toBe(true);

      // Si se marca No Cumple sin observaciones, activa flag de error
      comp.setAprobado(mockItem, false);
      expect(comp.getAprobado(mockItem)).toBe(false);
      expect(comp.errorObservaciones()).toBe(true);

      // Intentar guardar con observaciones vacías no debe cerrar el drawer
      await comp.guardarCasoActual();
      expect(comp.errorObservaciones()).toBe(true);
      expect(comp.drawerAbierto()).toBe(true);

      // Al escribir observaciones válidas, se limpia el error
      comp.setObservaciones(mockItem, 'Documento borroso y sin sello.');
      expect(comp.errorObservaciones()).toBe(false);

      // Al cerrar revisión, se limpia el estado
      comp.cerrarRevision();
      expect(comp.drawerAbierto()).toBe(false);
      expect(comp.casoSeleccionado()).toBeNull();
      expect(comp.errorObservaciones()).toBe(false);
    });
  });
});
