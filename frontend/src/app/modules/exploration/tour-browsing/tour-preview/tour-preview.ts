import { httpResource } from '@angular/common/http';
import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { GeographicMap, MapPin } from '../../../../shared/geographic-map/geographic-map';
import { TourPreviewDto } from '../../api/exploration-api-types';

@Component({
  imports: [GeographicMap, RouterLink],
  selector: 'app-tour-preview',
  styleUrl: './tour-preview.scss',
  templateUrl: './tour-preview.html',
})
export class TourPreview {
  // The tour id from the route /exploration/tours/:id.
  readonly id = input.required<string>();

  protected readonly tour = httpResource<TourPreviewDto>(
    () => `/api/exploration/tours/${this.id()}/preview`,
  );

  // Only the key points visible without purchase, as pins without a line between them.
  protected readonly pins = computed<MapPin[]>(() =>
    this.tour.hasValue() ? this.tour.value().keyPoints.map((point) => point.location) : [],
  );
}
