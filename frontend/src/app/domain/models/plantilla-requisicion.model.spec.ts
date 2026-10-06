import { describe, expect, it } from 'vitest';
import {
  PLANTILLA_REQUISICION_BASE,
  resolverValoresIniciales,
} from './plantilla-requisicion.model';
import { RequisicionPersonalInicial } from './requisicion-personal.model';

describe('resolverValoresIniciales', () => {
  it('resuelve variables del curso cuando hay contexto', () => {
    const contexto: RequisicionPersonalInicial = {
      cargo: '',
      numeroVacantes: 3,
      origen: {
        idCohorteCarrera: 27,
        carrera: 'Diseño Gráfico',
        cohorte: 'OCT2026',
        idsMallaComplexivo: [101, 102, 103],
      },
    };

    const res = resolverValoresIniciales(
      PLANTILLA_REQUISICION_BASE,
      contexto,
      'Pamela Parra',
    );

    expect(res.cargo).toBe(
      'Docente para curso complexivo de la carrera de Diseño Gráfico',
    );
    expect(res.numeroVacantes).toBe(3);
    expect(res.solicitante).toBe('Pamela Parra');
    expect(res.area).toBe('Unidad de Titulación');
    expect(res.institucion).toBe('istpet');
    expect(res.contratacion).toBe('servicios_profesionales');
  });

  it('deja el cargo vacío y sin marcadores {{...}} cuando no hay contexto de curso', () => {
    const res = resolverValoresIniciales(
      PLANTILLA_REQUISICION_BASE,
      null,
      'Pamela Parra',
    );

    expect(res.cargo).toBe('');
    expect(res.numeroVacantes).toBeNull();
    expect(res.solicitante).toBe('Pamela Parra');
    expect(res.area).toBe('Unidad de Titulación');
    expect(res.cargo).not.toContain('{{');
  });

  it('respeta la fecha actual en formato local YYYY-MM-DD', () => {
    const res = resolverValoresIniciales(
      PLANTILLA_REQUISICION_BASE,
      null,
      'Usuario Test',
    );

    expect(res.fechaSolicitud).toMatch(/^\d{4}-\d{2}-\d{2}$/);
  });
});
