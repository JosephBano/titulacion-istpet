import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import {
  CarrerasService,
  CarreraDto,
  EstudianteCarreraDto,
  ProfesorCarreraDto,
  UsuarioCarrerasResponseDto,
} from '../../core/services/carreras.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent implements OnInit {
  private authService = inject(AuthService);
  private carrerasService = inject(CarrerasService);
  private router = inject(Router);

  currentUser = this.authService.currentUser;
  currentYear = new Date().getFullYear();
  isDarkMode = signal(false);
  isSidebarCollapsed = signal(false);
  activeTab = signal('resumen');
  searchQuery = signal('');

  // Estado Multicarrera
  carrerasDisponibles = signal<any[]>([]);
  carreraSeleccionada = signal<any | null>(null);
  carrerasCargando = signal<boolean>(true);

  userRolesFormatted(): string {
    if (this.isAdmin()) return 'Administrador';
    if (this.isDocente()) return 'Docente';
    return 'Estudiante';
  }

  isAdmin(): boolean {
    return this.hasRole('ADMINISTRADOR') || this.hasRole('ADMIN_SIST');
  }

  isDocente(): boolean {
    return (this.hasRole('DOCENTE') || this.hasRole('PROFESOR')) && !this.isAdmin();
  }

  isEstudiante(): boolean {
    return (
      (this.hasRole('ESTUDIANTE') || this.hasRole('ALUMNO')) && !this.isAdmin() && !this.isDocente()
    );
  }

  hasRole(roleCode: string): boolean {
    return this.authService.hasRole(roleCode);
  }

  hasAnyRole(roles: string[]): boolean {
    return this.authService.hasAnyRole(roles);
  }

  hasPermission(moduleName: string, operationName: string): boolean {
    return this.authService.hasPermission(moduleName, operationName);
  }

  ngOnInit(): void {
    const savedTheme = localStorage.getItem('titan_theme') || 'light';
    const isDark = savedTheme === 'dark';
    this.isDarkMode.set(isDark);
    document.documentElement.setAttribute('data-theme', isDark ? 'dark' : 'light');

    if (this.isEstudiante()) {
      this.activeTab.set('postulaciones');
    } else if (this.isDocente()) {
      this.activeTab.set('evaluacion');
    } else {
      this.activeTab.set('resumen');
    }

    this.cargarCarrerasUsuario();
  }

  cargarCarrerasUsuario(): void {
    this.carrerasCargando.set(true);
    this.carrerasService.getMisCarreras().subscribe({
      next: (data: UsuarioCarrerasResponseDto) => {
        let lista: any[] = [];
        if (this.isEstudiante()) {
          lista = data.carrerasEstudiante || [];
        } else if (this.isDocente()) {
          lista = data.carrerasDocente || [];
        } else {
          lista = [...(data.carrerasEstudiante || []), ...(data.carrerasDocente || [])];
        }

        this.carrerasDisponibles.set(lista);
        if (lista.length > 0) {
          this.carreraSeleccionada.set(lista[0]);
        }
        this.carrerasCargando.set(false);
      },
      error: (err) => {
        console.error('Error al cargar carreras del usuario:', err);
        this.carrerasCargando.set(false);
      },
    });
  }

  onCarreraChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    const idCarrera = parseInt(select.value, 10);
    const seleccionada = this.carrerasDisponibles().find((c) => c.idCarrera === idCarrera);
    if (seleccionada) {
      this.carreraSeleccionada.set(seleccionada);
    }
  }

  toggleTheme(): void {
    const nextTheme = !this.isDarkMode();
    this.isDarkMode.set(nextTheme);
    const themeStr = nextTheme ? 'dark' : 'light';
    document.documentElement.setAttribute('data-theme', themeStr);
    localStorage.setItem('titan_theme', themeStr);
  }

  toggleSidebar(): void {
    this.isSidebarCollapsed.set(!this.isSidebarCollapsed());
  }

  setActiveTab(tab: string): void {
    this.activeTab.set(tab);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
