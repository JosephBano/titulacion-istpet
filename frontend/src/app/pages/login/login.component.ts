import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

/**
 * Componente de inicio de sesión con soporte de modo claro/oscuro y validación institucional ISTPET.
 */
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  usernameOrEmail = '';
  password = '';
  systemCode = 'TITULACION';
  currentYear = new Date().getFullYear();

  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  loginSuccessMessage = signal<string | null>(null);
  isDarkMode = signal(false);
  showPassword = signal(false);

  // Estado para el flujo de Recuperación de Clave
  modoRecuperacion = signal(false);
  pasoRecuperacion = signal<'documento' | 'fecha_nacimiento' | 'nueva_contrasena'>('documento');
  recoveryDocumento = '';
  recoveryFechaNacimiento = '';
  recoveryNuevaContrasenia = '';
  recoveryConfirmarContrasenia = '';
  recoveryToken = signal<string | null>(null);
  showRecoveryPassword = signal(false);
  showRecoveryConfirmPassword = signal(false);
  recoveryNombreUsuario = signal<string | null>(null);
  recoveryLoading = signal(false);
  recoveryError = signal<string | null>(null);
  recoverySuccess = signal<string | null>(null);

  toggleShowPassword(): void {
    this.showPassword.set(!this.showPassword());
  }

  toggleShowRecoveryPassword(): void {
    this.showRecoveryPassword.set(!this.showRecoveryPassword());
  }

  toggleShowRecoveryConfirmPassword(): void {
    this.showRecoveryConfirmPassword.set(!this.showRecoveryConfirmPassword());
  }

  activarModoRecuperacion(): void {
    this.errorMessage.set(null);
    this.loginSuccessMessage.set(null);
    this.recoveryError.set(null);
    this.recoverySuccess.set(null);
    this.recoveryDocumento = this.usernameOrEmail.trim();
    this.recoveryFechaNacimiento = '';
    this.recoveryNuevaContrasenia = '';
    this.recoveryConfirmarContrasenia = '';
    this.recoveryToken.set(null);
    this.recoveryNombreUsuario.set(null);
    this.pasoRecuperacion.set('documento');
    this.modoRecuperacion.set(true);
  }

  cancelarRecuperacion(): void {
    this.modoRecuperacion.set(false);
    this.recoveryError.set(null);
    this.recoverySuccess.set(null);
    this.recoveryLoading.set(false);
    this.recoveryNuevaContrasenia = '';
    this.recoveryConfirmarContrasenia = '';
    this.recoveryToken.set(null);
    this.pasoRecuperacion.set('documento');
  }

  cambiarDocumentoRecuperacion(): void {
    this.pasoRecuperacion.set('documento');
    this.recoveryError.set(null);
    this.recoverySuccess.set(null);
    this.recoveryFechaNacimiento = '';
    this.recoveryNuevaContrasenia = '';
    this.recoveryConfirmarContrasenia = '';
    this.recoveryToken.set(null);
  }

  onVerificarDocumento(): void {
    if (!this.recoveryDocumento.trim()) {
      this.recoveryError.set('Por favor, ingresa tu número de documento de identidad.');
      return;
    }

    this.recoveryLoading.set(true);
    this.recoveryError.set(null);
    this.recoverySuccess.set(null);

    this.authService.validarDocumentoRecuperacion(this.recoveryDocumento).subscribe({
      next: (res) => {
        this.recoveryLoading.set(false);
        if (res.valido) {
          this.recoveryNombreUsuario.set(res.nombreCompleto || null);
          this.pasoRecuperacion.set('fecha_nacimiento');
          const saludo = res.nombreCompleto ? `¡Hola, ${res.nombreCompleto}! ` : '';
          this.recoverySuccess.set(
            `${saludo}Documento validado para titulación. Ingresa tu fecha de nacimiento para continuar.`,
          );
        } else {
          this.recoveryError.set(
            res.mensaje || 'El documento no se encuentra habilitado para el sistema.',
          );
        }
      },
      error: (err) => {
        this.recoveryLoading.set(false);
        const msg =
          err.error?.mensaje ||
          err.error?.message ||
          'Error de conexión al verificar el documento de identidad.';
        this.recoveryError.set(msg);
      },
    });
  }

  onVerificarFechaNacimiento(): void {
    if (!this.recoveryFechaNacimiento) {
      this.recoveryError.set('Por favor, ingresa tu fecha de nacimiento.');
      return;
    }

    this.recoveryLoading.set(true);
    this.recoveryError.set(null);
    this.recoverySuccess.set(null);

    this.authService
      .validarFechaNacimiento(this.recoveryDocumento, this.recoveryFechaNacimiento)
      .subscribe({
        next: (res) => {
          this.recoveryLoading.set(false);
          if (res.valido) {
            this.recoveryToken.set(res.token || null);
            this.pasoRecuperacion.set('nueva_contrasena');
            this.recoverySuccess.set(
              res.mensaje || 'Identidad validada con éxito. Por favor, define tu nueva contraseña.',
            );
          } else {
            this.recoveryError.set(
              res.mensaje || 'La fecha de nacimiento no coincide con los registros.',
            );
          }
        },
        error: (err) => {
          this.recoveryLoading.set(false);
          const msg =
            err.error?.mensaje ||
            err.error?.message ||
            'Error de conexión al verificar la fecha de nacimiento.';
          this.recoveryError.set(msg);
        },
      });
  }

  onRestablecerContrasena(): void {
    if (!this.recoveryNuevaContrasenia || !this.recoveryConfirmarContrasenia) {
      this.recoveryError.set('Por favor, completa ambos campos de contraseña.');
      return;
    }

    if (this.recoveryNuevaContrasenia !== this.recoveryConfirmarContrasenia) {
      this.recoveryError.set('Las contraseñas no coinciden.');
      return;
    }

    if (
      this.recoveryNuevaContrasenia.length < 6 ||
      this.recoveryNuevaContrasenia.length > 20
    ) {
      this.recoveryError.set('La nueva contraseña debe tener entre 6 y 20 caracteres.');
      return;
    }

    if (!this.recoveryToken()) {
      this.recoveryError.set(
        'Token de verificación no disponible o expirado. Por favor, reinicia el proceso.',
      );
      return;
    }

    this.recoveryLoading.set(true);
    this.recoveryError.set(null);
    this.recoverySuccess.set(null);

    this.authService
      .restablecerContrasena({
        numeroDocumento: this.recoveryDocumento,
        token: this.recoveryToken()!,
        nuevaContrasenia: this.recoveryNuevaContrasenia,
        confirmarContrasenia: this.recoveryConfirmarContrasenia,
      })
      .subscribe({
        next: (res) => {
          this.recoveryLoading.set(false);
          if (res.exito) {
            this.modoRecuperacion.set(false);
            this.pasoRecuperacion.set('documento');
            this.usernameOrEmail = this.recoveryDocumento;
            this.password = '';
            this.loginSuccessMessage.set(
              res.mensaje || 'Contraseña restablecida con éxito. Inicia sesión con tu nueva clave.',
            );
          } else {
            this.recoveryError.set(res.mensaje || 'No se pudo restablecer la contraseña.');
          }
        },
        error: (err) => {
          this.recoveryLoading.set(false);
          const msg =
            err.error?.mensaje ||
            err.error?.message ||
            'Error al restablecer la contraseña. Inténtalo de nuevo.';
          this.recoveryError.set(msg);
        },
      });
  }

  ngOnInit(): void {
    const savedTheme = localStorage.getItem('titulacion_theme') || 'light';
    this.setTheme(savedTheme === 'dark');
  }

  toggleTheme(): void {
    this.setTheme(!this.isDarkMode());
  }

  private setTheme(dark: boolean): void {
    this.isDarkMode.set(dark);
    const theme = dark ? 'dark' : 'light';
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('titulacion_theme', theme);
  }

  onSubmit(): void {
    if (!this.usernameOrEmail || !this.password) {
      this.errorMessage.set('Por favor, ingresa tu documento de identidad y contraseña.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService
      .login({
        usernameOrEmail: this.usernameOrEmail,
        password: this.password,
        systemCode: this.systemCode,
      })
      .subscribe({
        next: () => {
          this.isLoading.set(false);
          this.router.navigate(['/dashboard']);
        },
        error: (err) => {
          this.isLoading.set(false);
          const msg = err.error?.message || 'Credenciales de acceso inválidas.';
          this.errorMessage.set(msg);
        },
      });
  }
}
