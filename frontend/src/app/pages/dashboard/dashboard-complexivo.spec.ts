// @vitest-environment jsdom
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { AuthService } from '../../core/services/auth.service';
import { CarrerasService } from '../../core/services/carreras.service';
import { TitulacionService } from '../../core/services/titulacion.service';
import { NetworkStatusService } from '../../core/services/network-status.service';
import { NotificationService } from '../../core/services/notification.service';
import { RequisicionPersonalInicial } from '../../domain/models/requisicion-personal.model';
import { DashboardComponent } from './dashboard.component';

describe('Navegación a requisición desde el curso complexivo', () => {
  const datos: RequisicionPersonalInicial = {
    cargo: 'Docente para curso complexivo de la carrera de Diseño gráfico',
    numeroVacantes: 3,
    origen: {
      idCohorteCarrera: 1,
      carrera: 'Diseño gráfico',
      cohorte: 'OCT2026',
      idsMallaComplexivo: [1, 2, 3],
    },
  };
  let dashboard: DashboardComponent;
  let tienePermiso: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    TestBed.resetTestingModule();
    tienePermiso = vi.fn().mockReturnValue(true);
    TestBed.configureTestingModule({
      providers: [
        {
          provide: AuthService,
          useValue: { currentUser: signal(null), hasRole: () => true, hasPermission: tienePermiso },
        },
        { provide: CarrerasService, useValue: {} },
        { provide: TitulacionService, useValue: {} },
        {
          provide: NetworkStatusService,
          useValue: {
            isOnline: signal(true),
            isLowBandwidth: signal(false),
            connectionType: signal('wifi'),
          },
        },
        { provide: NotificationService, useValue: {} },
        { provide: Router, useValue: {} },
      ],
    });
    dashboard = TestBed.runInInjectionContext(() => new DashboardComponent());
    dashboard.activeTab.set('configuracion-complexivo');
  });

  it('traslada los datos, abre la requisición y los limpia al volver a configuración', () => {
    dashboard.abrirRequisicionPersonal(datos);
    expect(dashboard.activeTab()).toBe('requisicion-personal');
    expect(dashboard.requisicionInicial()).toEqual(datos);
    dashboard.setActiveTab('configuracion-complexivo');
    expect(dashboard.requisicionInicial()).toBeNull();
    dashboard.setActiveTab('requisicion-personal');
    expect(dashboard.requisicionInicial()).toBeNull();
  });

  it('respeta el permiso de acceso antes de trasladar datos o navegar', () => {
    tienePermiso.mockReturnValue(false);
    dashboard.abrirRequisicionPersonal(datos);
    expect(dashboard.activeTab()).toBe('configuracion-complexivo');
    expect(dashboard.requisicionInicial()).toBeNull();
  });
});
