import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-convocatoria-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './convocatoria-card.component.html',
  styleUrls: ['./convocatoria-card.component.css'],
})
export class ConvocatoriaCardComponent {
  estaAbierta = input<boolean>(false);
  detalle = input<string>('Período Ordinario de Titulación');
  mensaje = input<string>('Consulte las fechas de postulación e inicio de corte.');
  diasRestantes = input<number | null>(null);
}
