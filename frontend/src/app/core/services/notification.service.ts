import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private readonly snackBar = inject(MatSnackBar);

  private readonly defaultConfig = {
    duration: 4000,
    horizontalPosition: 'center' as const,
    verticalPosition: 'bottom' as const,
  };

  success(texto: string): void {
    this.snackBar.open(texto, 'Cerrar', {
      ...this.defaultConfig,
      panelClass: ['istpet-snackbar', 'istpet-snackbar--success'],
    });
  }

  error(texto: string): void {
    this.snackBar.open(texto, 'Cerrar', {
      ...this.defaultConfig,
      duration: 6000,
      panelClass: ['istpet-snackbar', 'istpet-snackbar--error'],
    });
  }

  info(texto: string): void {
    this.snackBar.open(texto, 'Cerrar', {
      ...this.defaultConfig,
      panelClass: ['istpet-snackbar', 'istpet-snackbar--info'],
    });
  }
}
