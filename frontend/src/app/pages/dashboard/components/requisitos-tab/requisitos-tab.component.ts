import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RequisitoMaestro } from '../../../../core/models/titulacion.models';

@Component({
  selector: 'app-requisitos-tab',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './requisitos-tab.component.html',
  styleUrls: ['./requisitos-tab.component.css'],
})
export class RequisitosTabComponent {
  requisitos = input<RequisitoMaestro[]>([]);
  loading = input<boolean>(false);

  nuevoRequisito = output<void>();
  toggleEstado = output<RequisitoMaestro>();
}
