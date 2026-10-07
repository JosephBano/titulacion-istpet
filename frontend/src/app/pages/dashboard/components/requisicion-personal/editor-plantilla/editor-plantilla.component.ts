import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  OnDestroy,
  inject,
  signal,
  effect,
  output,
  input,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { PlantillaRequisicionStore } from '../../../../../application/complexivo/plantilla-requisicion.store';
import {
  DatosFormularioRequisicion,
  PlantillaRequisicionDefinicion,
} from '../../../../../domain/models/plantilla-requisicion.model';
import {
  crearRequisicionPdf,
  RecursosPdfPlantilla,
} from '../../../../../shared/utils/requisicion-personal-pdf';

type PestanaEditor = 'contenido' | 'apariencia' | 'valores' | 'firmas';

@Component({
  selector: 'app-editor-plantilla',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './editor-plantilla.component.html',
  styleUrls: ['../../complexivo-ui.css', './editor-plantilla.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EditorPlantillaComponent implements OnInit, OnDestroy {
  readonly store = inject(PlantillaRequisicionStore);
  private readonly sanitizer = inject(DomSanitizer);

  readonly cerrar = output<void>();
  readonly guardado = output<void>();
  readonly datosEjemplo = input<Partial<DatosFormularioRequisicion>>({});

  readonly pestanaActiva = signal<PestanaEditor>('contenido');
  readonly pdfPreviewUrl = signal<SafeResourceUrl | null>(null);
  readonly generandoPdf = signal<boolean>(false);
  readonly subiendoLogo = signal<boolean>(false);
  readonly subiendoFondo = signal<boolean>(false);
  readonly alertaExito = signal<string | null>(null);

  private rawObjectUrl: string | null = null;
  private debounceTimer: ReturnType<typeof setTimeout> | null = null;
  private renderVersion = 0;
  private recursosCache: RecursosPdfPlantilla = {};

  constructor() {
    effect(() => {
      const borrador = this.store.borrador();
      if (borrador) {
        this.programarActualizacionPdf(borrador);
      }
    });
  }

  ngOnInit(): void {
    if (!this.store.borrador()) {
      this.store.iniciarEdicion();
    }
    const borradorActual = this.store.borrador();
    if (borradorActual) {
      void this.cargarRecursosYRenderizar(borradorActual);
    }
  }

  ngOnDestroy(): void {
    if (this.debounceTimer) {
      clearTimeout(this.debounceTimer);
    }
    if (this.rawObjectUrl) {
      URL.revokeObjectURL(this.rawObjectUrl);
      this.rawObjectUrl = null;
    }
  }

  cambiarPestana(p: PestanaEditor): void {
    this.pestanaActiva.set(p);
  }

  get borrador(): PlantillaRequisicionDefinicion | null {
    return this.store.borrador();
  }

  notificarCambio(): void {
    const actual = this.store.borrador();
    if (actual) {
      this.store.actualizarBorrador({ ...actual });
    }
  }

  restaurarBase(): void {
    this.store.restaurarBase();
    this.recursosCache = {};
  }

  cancelar(): void {
    this.store.cancelarEdicion();
    this.cerrar.emit();
  }

  async guardar(): Promise<void> {
    const exito = await this.store.guardarPlantilla();
    if (exito) {
      this.alertaExito.set('Plantilla institucional guardada exitosamente.');
      this.guardado.emit();
      setTimeout(() => {
        this.alertaExito.set(null);
        this.cerrar.emit();
      }, 1200);
    }
  }

  async onArchivoLogoSeleccionado(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    const file = input.files[0];
    this.subiendoLogo.set(true);

    try {
      const adjunto = await this.store.subirImagen(file);
      const actual = this.store.borrador();
      if (actual) {
        actual.documento.logo.idAdjuntosImagenes = adjunto.idAdjuntosImagenes;
        this.store.actualizarBorrador({ ...actual });

        // Cargar data URL para el render del PDF
        const reader = new FileReader();
        reader.onload = () => {
          this.recursosCache.logoDataUrl = reader.result as string;
          this.programarActualizacionPdf(actual, 0);
        };
        reader.readAsDataURL(file);
      }
    } catch (err: unknown) {
      const mensaje = err instanceof Error ? err.message : 'Error al subir logotipo.';
      this.store.error.set(mensaje);
    } finally {
      this.subiendoLogo.set(false);
      input.value = '';
    }
  }

  quitarLogo(): void {
    const actual = this.store.borrador();
    if (actual) {
      actual.documento.logo.idAdjuntosImagenes = null;
      delete this.recursosCache.logoDataUrl;
      this.store.actualizarBorrador({ ...actual });
    }
  }

  async onArchivoFondoSeleccionado(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    const file = input.files[0];
    this.subiendoFondo.set(true);

    try {
      const adjunto = await this.store.subirImagen(file);
      const actual = this.store.borrador();
      if (actual) {
        actual.documento.fondo.idAdjuntosImagenes = adjunto.idAdjuntosImagenes;
        this.store.actualizarBorrador({ ...actual });

        const reader = new FileReader();
        reader.onload = () => {
          this.recursosCache.fondoDataUrl = reader.result as string;
          this.programarActualizacionPdf(actual, 0);
        };
        reader.readAsDataURL(file);
      }
    } catch (err: unknown) {
      const mensaje = err instanceof Error ? err.message : 'Error al subir fondo.';
      this.store.error.set(mensaje);
    } finally {
      this.subiendoFondo.set(false);
      input.value = '';
    }
  }

  quitarFondo(): void {
    const actual = this.store.borrador();
    if (actual) {
      actual.documento.fondo.idAdjuntosImagenes = null;
      delete this.recursosCache.fondoDataUrl;
      this.store.actualizarBorrador({ ...actual });
    }
  }

  private async cargarRecursosYRenderizar(def: PlantillaRequisicionDefinicion): Promise<void> {
    if (def.documento.logo.idAdjuntosImagenes && !this.recursosCache.logoDataUrl) {
      try {
        const blob = await this.store.obtenerImagenBlob(def.documento.logo.idAdjuntosImagenes);
        this.recursosCache.logoDataUrl = await this.blobToDataUrl(blob);
      } catch {
        // Fallback silencioso si la imagen aún no está cargada
      }
    }
    if (def.documento.fondo.idAdjuntosImagenes && !this.recursosCache.fondoDataUrl) {
      try {
        const blob = await this.store.obtenerImagenBlob(def.documento.fondo.idAdjuntosImagenes);
        this.recursosCache.fondoDataUrl = await this.blobToDataUrl(blob);
      } catch {
        // Fallback silencioso
      }
    }
    this.programarActualizacionPdf(def, 0);
  }

  private blobToDataUrl(blob: Blob): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = reject;
      reader.readAsDataURL(blob);
    });
  }

  private programarActualizacionPdf(
    definicion: PlantillaRequisicionDefinicion,
    esperaMs = 300,
  ): void {
    if (this.debounceTimer) {
      clearTimeout(this.debounceTimer);
    }

    const versionActual = ++this.renderVersion;
    this.generandoPdf.set(true);

    this.debounceTimer = setTimeout(async () => {
      try {
        const datos = this.datosEjemplo();
        const pdf = await crearRequisicionPdf(definicion, datos, this.recursosCache);

        // Cancelar si ya hay una versión de render más reciente
        if (versionActual !== this.renderVersion) return;

        const blob = pdf.output('blob');
        const url = URL.createObjectURL(blob);

        if (this.rawObjectUrl) {
          URL.revokeObjectURL(this.rawObjectUrl);
        }
        this.rawObjectUrl = url;

        this.pdfPreviewUrl.set(this.sanitizer.bypassSecurityTrustResourceUrl(url));
      } catch (err: unknown) {
        console.error('Error generando vista previa PDF:', err);
      } finally {
        if (versionActual === this.renderVersion) {
          this.generandoPdf.set(false);
        }
      }
    }, esperaMs);
  }
}
