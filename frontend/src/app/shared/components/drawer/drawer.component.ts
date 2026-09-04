import { Component, HostListener, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

export type DrawerSize = 'sm' | 'md' | 'lg' | 'xl' | 'full';

/**
 * DrawerComponent — Panel lateral deslizante (Offcanvas / Slide-over)
 * Nace desde el lado derecho de la pantalla con soporte para advertencia
 * al cerrar en caso de tener datos no guardados (hasDirtyData).
 */
@Component({
  selector: 'app-drawer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './drawer.component.html',
  styleUrls: ['./drawer.component.css'],
})
export class DrawerComponent {
  /** Estado de apertura del drawer */
  isOpen = input<boolean>(false);

  /** Título principal del panel */
  title = input<string>('');

  /** Subtítulo o texto descriptivo breve */
  subtitle = input<string>('');

  /** Categoría o texto superior pequeño */
  eyebrow = input<string>('');

  /** Ancho del panel: 'sm' (380px), 'md' (480px), 'lg' (640px), 'xl' (820px), 'full' (100vw) */
  size = input<DrawerSize>('md');

  /**
   * Indica si el contenido del drawer tiene cambios pendientes o datos sin guardar.
   * Si es true, al intentar cerrar se mostrará el modal de advertencia de pérdida de datos.
   */
  hasDirtyData = input<boolean>(false);

  /** Si hacer clic en el backdrop oscuro solicita cerrar el panel */
  closeOnBackdrop = input<boolean>(true);

  /** Si presionar la tecla Escape solicita cerrar el panel */
  closeOnEscape = input<boolean>(true);

  /** Título del modal de confirmación de descarte */
  discardModalTitle = input<string>('¿Deseas descartar los cambios?');

  /** Mensaje explicativo en el modal de confirmación */
  discardModalMessage = input<string>(
    'Tienes información o cambios pendientes en este formulario. Si cierras el panel ahora, todos los datos no guardados se perderán.',
  );

  /** Emite cuando el drawer se cierra formalmente */
  closed = output<void>();

  /** Emite el nuevo valor booleano para enlace bidireccional [(isOpen)] */
  isOpenChange = output<boolean>();

  /** Emite cuando el usuario confirma expresamente descartar los datos no guardados */
  discardConfirmed = output<void>();

  /** Estado interno para desplegar el modal de confirmación de descarte */
  showDiscardModal = signal<boolean>(false);

  /** Manejo del atajo de teclado Escape */
  @HostListener('window:keydown.escape', ['$event'])
  handleEscape(event: Event): void {
    if (!this.isOpen()) return;

    if (this.showDiscardModal()) {
      this.cancelDiscard();
      event.stopPropagation();
      return;
    }

    if (this.closeOnEscape()) {
      this.requestClose();
      event.stopPropagation();
    }
  }

  /**
   * Solicita el cierre del panel.
   * Si hay datos sin guardar (hasDirtyData), abre el modal de advertencia.
   * Si no, procede a cerrarlo directamente.
   */
  requestClose(): void {
    if (this.hasDirtyData()) {
      this.showDiscardModal.set(true);
    } else {
      this.forceClose();
    }
  }

  /** Cancela el cierre y regresa a continuar editando */
  cancelDiscard(): void {
    this.showDiscardModal.set(false);
  }

  /** Confirma el descarte y cierra el drawer */
  confirmDiscard(): void {
    this.showDiscardModal.set(false);
    this.discardConfirmed.emit();
    this.forceClose();
  }

  /** Ejecuta el cierre definitivo */
  private forceClose(): void {
    this.isOpenChange.emit(false);
    this.closed.emit();
  }

  /** Clic sobre el fondo oscuro */
  onBackdropClick(): void {
    if (this.closeOnBackdrop()) {
      this.requestClose();
    }
  }
}
