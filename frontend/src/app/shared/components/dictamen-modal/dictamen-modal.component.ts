import { Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import {
  DictamenPostulacionRequest,
  PostulacionDetalle,
  PostulacionRequisitoDetalle,
} from '../../../core/models/titulacion.models';
import { TitulacionService } from '../../../core/services/titulacion.service';

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
  private readonly titulacionService = inject(TitulacionService);
  private readonly sanitizer = inject(DomSanitizer);

  data = input<DictamenModalData | null>(null);

  modalClose = output<void>();
  confirm = output<DictamenPostulacionRequest>();

  observacionesTexto = signal<string>('');
  cargandoDetalle = signal<boolean>(false);
  detallePostulacion = signal<PostulacionDetalle | null>(null);

  // Archivo seleccionado para previsualización en vivo (PDF / Imagen)
  requisitoSeleccionadoParaVisor = signal<PostulacionRequisitoDetalle | null>(null);

  getArchivoUrl(ruta?: string | null): string {
    if (!ruta) return '';
    if (ruta.startsWith('http://') || ruta.startsWith('https://')) {
      return ruta;
    }
    const cleanPath = ruta.startsWith('/') ? ruta : '/' + ruta;
    return 'http://localhost:5192' + cleanPath;
  }

  safeVisorUrl = computed<SafeResourceUrl | null>(() => {
    const req = this.requisitoSeleccionadoParaVisor();
    if (!req || !req.rutaArchivoAdjunto) return null;
    const fullUrl = this.getArchivoUrl(req.rutaArchivoAdjunto);
    return this.sanitizer.bypassSecurityTrustResourceUrl(fullUrl);
  });

  esPdf = computed<boolean>(() => {
    const req = this.requisitoSeleccionadoParaVisor();
    if (!req) return false;
    const nombre = (req.nombreArchivoAdjunto || req.rutaArchivoAdjunto || '').toLowerCase();
    return nombre.endsWith('.pdf');
  });

  esImagen = computed<boolean>(() => {
    const req = this.requisitoSeleccionadoParaVisor();
    if (!req) return false;
    const nombre = (req.nombreArchivoAdjunto || req.rutaArchivoAdjunto || '').toLowerCase();
    return (
      nombre.endsWith('.png') ||
      nombre.endsWith('.jpg') ||
      nombre.endsWith('.jpeg') ||
      nombre.endsWith('.webp')
    );
  });

  constructor() {
    effect(() => {
      const d = this.data();
      if (d && d.idPostulacion) {
        this.observacionesTexto.set(d.observaciones || '');
        this.requisitoSeleccionadoParaVisor.set(null);
        this.cargarExpedienteCompleto(d.idPostulacion);
      } else {
        this.detallePostulacion.set(null);
        this.requisitoSeleccionadoParaVisor.set(null);
      }
    });
  }

  private cargarExpedienteCompleto(idPostulacion: number): void {
    this.cargandoDetalle.set(true);
    this.titulacionService.getPostulacionPorId(idPostulacion).subscribe({
      next: (detalle) => {
        this.detallePostulacion.set(detalle);
        this.cargandoDetalle.set(false);
        // Si hay algún requisito con adjunto, preseleccionar el primero para visualizarlo
        const primerAdjunto = detalle.requisitos.find((r) => r.esAdjunto && r.rutaArchivoAdjunto);
        if (primerAdjunto) {
          this.requisitoSeleccionadoParaVisor.set(primerAdjunto);
        }
      },
      error: () => {
        this.cargandoDetalle.set(false);
      },
    });
  }

  seleccionarParaVisor(req: PostulacionRequisitoDetalle): void {
    this.requisitoSeleccionadoParaVisor.set(req);
  }

  cerrarVisor(): void {
    this.requisitoSeleccionadoParaVisor.set(null);
  }

  onConfirm(): void {
    const d = this.data();
    if (!d) return;

    this.confirm.emit({
      idPostulacionAlumnos: d.idPostulacion,
      decision: d.decision,
      observaciones: this.observacionesTexto(),
    });
  }

  onClose(): void {
    this.modalClose.emit();
  }
}
