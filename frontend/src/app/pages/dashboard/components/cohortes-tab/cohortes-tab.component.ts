import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ConvocatoriaDetalle,
  ConvocatoriaResumen,
} from '../../../../core/models/titulacion.models';
import { ConvocatoriaCardComponent } from '../../../../shared/components/convocatoria-card/convocatoria-card.component';

@Component({
  selector: 'app-cohortes-tab',
  standalone: true,
  imports: [CommonModule, ConvocatoriaCardComponent],
  templateUrl: './cohortes-tab.component.html',
  styleUrls: ['./cohortes-tab.component.css'],
})
export class CohortesTabComponent {
  convocatoriaActiva = input<ConvocatoriaDetalle | null>(null);
  convocatoriasLista = input<ConvocatoriaResumen[]>([]);
  loading = input<boolean>(false);
  periodoNombreAmigable = input<string>('');

  aperturarNuevo = output<void>();
}
