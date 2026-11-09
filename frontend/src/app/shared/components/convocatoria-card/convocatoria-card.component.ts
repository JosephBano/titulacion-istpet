import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type EstadoPuntoTipo = 'info' | 'warning' | 'success' | 'danger';

@Component({
  selector: 'app-convocatoria-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './convocatoria-card.component.html',
  styleUrls: ['./convocatoria-card.component.css'],
})
export class ConvocatoriaCardComponent {
  simplificado = input<boolean>(false);
  estaAbierta = input<boolean>(false);
  detalle = input<string>('Período Ordinario de Titulación');
  mensaje = input<string>('Consulte las fechas de postulación e inicio de corte.');
  diasRestantes = input<number | null>(null);

  // Punto y estado del proceso del estudiante
  puntoTitulo = input<string | null>(null);
  puntoDescripcion = input<string | null>(null);
  puntoTipo = input<EstadoPuntoTipo>('info');
}
