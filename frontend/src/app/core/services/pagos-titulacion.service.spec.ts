// @vitest-environment jsdom
import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { BrowserTestingModule, platformBrowserTesting } from '@angular/platform-browser/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { PagosTitulacionService } from './pagos-titulacion.service';
import { API_BASE_URL } from '../config/api.config';
import {
  ConfiguracionPagoModalidad,
  PagoTitulacion,
  ResumenFinancieroEstudiante,
} from '../models/pagos-titulacion.models';

try {
  TestBed.initTestEnvironment(BrowserTestingModule, platformBrowserTesting());
} catch {
  // Entorno ya inicializado
}

describe('PagosTitulacionService', () => {
  let service: PagosTitulacionService;
  let httpMock: HttpTestingController;
  const mockApiUrl = 'http://localhost:5000';

  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        PagosTitulacionService,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: mockApiUrl },
      ],
    });

    service = TestBed.inject(PagosTitulacionService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    TestBed.resetTestingModule();
  });

  it('debe obtener las configuraciones de pago por modalidad', () => {
    const mockConfigs: ConfiguracionPagoModalidad[] = [
      {
        idConfiguracionPago: 1,
        idModalidadTitulacion: 1,
        modalidadNombre: 'Examen Complexivo',
        idPeriodo: null,
        valorBase: 700,
        maximoInversion: 0,
        totalTitulacion: 700,
        idEspecie: null,
        especieNombre: null,
        activo: true,
        fechaRegistro: '2026-11-06T10:00:00Z',
        fechaModificacion: null,
      },
      {
        idConfiguracionPago: 2,
        idModalidadTitulacion: 2,
        modalidadNombre: 'Propuesta Tecnológica',
        idPeriodo: null,
        valorBase: 500,
        maximoInversion: 200,
        totalTitulacion: 700,
        idEspecie: null,
        especieNombre: null,
        activo: true,
        fechaRegistro: '2026-11-06T10:00:00Z',
        fechaModificacion: null,
      },
    ];

    service.getConfiguraciones().subscribe((res) => {
      expect(res).toEqual(mockConfigs);
      expect(res.length).toBe(2);
      expect(res[0].valorBase).toBe(700);
      expect(res[1].maximoInversion).toBe(200);
    });

    const req = httpMock.expectOne(`${mockApiUrl}/api/v1/pagos-titulacion/configuraciones`);
    expect(req.request.method).toBe('GET');
    req.flush(mockConfigs);
  });

  it('debe registrar un pago de valor base para el estudiante', () => {
    const mockPago: PagoTitulacion = {
      idPagoTitulacion: 10,
      idPostulacionAlumno: 101,
      estudianteCedula: '1720000001',
      estudianteNombre: 'Juan Carlos Pérez López',
      carreraNombre: 'Desarrollo de Software',
      modalidadNombre: 'Examen Complexivo',
      idMatricula: 500,
      tipoRubro: 'BASE',
      numeroFactura: 'FAC-001',
      valor: 700,
      estado: 'REGISTRADO',
      fechaRegistro: '2026-11-06T11:00:00Z',
      idUsuarioRegistro: 1,
      usuarioRegistroNombre: 'Secretaría General',
      observacion: null,
      motivoRechazo: null,
      fechaRevision: null,
      idUsuarioRevision: null,
      usuarioRevisionNombre: null,
      comprobanteArchivo: null,
    };

    service
      .registrarPago({
        idPostulacionAlumno: 101,
        tipoRubro: 'BASE',
        numeroFactura: 'FAC-001',
        valor: 700,
      })
      .subscribe((res) => {
        expect(res.estado).toBe('REGISTRADO');
        expect(res.valor).toBe(700);
        expect(res.numeroFactura).toBe('FAC-001');
      });

    const req = httpMock.expectOne(`${mockApiUrl}/api/v1/pagos-titulacion/registrar`);
    expect(req.request.method).toBe('POST');
    req.flush(mockPago);
  });

  it('debe enviar la revisión de Contabilidad (aprobar o rechazar con motivo)', () => {
    const mockAprobado: PagoTitulacion = {
      idPagoTitulacion: 10,
      idPostulacionAlumno: 101,
      estudianteCedula: '1720000001',
      estudianteNombre: 'Juan Carlos Pérez López',
      carreraNombre: 'Desarrollo de Software',
      modalidadNombre: 'Examen Complexivo',
      idMatricula: 500,
      tipoRubro: 'BASE',
      numeroFactura: 'FAC-001',
      valor: 700,
      estado: 'APROBADO',
      fechaRegistro: '2026-11-06T11:00:00Z',
      idUsuarioRegistro: 1,
      usuarioRegistroNombre: 'Secretaría General',
      observacion: 'Verificado',
      motivoRechazo: null,
      fechaRevision: '2026-11-06T12:00:00Z',
      idUsuarioRevision: 2,
      usuarioRevisionNombre: 'Contabilidad',
      comprobanteArchivo: null,
    };

    service
      .procesarRevisionContabilidad(10, {
        aprobar: true,
        observacion: 'Verificado',
      })
      .subscribe((res) => {
        expect(res.estado).toBe('APROBADO');
        expect(res.usuarioRevisionNombre).toBe('Contabilidad');
      });

    const req = httpMock.expectOne(
      `${mockApiUrl}/api/v1/pagos-titulacion/contabilidad/10/revision`,
    );
    expect(req.request.method).toBe('PUT');
    req.flush(mockAprobado);
  });

  it('debe obtener el resumen financiero de la postulación', () => {
    const mockResumen: ResumenFinancieroEstudiante = {
      idPostulacionAlumno: 101,
      idMatricula: 500,
      estudianteCedula: '1720000001',
      estudianteNombre: 'Juan Carlos Pérez López',
      carreraNombre: 'Desarrollo de Software',
      modalidadNombre: 'Propuesta Tecnológica',
      idModalidadTitulacion: 2,
      idPeriodo: '2026-S1',
      valorBaseMaximo: 500,
      inversionMaxima: 200,
      totalTitulacion: 700,
      totalBasePagado: 500,
      totalInversionPagado: 100,
      totalPagado: 600,
      saldoBaseDisponible: 0,
      saldoInversionDisponible: 100,
      saldoTotalPendiente: 100,
      pagos: [],
    };

    service.getResumenFinanciero(101).subscribe((res) => {
      expect(res.totalTitulacion).toBe(700);
      expect(res.totalPagado).toBe(600);
      expect(res.saldoTotalPendiente).toBe(100);
      expect(res.saldoInversionDisponible).toBe(100);
    });

    const req = httpMock.expectOne(`${mockApiUrl}/api/v1/pagos-titulacion/postulacion/101/resumen`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResumen);
  });
});
