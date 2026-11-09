import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ConfirmModalComponent } from './shared/components/confirm-modal/confirm-modal.component';
import { ConfirmService } from './core/services/confirm.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ConfirmModalComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  protected readonly title = signal('titulacion-istpet-web');
  protected readonly confirmService = inject(ConfirmService);
}
