import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-stepper',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './stepper.component.html',
  styleUrls: ['./stepper.component.css'],
})
export class StepperComponent {
  estadoNombre = input<string>('Pendiente de Postulación');
  etapaActual = input<number>(1);
  modalidadNombre = input<string>('Por seleccionar');
  tienePostulacion = input<boolean>(false);

  esRechazado = computed(() => {
    const est = (this.estadoNombre() || '').toUpperCase();
    return est.includes('RECHAZ') || est.includes('NEGAD');
  });

  esEtapaAprobada = computed(() => !this.esRechazado() && this.etapaActual() >= 3);
}
