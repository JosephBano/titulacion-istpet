import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-kpi-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './kpi-card.component.html',
  styleUrls: ['./kpi-card.component.css'],
})
export class KpiCardComponent {
  eyebrow = input.required<string>();
  value = input.required<string | number>();
  subtext = input<string>('');
  valueFontSize = input<string>('1.75rem');
}
