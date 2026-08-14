import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

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
  isDarkMode = signal(false);
  showPassword = signal(false);

  toggleShowPassword(): void {
    this.showPassword.set(!this.showPassword());
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
      this.errorMessage.set('Por favor, ingresa tu usuario y contraseña.');
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
