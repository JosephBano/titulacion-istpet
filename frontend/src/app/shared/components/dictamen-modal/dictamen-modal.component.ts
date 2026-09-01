import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { DictamenPostulacionRequest } from '../../../core/models/titulacion.models';

export interface DictamenModalData {
  idPostulacion: number;
  decision: 'APROBAR' | 'OBSERVAR' | 'RECHAZAR';
  observaciones: string;
}

@Component({
  selector: 'app-dictamen-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule],
  templateUrl: './dictamen-modal.component.html',
  styleUrls: ['./dictamen-modal.component.css'],
})
export class DictamenModalComponent {
  data = input<DictamenModalData | null>(null);

  modalClose = output<void>();
  confirm = output<DictamenPostulacionRequest>();

  observacionesTexto = signal<string>('');

  onConfirm(): void {
    const d = this.data();
    if (!d) return;

    this.confirm.emit({
      idPostulacionAlumnos: d.idPostulacion,
      decision: d.decision,
      observaciones: this.observacionesTexto() || d.observaciones,
    });
  }

  onClose(): void {
    this.modalClose.emit();
  }
}
