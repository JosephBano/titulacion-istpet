import { Component, input, output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  PortalEstudiante,
  PostulacionDetalle,
  PostulacionRequisitoDetalle,
} from '../../../../core/models/titulacion.models';
import { NotificationService } from '../../../../core/services/notification.service';
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
  private readonly notification = inject(NotificationService);
  readonly MAX_FILE_SIZE_BYTES = 2 * 1024 * 1024; // 2 MB

  portal = input<PortalEstudiante | null>(null);
  modalidadSeleccionada = input<number | null>(null);
  postulando = input<boolean>(false);
  subiendoRequisitoId = input<number | null>(null);
  periodoNombreAmigable = input<string>('');

  modalidadSelect = output<number>();
  enviarPostulacion = output<void>();
  subirDocumento = output<{ req: PostulacionRequisitoDetalle; file: File }>();

  onFileChange(req: PostulacionRequisitoDetalle, event: Event): void {
    const inputEl = event.target as HTMLInputElement;
    if (inputEl && inputEl.files && inputEl.files.length > 0) {
      const file = inputEl.files[0];

      // Validación 1: Formato estrictamente PDF
      const esPdf =
        file.type === 'application/pdf' || file.name.toLowerCase().endsWith('.pdf');
      if (!esPdf) {
        this.notification.error(
          `Formato inválido: El documento "${file.name}" debe ser obligatoriamente un archivo PDF (.pdf).`,
        );
        inputEl.value = '';
        return;
      }

      // Validación 2: Límite estricto de tamaño 2 MB
      if (file.size > this.MAX_FILE_SIZE_BYTES) {
        const tamanoMb = (file.size / (1024 * 1024)).toFixed(2);
        this.notification.error(
          `El archivo supera el límite: "${file.name}" pesa ${tamanoMb} MB. El tamaño máximo permitido es de 2 MB.`,
        );
        inputEl.value = '';
        return;
      }

      this.subirDocumento.emit({ req, file });
      inputEl.value = '';
    }
  }

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
    return requisitos.filter((r) => r.valorBool === true || r.estadoValidacion === 'APROBADO')
      .length;
  }

  esRequisitoAprobado(req: PostulacionRequisitoDetalle): boolean {
    const est = (req.estadoValidacion || '').toUpperCase();
    return est === 'APROBADO' || (req.valorBool === true && est !== 'RECHAZADO' && est !== 'OBSERVADO');
  }

  esRequisitoRechazado(req: PostulacionRequisitoDetalle): boolean {
    if (this.esRequisitoAprobado(req)) return false;
    const est = (req.estadoValidacion || '').toUpperCase();
    return est === 'RECHAZADO' || est === 'OBSERVADO' || est.includes('RECHAZ');
  }

  esRequisitoPendiente(req: PostulacionRequisitoDetalle): boolean {
    return !this.esRequisitoAprobado(req) && !this.esRequisitoRechazado(req);
  }

  esPostulacionRechazada(estado?: string): boolean {
    if (!estado) return false;
    const est = estado.toUpperCase();
    return est.includes('RECHAZ') || est.includes('NEGAD');
  }

  requisitosFaltantes(requisitos?: PostulacionRequisitoDetalle[]): PostulacionRequisitoDetalle[] {
    if (!requisitos) return [];
    return requisitos.filter((r) => !this.esRequisitoAprobado(r));
  }

  tieneRequisitosFinalesPendientes(requisitos?: PostulacionRequisitoDetalle[]): boolean {
    if (!requisitos) return false;
    return requisitos.some((r) => r.esRequisitoFinal && !this.esRequisitoAprobado(r));
  }

  obtenerObservacionPostulacion(postulacion: PostulacionDetalle | null): string | null {
    if (!postulacion) return null;
    if (
      postulacion.observacionDictamen &&
      typeof postulacion.observacionDictamen === 'string' &&
      postulacion.observacionDictamen.trim().length > 0
    ) {
      return postulacion.observacionDictamen.trim();
    }
    if (postulacion.requisitos && Array.isArray(postulacion.requisitos)) {
      const conObs = postulacion.requisitos.find(
        (r) =>
          r.observaciones &&
          typeof r.observaciones === 'string' &&
          r.observaciones.trim().length > 0,
      );
      if (conObs && conObs.observaciones) {
        return conObs.observaciones.trim();
      }
    }
    return null;
  }
}
