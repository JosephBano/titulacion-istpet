// @vitest-environment jsdom
import '@angular/compiler';
import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { BrowserTestingModule, platformBrowserTesting } from '@angular/platform-browser/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ReportesTabComponent } from './reportes-tab.component';
import { ReportesService } from '../../../../core/services/reportes.service';
import { EgresadosService } from '../../../../core/services/egresados.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { API_BASE_URL } from '../../../../core/config/api.config';

try {
  TestBed.initTestEnvironment(BrowserTestingModule, platformBrowserTesting());
} catch {
  // Entorno ya inicializado
}

describe('ReportesTabComponent Unit Tests', () => {
  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        ReportesService,
        EgresadosService,
        NotificationService,
        { provide: API_BASE_URL, useValue: 'http://localhost:5000' },
      ],
    });
  });

  it('debe tener únicamente reportes disponibles y activos en el catálogo (sin elementos próximamente)', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new ReportesTabComponent();
      const catalogo = comp.catalogo();
      expect(catalogo.length).toBeGreaterThan(0);
      const todosDisponibles = catalogo.every(
        (r) => r.disponible === true && r.badge !== 'Próximamente',
      );
      expect(todosDisponibles).toBe(true);
    });
  });

  it('no debe iniciar con el drawer abierto por defecto', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new ReportesTabComponent();
      expect(comp.drawerAbierto()).toBe(false);
    });
  });

  it('debe abrir el drawer y cargar datos al invocar abrirReporte', () => {
    TestBed.runInInjectionContext(() => {
      const comp = new ReportesTabComponent();
      const primerReporte = comp.catalogo()[0];
      comp.abrirReporte(primerReporte);
      expect(comp.drawerAbierto()).toBe(true);
      expect(comp.reporteSeleccionado().id).toBe(primerReporte.id);

      comp.cerrarDrawer();
      expect(comp.drawerAbierto()).toBe(false);
    });
  });
});
