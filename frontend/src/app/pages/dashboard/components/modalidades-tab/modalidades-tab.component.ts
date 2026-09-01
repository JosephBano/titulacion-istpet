import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModalidadMaestra } from '../../../../core/models/titulacion.models';

@Component({
  selector: 'app-modalidades-tab',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './modalidades-tab.component.html',
  styleUrls: ['./modalidades-tab.component.css'],
})
export class ModalidadesTabComponent {
  modalidades = input<ModalidadMaestra[]>([]);
  loading = input<boolean>(false);

  nuevaModalidad = output<void>();
  abrirMatriz = output<ModalidadMaestra>();
  toggleEstado = output<ModalidadMaestra>();
}
