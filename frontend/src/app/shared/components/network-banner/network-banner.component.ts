import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-network-banner',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './network-banner.component.html',
  styleUrls: ['./network-banner.component.css'],
})
export class NetworkBannerComponent {
  isOnline = input<boolean>(true);
  isLowBandwidth = input<boolean>(false);
  connectionType = input<string>('4g');
}
