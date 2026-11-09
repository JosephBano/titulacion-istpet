import { Component, effect, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DictamenPostulacionRequest } from '../../../core/models/titulacion.models';

export interface DictamenModalData {
  idPostulacion: number;
  decision: 'APROBAR' | 'OBSERVAR' | 'RECHAZAR';
  observaciones?: string;
  nombreAlumno?: string;
  cedulaAlumno?: string;
}

@Component({
  selector: 'app-dictamen-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dictamen-modal.component.html',
  styleUrls: ['./dictamen-modal.component.css'],
})
export class DictamenModalComponent {
  data = input<DictamenModalData | null>(null);

  modalClose = output<void>();
  confirm = output<DictamenPostulacionRequest>();

  observacionesTexto = signal<string>('');

  constructor() {
    effect(() => {
      const d = this.data();
      this.observacionesTexto.set(d?.observaciones || '');
    });
  }

  onConfirm(): void {
    const d = this.data();
    if (!d) return;

    if (d.decision === 'RECHAZAR' && !this.observacionesTexto().trim()) {
      return;
    }

    this.confirm.emit({
      idPostulacionAlumnos: d.idPostulacion,
      decision: d.decision,
      observaciones: this.observacionesTexto().trim(),
    });
  }

  onClose(): void {
    this.modalClose.emit();
  }
}
