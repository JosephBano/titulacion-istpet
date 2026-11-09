// @vitest-environment jsdom
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BrowserTestingModule, platformBrowserTesting } from '@angular/platform-browser/testing';
import { of } from 'rxjs';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { PagosTabComponent } from './pagos-tab.component';
import { PagosTitulacionService } from '../../../../core/services/pagos-titulacion.service';
import { TitulacionService } from '../../../../core/services/titulacion.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AuthService } from '../../../../core/services/auth.service';
import {
  ConfiguracionPagoModalidad,
  PagoTitulacion,
  ResumenFinancieroEstudiante,
} from '../../../../core/models/pagos-titulacion.models';

try {
  TestBed.initTestEnvironment(BrowserTestingModule, platformBrowserTesting());
} catch {
  // Inicializado por el runner
}

describe('PagosTabComponent', () => {
  let component: PagosTabComponent;
  let fixture: ComponentFixture<PagosTabComponent>;

  let pagosServiceMock: {
    getBandejaContabilidad: ReturnType<typeof vi.fn>;
    procesarRevisionContabilidad: ReturnType<typeof vi.fn>;
    getResumenFinanciero: ReturnType<typeof vi.fn>;
    registrarPago: ReturnType<typeof vi.fn>;
    getConfiguraciones: ReturnType<typeof vi.fn>;
    guardarConfiguracion: ReturnType<typeof vi.fn>;
    getHistorial: ReturnType<typeof vi.fn>;
  };
  let titulacionServiceMock: {
    getPostulaciones: ReturnType<typeof vi.fn>;
  };
  let notificationServiceMock: {
    success: ReturnType<typeof vi.fn>;
    error: ReturnType<typeof vi.fn>;
    warning: ReturnType<typeof vi.fn>;
    info: ReturnType<typeof vi.fn>;
  };
  let authServiceMock: {
    hasRole: ReturnType<typeof vi.fn>;
  };

  const mockPagos: PagoTitulacion[] = [
    {
      idPagoTitulacion: 1,
      idPostulacionAlumno: 10,
      estudianteCedula: '1720000001',
      estudianteNombre: 'Estudiante Prueba Uno',
      carreraNombre: 'Desarrollo de Software',
      modalidadNombre: 'Examen Complexivo',
      idMatricula: 100,
      tipoRubro: 'BASE',
      numeroFactura: 'FAC-001',
      valor: 700.0,
      estado: 'REGISTRADO',
      fechaRegistro: new Date().toISOString(),
      idUsuarioRegistro: 5,
      usuarioRegistroNombre: 'Secretaria General',
      observacion: 'Pago total complexivo',
      motivoRechazo: null,
      fechaRevision: null,
      idUsuarioRevision: null,
      usuarioRevisionNombre: null,
      comprobanteArchivo: null,
    },
    {
      idPagoTitulacion: 2,
      idPostulacionAlumno: 11,
      estudianteCedula: '1720000002',
      estudianteNombre: 'Estudiante Prueba Dos',
      carreraNombre: 'Diseño Gráfico',
      modalidadNombre: 'Propuesta Tecnológica',
      idMatricula: 101,
      tipoRubro: 'INVERSION',
      numeroFactura: 'FAC-002',
      valor: 150.0,
      estado: 'APROBADO',
      fechaRegistro: new Date().toISOString(),
      idUsuarioRegistro: 5,
      usuarioRegistroNombre: 'Secretaria General',
      observacion: null,
      motivoRechazo: null,
      fechaRevision: new Date().toISOString(),
      idUsuarioRevision: 8,
      usuarioRevisionNombre: 'Contador Oficial',
      comprobanteArchivo: null,
    },
  ];

  const mockResumenComplexivo: ResumenFinancieroEstudiante = {
    idPostulacionAlumno: 10,
    idMatricula: 100,
    estudianteCedula: '1720000001',
    estudianteNombre: 'Estudiante Prueba Uno',
    carreraNombre: 'Desarrollo de Software',
    modalidadNombre: 'Examen Complexivo',
    idModalidadTitulacion: 1,
    idPeriodo: '2026-S1',
    valorBaseMaximo: 700.0,
    inversionMaxima: 0.0,
    totalTitulacion: 700.0,
    totalBasePagado: 0.0,
    totalInversionPagado: 0.0,
    totalPagado: 0.0,
    saldoBaseDisponible: 700.0,
    saldoInversionDisponible: 0.0,
    saldoTotalPendiente: 700.0,
    pagos: [],
  };

  const mockResumenPropuesta: ResumenFinancieroEstudiante = {
    idPostulacionAlumno: 11,
    idMatricula: 101,
    estudianteCedula: '1720000002',
    estudianteNombre: 'Estudiante Prueba Dos',
    carreraNombre: 'Diseño Gráfico',
    modalidadNombre: 'Propuesta Tecnológica',
    idModalidadTitulacion: 2,
    idPeriodo: '2026-S1',
    valorBaseMaximo: 500.0,
    inversionMaxima: 200.0,
    totalTitulacion: 700.0,
    totalBasePagado: 0.0,
    totalInversionPagado: 0.0,
    totalPagado: 0.0,
    saldoBaseDisponible: 500.0,
    saldoInversionDisponible: 200.0,
    saldoTotalPendiente: 700.0,
    pagos: [],
  };

  const mockConfiguraciones: ConfiguracionPagoModalidad[] = [
    {
      idConfiguracionPago: 1,
      idModalidadTitulacion: 1,
      modalidadNombre: 'Examen Complexivo',
      idPeriodo: null,
      valorBase: 700.0,
      maximoInversion: 0.0,
      totalTitulacion: 700.0,
      idEspecie: null,
      especieNombre: null,
      activo: true,
      fechaRegistro: new Date().toISOString(),
      fechaModificacion: null,
    },
    {
      idConfiguracionPago: 2,
      idModalidadTitulacion: 2,
      modalidadNombre: 'Propuesta Tecnológica',
      idPeriodo: null,
      valorBase: 500.0,
      maximoInversion: 200.0,
      totalTitulacion: 700.0,
      idEspecie: null,
      especieNombre: null,
      activo: true,
      fechaRegistro: new Date().toISOString(),
      fechaModificacion: null,
    },
  ];

  beforeEach(async () => {
    TestBed.resetTestingModule();

    pagosServiceMock = {
      getBandejaContabilidad: vi.fn().mockReturnValue(of(mockPagos)),
      procesarRevisionContabilidad: vi.fn(),
      getResumenFinanciero: vi.fn(),
      registrarPago: vi.fn(),
      getConfiguraciones: vi.fn().mockReturnValue(of(mockConfiguraciones)),
      guardarConfiguracion: vi.fn(),
      getHistorial: vi.fn().mockReturnValue(of([])),
    };

    titulacionServiceMock = {
      getPostulaciones: vi
        .fn()
        .mockReturnValue(of({ items: [], total: 0, pagina: 1, tamanoPagina: 20, totalPaginas: 1 })),
    };

    notificationServiceMock = {
      success: vi.fn(),
      error: vi.fn(),
      warning: vi.fn(),
      info: vi.fn(),
    };

    authServiceMock = {
      hasRole: vi.fn().mockImplementation((role: string) => role === 'CONTABILIDAD'),
    };

    await TestBed.configureTestingModule({
      imports: [PagosTabComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: PagosTitulacionService, useValue: pagosServiceMock },
        { provide: TitulacionService, useValue: titulacionServiceMock },
        { provide: NotificationService, useValue: notificationServiceMock },
        { provide: AuthService, useValue: authServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PagosTabComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('debe crearse correctamente e inicializar en bandeja para usuario de contabilidad', () => {
    expect(component).toBeTruthy();
    expect(component.isContabilidad()).toBe(true);
    expect(component.puedeAprobarRechazar()).toBe(true);
    expect(component.subTabActiva()).toBe('bandeja');
    expect(component.pagosBandeja().length).toBe(2);
  });

  it('permite a Contabilidad aprobar un pago registrado', () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    pagosServiceMock.procesarRevisionContabilidad.mockReturnValue(
      of({ ...mockPagos[0], estado: 'APROBADO' }),
    );

    component.aprobarPago(mockPagos[0]);

    expect(pagosServiceMock.procesarRevisionContabilidad).toHaveBeenCalledWith(
      1,
      expect.objectContaining({ aprobar: true }),
    );
    expect(notificationServiceMock.success).toHaveBeenCalledWith(
      expect.stringMatching(/aprobado/i),
    );
  });

  it('rechaza pago con motivo obligatorio y valida longitud mínima', () => {
    component.abrirModalRechazo(mockPagos[0]);
    expect(component.pagoParaRechazar()).toEqual(mockPagos[0]);

    // Motivo vacío
    component.motivoRechazoTexto.set('   ');
    component.confirmarRechazo();
    expect(notificationServiceMock.warning).toHaveBeenCalled();
    expect(pagosServiceMock.procesarRevisionContabilidad).not.toHaveBeenCalled();

    // Motivo válido
    component.motivoRechazoTexto.set('Factura no corresponde al RUC del instituto');
    pagosServiceMock.procesarRevisionContabilidad.mockReturnValue(
      of({ ...mockPagos[0], estado: 'RECHAZADO', motivoRechazo: 'Factura no corresponde' }),
    );

    component.confirmarRechazo();

    expect(pagosServiceMock.procesarRevisionContabilidad).toHaveBeenCalledWith(
      1,
      expect.objectContaining({
        aprobar: false,
        motivoRechazo: 'Factura no corresponde al RUC del instituto',
      }),
    );
    expect(notificationServiceMock.success).toHaveBeenCalledWith(
      expect.stringMatching(/rechazado.*liberado/i),
    );
    expect(component.pagoParaRechazar()).toBeNull();
  });

  it('valida topes de examen complexivo ($700 base, $0 inversion)', () => {
    component.resumenFinanciero.set(mockResumenComplexivo);
    component.nuevoPagoTipoRubro.set('BASE');

    // Monto válido $700
    component.onValorInput(700);
    expect(component.errorValidacionValor()).toBeNull();

    // Monto superior a $700 debe fallar e indicar disponible y tope
    component.onValorInput(701);
    expect(component.errorValidacionValor()).toContain('supera el saldo disponible de $700.00');

    // Inversión en complexivo debe dar error
    component.nuevoPagoTipoRubro.set('INVERSION');
    component.onValorInput(50);
    expect(component.errorValidacionValor()).toContain('no contempla rubro de inversión');
  });

  it('valida topes de propuesta tecnologica ($500 base, hasta $200 inversion, rechaza $201)', () => {
    component.resumenFinanciero.set(mockResumenPropuesta);

    // Rubro Inversión: $200 permitido
    component.nuevoPagoTipoRubro.set('INVERSION');
    component.onValorInput(200);
    expect(component.errorValidacionValor()).toBeNull();

    // Rubro Inversión: $201 rechazado
    component.onValorInput(201);
    expect(component.errorValidacionValor()).toContain('supera el saldo disponible de $200.00');

    // Rubro Base: $500 permitido, $501 rechazado
    component.nuevoPagoTipoRubro.set('BASE');
    component.onValorInput(500);
    expect(component.errorValidacionValor()).toBeNull();

    component.onValorInput(501);
    expect(component.errorValidacionValor()).toContain('supera el saldo disponible de $500.00');
  });

  it('valida politica de configuracion de modalidades (suma = $700 y max inversion <= $200)', () => {
    component.configuracionEnEdicion.set(mockConfiguraciones[1]);

    // Caso inválido: inversión > 200
    component.editValorBase.set(450);
    component.editMaximoInversion.set(250);
    component.onEditValoresChange();
    expect(component.errorConfiguracion()).toContain(
      'no puede superar el tope normativo de $200.00',
    );

    // Caso inválido: suma != 700
    component.editValorBase.set(500);
    component.editMaximoInversion.set(150); // total 650
    component.onEditValoresChange();
    expect(component.errorConfiguracion()).toContain('debe ser exactamente igual a $700.00');

    // Caso válido: 500 base + 200 inv
    component.editValorBase.set(500);
    component.editMaximoInversion.set(200);
    component.onEditValoresChange();
    expect(component.errorConfiguracion()).toBeNull();
  });
});
