import { Component, computed, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { UserPermissions } from '../../../core/models/auth.models';
import { CarreraUsuarioItem } from '../../../core/services/carreras.service';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [CommonModule, MatMenuModule, MatDividerModule],
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.css'],
})
export class TopbarComponent {
  // Estado (provisto por el contenedor — este componente es puramente presentacional)
  isDarkMode = input<boolean>(false);
  isSidebarCollapsed = input<boolean>(false);
  currentUser = input<UserPermissions | null>(null);
  userRolesFormatted = input<string>('');
  carrerasDisponibles = input<CarreraUsuarioItem[]>([]);
  carreraSeleccionada = input<CarreraUsuarioItem | null>(null);
  carrerasCargando = input<boolean>(false);

  // Eventos (la lógica de negocio vive en el contenedor)
  toggleSidebar = output<void>();
  toggleTheme = output<void>();
  carreraChange = output<number>();
  logout = output<void>();

  initial = computed(() => {
    const nombre = this.currentUser()?.nombre;
    return nombre ? nombre.charAt(0).toUpperCase() : 'U';
  });

  onCarreraSelect(event: Event): void {
    const select = event.target as HTMLSelectElement;
    const idCarrera = parseInt(select.value, 10);
    if (!Number.isNaN(idCarrera)) {
      this.carreraChange.emit(idCarrera);
    }
  }
}
