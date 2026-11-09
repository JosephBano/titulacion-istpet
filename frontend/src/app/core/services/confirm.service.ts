import { Injectable, signal } from '@angular/core';

export interface ConfirmDetail {
  label: string;
  value: string;
}

export interface ConfirmOptions {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  variant?: 'primary' | 'success' | 'danger' | 'warning' | 'info';
  icon?: 'help' | 'upload' | 'check' | 'alert' | 'trash' | 'x' | 'file';
  documentName?: string;
  details?: ConfirmDetail[];
}

export interface ConfirmState {
  isOpen: boolean;
  title: string;
  message: string;
  confirmText: string;
  cancelText: string;
  variant: 'primary' | 'success' | 'danger' | 'warning' | 'info';
  icon: 'help' | 'upload' | 'check' | 'alert' | 'trash' | 'x' | 'file';
  documentName?: string;
  details?: ConfirmDetail[];
  resolve: (value: boolean) => void;
}

@Injectable({
  providedIn: 'root',
})
export class ConfirmService {
  private readonly _state = signal<ConfirmState | null>(null);
  readonly state = this._state.asReadonly();

  confirm(options: ConfirmOptions): Promise<boolean> {
    return new Promise<boolean>((resolve) => {
      this._state.set({
        ...options,
        confirmText: options.confirmText ?? 'Confirmar',
        cancelText: options.cancelText ?? 'Cancelar',
        variant: options.variant ?? 'primary',
        icon: options.icon ?? 'help',
        isOpen: true,
        resolve,
      });
    });
  }

  handleConfirm(): void {
    const current = this._state();
    if (current) {
      current.resolve(true);
      this._state.set(null);
    }
  }

  handleCancel(): void {
    const current = this._state();
    if (current) {
      current.resolve(false);
      this._state.set(null);
    }
  }
}
