import { Component, HostListener, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConfirmDetail } from '../../../core/services/confirm.service';

export type ConfirmVariant = 'primary' | 'success' | 'danger' | 'warning' | 'info';
export type ConfirmIconType = 'help' | 'upload' | 'check' | 'alert' | 'trash' | 'x' | 'file';

@Component({
  selector: 'app-confirm-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './confirm-modal.component.html',
  styleUrls: ['./confirm-modal.component.css'],
})
export class ConfirmModalComponent {
  isOpen = input<boolean>(false);
  title = input<string>('¿Confirmar acción?');
  message = input<string>('');
  confirmText = input<string | undefined>('Confirmar');
  cancelText = input<string | undefined>('Cancelar');
  variant = input<ConfirmVariant | undefined>('primary');
  icon = input<ConfirmIconType | undefined>('help');
  documentName = input<string | null | undefined>(null);
  details = input<ConfirmDetail[] | null | undefined>(null);
  loading = input<boolean>(false);

  confirm = output<void>();
  cancelConfirm = output<void>();
  isOpenChange = output<boolean>();

  @HostListener('document:keydown.escape')
  handleEscape(): void {
    if (this.isOpen() && !this.loading()) {
      this.onCancel();
    }
  }

  onConfirm(): void {
    if (!this.loading()) {
      this.confirm.emit();
      this.isOpenChange.emit(false);
    }
  }

  onCancel(): void {
    if (!this.loading()) {
      this.cancelConfirm.emit();
      this.isOpenChange.emit(false);
    }
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget && !this.loading()) {
      this.onCancel();
    }
  }
}
