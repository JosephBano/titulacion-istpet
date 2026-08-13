import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { EstudiantesStore } from '../../application/estudiantes/estudiantes.store';
import { nombreCompleto } from '../../domain/models/estudiante.model';

@Component({
  selector: 'app-estudiantes-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './estudiantes-page.html',
  styleUrl: './estudiantes-page.scss',
})
export class EstudiantesPage {
  protected readonly store = inject(EstudiantesStore);
  protected readonly nombreCompleto = nombreCompleto;

  constructor() {
    void this.store.cargar();
  }

  protected alFiltrar(evento: Event): void {
    this.store.filtrar((evento.target as HTMLInputElement).value);
  }
}
