import { Component, inject } from '@angular/core';
import { Analytics } from '../../shared/analytics/analytics';

@Component({
  selector: 'app-privacy',
  styleUrl: './privacy.scss',
  templateUrl: './privacy.html',
})
export class Privacy {
  protected readonly analytics = inject(Analytics);
}
