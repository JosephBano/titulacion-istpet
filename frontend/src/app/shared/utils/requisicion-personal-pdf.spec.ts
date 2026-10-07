import { describe, expect, it } from 'vitest';
import { PLANTILLA_REQUISICION_BASE } from '../../domain/models/plantilla-requisicion.model';
import { crearRequisicionPdf } from './requisicion-personal-pdf';

describe('crearRequisicionPdf', () => {
  it('genera un documento jsPDF con formato A4 y propiedades correctas', async () => {
    const datos = {
      fechaSolicitud: '2026-10-06',
      area: 'Unidad de Titulación',
      solicitante: 'Pamela Parra',
      cargo: 'Docente para curso complexivo',
      numeroVacantes: 3,
      institucion: 'istpet',
      contratacion: 'servicios_profesionales',
      jornada: 'horas',
      remuneracion: 'factura',
      valorRemuneracion: 20,
      valorTotal: 1920,
      horario: '19:00 a 21:00',
      lugar: 'ISTPET Campus',
      fechaIngreso: '2026-10-15',
      motivo: 'proyecto_temporal',
      detalleMotivo: 'Preparación complexivo',
      formacion: 'Tercer nivel en Diseño',
      experiencia: '2 años',
      conocimientos: 'Diseño web',
      competencias: ['liderazgo', 'equipo'],
      observaciones: 'Ninguna',
    };

    const pdf = await crearRequisicionPdf(PLANTILLA_REQUISICION_BASE, datos);

    expect(pdf).toBeDefined();
    expect(pdf.getNumberOfPages()).toBeGreaterThanOrEqual(1);

    const blob = pdf.output('blob');
    expect(blob.size).toBeGreaterThan(1000);
    expect(blob.type).toBe('application/pdf');
  });
});
