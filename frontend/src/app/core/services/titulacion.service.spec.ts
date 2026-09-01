import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TitulacionService } from './titulacion.service';
import { NetworkStatusService } from './network-status.service';
import { API_BASE_URL } from '../config/api.config';
import { PortalEstudiante, HealthCheckResponse } from '../models/titulacion.models';

describe('TitulacionService & Network Resilience Tests', () => {
  let service: TitulacionService;
  let networkService: NetworkStatusService;
  let httpMock: HttpTestingController;

  const mockApiUrl = 'http://localhost:5000';

  beforeEach(() => {
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        TitulacionService,
        NetworkStatusService,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: mockApiUrl },
      ],
    });

    service = TestBed.inject(TitulacionService);
    networkService = TestBed.inject(NetworkStatusService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  it('debe obtener el estado de Health Check del backend', () => {
    const mockHealth: HealthCheckResponse = {
      status: 'Healthy',
      totalDurationMs: 15.2,
      environment: 'Development',
      timestampUtc: '2026-08-31T12:00:00Z',
      version: '1.0.0',
      checks: [
        {
          name: 'mysql_sigafi_database',
          status: 'Healthy',
          durationMs: 14.1,
          description: null,
          error: null,
        },
      ],
    };

    service.getHealthStatus().subscribe((res) => {
      expect(res.status).toBe('Healthy');
      expect(res.checks.length).toBe(1);
    });

    const req = httpMock.expectOne(`${mockApiUrl}/health`);
    expect(req.request.method).toBe('GET');
    req.flush(mockHealth);
  });

  it('debe consultar el portal del estudiante y guardar en caché local', () => {
    const mockPortal: PortalEstudiante = {
      convocatoria: {
        estaAbierta: true,
        periodo: '2026-I',
        detalle: 'Convocatoria 2026-I',
        fechaInicio: '2026-09-01',
        fechaCierre: '2026-09-30',
        diasRestantes: 25,
        mensaje: 'Convocatoria vigente',
      },
      estudiante: {
        idAlumno: '1720000001',
        cedula: '1720000001',
        nombreCompleto: 'Juan Pérez',
        email: 'juan@istpet.edu.ec',
        celular: '0999999999',
        idCarrera: 1,
        nombreCarrera: 'Desarrollo de Software',
        idMatricula: 100,
        esElegible: true,
        mensajeElegibilidad: 'Apto para titulación',
      },
      postulacionActiva: null,
      modalidadesDisponibles: [],
    };

    service.getMiPortal().subscribe((res) => {
      expect(res.convocatoria.estaAbierta).toBe(true);
      expect(res.estudiante.cedula).toBe('1720000001');

      // Verificar que se guardó en cache
      const cached = networkService.getCachedData<PortalEstudiante>('portal_estudiante_cache');
      expect(cached).toBeDefined();
      expect(cached?.estudiante.nombreCompleto).toBe('Juan Pérez');
    });

    const req = httpMock.expectOne(`${mockApiUrl}/api/v1/postulaciones/mi-portal`);
    expect(req.request.method).toBe('GET');
    req.flush(mockPortal);
  });

  it('Resiliencia Offline: debe responder con datos cacheados si se pierde la conexión a internet', () => {
    const mockCachedPortal: PortalEstudiante = {
      convocatoria: {
        estaAbierta: true,
        periodo: '2026-I',
        detalle: 'Convocatoria en Caché',
        fechaInicio: '2026-09-01',
        fechaCierre: '2026-09-30',
        diasRestantes: 20,
        mensaje: 'Datos sin conexión',
      },
      estudiante: {
        idAlumno: '1720000001',
        cedula: '1720000001',
        nombreCompleto: 'Juan Pérez Caché',
        email: 'juan@istpet.edu.ec',
        celular: '0999999999',
        idCarrera: 1,
        nombreCarrera: 'Desarrollo de Software',
        idMatricula: 100,
        esElegible: true,
        mensajeElegibilidad: 'Apto',
      },
      postulacionActiva: null,
      modalidadesDisponibles: [],
    };

    // Pre-guardar en cache
    networkService.setCachedData('portal_estudiante_cache', mockCachedPortal, 30);

    // Simular modo offline
    networkService.isOnline.set(false);

    service.getMiPortal().subscribe((res) => {
      expect(res.convocatoria.detalle).toBe('Convocatoria en Caché');
      expect(res.estudiante.nombreCompleto).toBe('Juan Pérez Caché');
    });

    // Ninguna petición HTTP debe haberse realizado
    httpMock.expectNone(`${mockApiUrl}/api/v1/postulaciones/mi-portal`);
  });

  it('debe obtener la lista de postulaciones para el administrador con paginación y filtros', () => {
    const mockRes = {
      items: [
        {
          idPostulacionAlumnos: 1,
          idMatricula: 10,
          idAlumno: '1720000001',
          nombreAlumno: 'Carlos Gómez',
          cedulaAlumno: '1720000001',
          idCarrera: 1,
          nombreCarrera: 'Desarrollo de Software',
          idCohorte: 1,
          detalleCohorte: '2026-I',
          idModalidadTitulacionCarrera: 1,
          modalidadTitulacion: 'Examen Complexivo',
          idPostulacionEstado: 1,
          nombreEstado: 'Registrada',
          esActivo: true,
          esCambioModalidad: null,
          totalRequisitos: 4,
          totalRequisitosCompletados: 3,
        },
      ],
      pagina: 1,
      tamanoPagina: 20,
      total: 1,
    };

    service.getPostulaciones(1, 20, 1).subscribe((res) => {
      expect(res.items.length).toBe(1);
      expect(res.total).toBe(1);
      expect(res.items[0].nombreAlumno).toBe('Carlos Gómez');
    });

    const req = httpMock.expectOne(
      `${mockApiUrl}/api/v1/postulaciones?pagina=1&tamanoPagina=20&idCarrera=1`,
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockRes);
  });

  it('debe listar y crear modalidades maestras', () => {
    const mockModalidades = [
      {
        idModalidadTitulacion: 1,
        modalidadTitulacion: 'Examen Complexivo',
        esComplexivo: 'SI',
        esArticuloCientifico: 'NO',
        generaTesis: 'NO',
        esActivo: true,
        totalRequisitosAsociados: 3,
      },
    ];

    service.getModalidadesMaestras().subscribe((data) => {
      expect(data.length).toBe(1);
      expect(data[0].modalidadTitulacion).toBe('Examen Complexivo');
    });

    const req = httpMock.expectOne(
      `${mockApiUrl}/api/v1/configuracion/modalidades?soloActivas=false`,
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockModalidades);
  });
});
