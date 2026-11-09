// @vitest-environment jsdom
import '@angular/compiler';
import { TestBed } from '@angular/core/testing';
import { BrowserTestingModule, platformBrowserTesting } from '@angular/platform-browser/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { AuthService } from '../../core/services/auth.service';
import { LoginComponent } from './login.component';

try {
  TestBed.initTestEnvironment(BrowserTestingModule, platformBrowserTesting());
} catch {
  // Entorno ya inicializado
}

describe('LoginComponent - Flujo de Recuperación de Clave', () => {
  let component: LoginComponent;
  let authServiceMock: {
    login: ReturnType<typeof vi.fn>;
    validarDocumentoRecuperacion: ReturnType<typeof vi.fn>;
    validarFechaNacimiento: ReturnType<typeof vi.fn>;
    restablecerContrasena: ReturnType<typeof vi.fn>;
  };
  let routerMock: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    TestBed.resetTestingModule();

    authServiceMock = {
      login: vi.fn(),
      validarDocumentoRecuperacion: vi.fn(),
      validarFechaNacimiento: vi.fn(),
      restablecerContrasena: vi.fn(),
    };
    routerMock = {
      navigate: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        { provide: Router, useValue: routerMock },
      ],
    });

    TestBed.runInInjectionContext(() => {
      component = new LoginComponent();
    });
  });

  it('inicia en el modo login y permite activar el modo de recuperación', () => {
    expect(component.modoRecuperacion()).toBe(false);
    expect(component.pasoRecuperacion()).toBe('documento');

    component.activarModoRecuperacion();

    expect(component.modoRecuperacion()).toBe(true);
    expect(component.pasoRecuperacion()).toBe('documento');
  });

  it('muestra error si el documento está vacío al validar', () => {
    component.activarModoRecuperacion();
    component.recoveryDocumento = '';

    component.onVerificarDocumento();

    expect(component.recoveryError()).toContain('Por favor, ingresa tu número de documento');
    expect(authServiceMock.validarDocumentoRecuperacion).not.toHaveBeenCalled();
  });

  it('muestra error si el documento no se encuentra habilitado para titulación', () => {
    authServiceMock.validarDocumentoRecuperacion.mockReturnValue(
      of({
        valido: false,
        mensaje: 'El documento no se encuentra habilitado para el sistema.',
      }),
    );

    component.activarModoRecuperacion();
    component.recoveryDocumento = '1799999999';
    component.onVerificarDocumento();

    expect(component.recoveryError()).toContain('no se encuentra habilitado');
    expect(component.pasoRecuperacion()).toBe('documento');
  });

  it('maneja errores de conexión al validar documento', () => {
    authServiceMock.validarDocumentoRecuperacion.mockReturnValue(
      throwError(() => ({ error: { mensaje: 'Error de red' } })),
    );

    component.activarModoRecuperacion();
    component.recoveryDocumento = '1712345678';
    component.onVerificarDocumento();

    expect(component.recoveryError()).toBe('Error de red');
    expect(component.pasoRecuperacion()).toBe('documento');
  });

  it('desbloquea el paso de fecha de nacimiento cuando el documento es válido para titulación', () => {
    authServiceMock.validarDocumentoRecuperacion.mockReturnValue(
      of({
        valido: true,
        nombreCompleto: 'Alexander Martinez',
        mensaje: 'Documento verificado correctamente.',
      }),
    );

    component.activarModoRecuperacion();
    component.recoveryDocumento = '1712345678';
    component.onVerificarDocumento();

    expect(component.pasoRecuperacion()).toBe('fecha_nacimiento');
    expect(component.recoveryNombreUsuario()).toBe('Alexander Martinez');
    expect(component.recoverySuccess()).toContain('Alexander Martinez');
    expect(component.recoveryError()).toBeNull();
  });

  it('valida la fecha de nacimiento exitosamente y pasa a nueva_contrasena', () => {
    authServiceMock.validarFechaNacimiento.mockReturnValue(
      of({ valido: true, mensaje: 'Identidad validada con éxito.', token: 'token-abc' }),
    );

    component.activarModoRecuperacion();
    component.recoveryDocumento = '1712345678';
    component.pasoRecuperacion.set('fecha_nacimiento');
    component.recoveryFechaNacimiento = '1998-05-15';

    component.onVerificarFechaNacimiento();

    expect(authServiceMock.validarFechaNacimiento).toHaveBeenCalledWith('1712345678', '1998-05-15');
    expect(component.pasoRecuperacion()).toBe('nueva_contrasena');
    expect(component.recoveryToken()).toBe('token-abc');
    expect(component.recoverySuccess()).toContain('Identidad validada con éxito');
    expect(component.recoveryError()).toBeNull();
  });

  it('muestra error cuando la fecha de nacimiento no coincide', () => {
    authServiceMock.validarFechaNacimiento.mockReturnValue(
      of({ valido: false, mensaje: 'La fecha de nacimiento no coincide con los registros.' }),
    );

    component.activarModoRecuperacion();
    component.recoveryDocumento = '1712345678';
    component.pasoRecuperacion.set('fecha_nacimiento');
    component.recoveryFechaNacimiento = '2005-01-01';

    component.onVerificarFechaNacimiento();

    expect(component.recoveryError()).toContain('no coincide');
  });

  it('valida que las contraseñas coincidan y tengan entre 6 y 20 caracteres', () => {
    component.activarModoRecuperacion();
    component.pasoRecuperacion.set('nueva_contrasena');
    component.recoveryToken.set('token-abc');

    component.recoveryNuevaContrasenia = '123';
    component.recoveryConfirmarContrasenia = '123';
    component.onRestablecerContrasena();
    expect(component.recoveryError()).toContain('entre 6 y 20 caracteres');

    component.recoveryNuevaContrasenia = '123456789012345678901';
    component.recoveryConfirmarContrasenia = '123456789012345678901';
    component.onRestablecerContrasena();
    expect(component.recoveryError()).toContain('entre 6 y 20 caracteres');

    component.recoveryNuevaContrasenia = '123456';
    component.recoveryConfirmarContrasenia = '654321';
    component.onRestablecerContrasena();
    expect(component.recoveryError()).toContain('no coinciden');
  });

  it('restablece la contraseña exitosamente y regresa al login', () => {
    authServiceMock.restablecerContrasena.mockReturnValue(
      of({ exito: true, mensaje: 'Contraseña restablecida exitosamente.' }),
    );

    component.activarModoRecuperacion();
    component.recoveryDocumento = '1712345678';
    component.pasoRecuperacion.set('nueva_contrasena');
    component.recoveryToken.set('token-abc');
    component.recoveryNuevaContrasenia = 'NuevaClave123*';
    component.recoveryConfirmarContrasenia = 'NuevaClave123*';

    component.onRestablecerContrasena();

    expect(authServiceMock.restablecerContrasena).toHaveBeenCalledWith({
      numeroDocumento: '1712345678',
      token: 'token-abc',
      nuevaContrasenia: 'NuevaClave123*',
      confirmarContrasenia: 'NuevaClave123*',
    });
    expect(component.modoRecuperacion()).toBe(false);
    expect(component.usernameOrEmail).toBe('1712345678');
    expect(component.loginSuccessMessage()).toContain('Contraseña restablecida exitosamente');
  });
});
