import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  PortalEstudiante,
  PostulacionDetalle,
  PostulacionRequisitoDetalle,
} from '../../../../core/models/titulacion.models';
import { ConvocatoriaCardComponent } from '../../../../shared/components/convocatoria-card/convocatoria-card.component';
import { StepperComponent } from '../../../../shared/components/stepper/stepper.component';

@Component({
  selector: 'app-estudiante-proceso',
  standalone: true,
  imports: [CommonModule, ConvocatoriaCardComponent, StepperComponent],
  templateUrl: './estudiante-proceso.component.html',
  styleUrls: ['./estudiante-proceso.component.css'],
})
export class EstudianteProcesoComponent {
  portal = input<PortalEstudiante | null>(null);
  modalidadSeleccionada = input<number | null>(null);
  postulando = input<boolean>(false);
  periodoNombreAmigable = input<string>('');

  modalidadSelect = output<number>();
  enviarPostulacion = output<void>();

  obtenerEtapaStepper(estado?: string): number {
    if (!estado) return 1;
    const est = estado.toUpperCase();
    if (est.includes('REGISTRAD') || est.includes('POSTULAD')) return 1;
    if (est.includes('REVIS') || est.includes('PENDIENT') || est.includes('OBSERV')) return 2;
    if (est.includes('APROBAD') || est.includes('MODALIDAD')) return 3;
    if (est.includes('EVALUA') || est.includes('COMPLEXIVO') || est.includes('TUTOR')) return 4;
    if (est.includes('TITULAD') || est.includes('GRADUAD') || est.includes('ACTA')) return 5;
    return 2;
  }

  totalAprobados(requisitos?: PostulacionRequisitoDetalle[]): number {
    if (!requisitos) return 0;
    return requisitos.filter(r => r.valorBool === true || r.estadoValidacion === 'APROBADO').length;
  }

  esRequisitoAprobado(req: PostulacionRequisitoDetalle): boolean {
    return req.valorBool === true || req.estadoValidacion === 'APROBADO';
  }

  esPostulacionRechazada(estado?: string): boolean {
    if (!estado) return false;
    const est = estado.toUpperCase();
    return est.includes('RECHAZ') || est.includes('NEGAD');
  }

  obtenerObservacionPostulacion(postulacion: PostulacionDetalle | null): string | null {
    if (!postulacion) return null;
    if (postulacion.observacionDictamen && typeof postulacion.observacionDictamen === 'string' && postulacion.observacionDictamen.trim().length > 0) {
      return postulacion.observacionDictamen.trim();
    }
    if (postulacion.requisitos && Array.isArray(postulacion.requisitos)) {
      const conObs = postulacion.requisitos.find(
        (r) => r.observaciones && typeof r.observaciones === 'string' && r.observaciones.trim().length > 0
      );
      if (conObs && conObs.observaciones) {
        return conObs.observaciones.trim();
      }
    }
    return null;
  }
}
