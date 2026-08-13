import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EstudiantesStore } from '../../application/estudiantes/estudiantes.store';
import { nombreCompleto } from '../../domain/models/estudiante.model';

@Component({
  selector: 'app-estudiantes-page',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './estudiantes-page.component.html',
  styleUrl: './estudiantes-page.component.scss',
})
export class EstudiantesPageComponent {
  protected readonly store = inject(EstudiantesStore);
  protected readonly nombreCompleto = nombreCompleto;

  constructor() {
    void this.store.cargar();
  }

  protected alFiltrar(evento: Event): void {
    this.store.filtrar((evento.target as HTMLInputElement).value);
  }
}
