import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CoordinatesDto } from '../../../../shared/api/coordinates-dto';
import { ExplorerPositioning } from '../../../../shared/explorer-positioning/explorer-positioning';
import {
  GeographicMap,
  MapCoordinate,
  MapPin,
} from '../../../../shared/geographic-map/geographic-map';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourExecutionDto } from '../../api/exploration-api-types';
import { TourExecuting } from '../tour-executing';

@Component({
  imports: [GeographicMap, RouterLink],
  selector: 'app-tour-execution',
  styleUrl: './tour-execution.scss',
  templateUrl: './tour-execution.html',
})
export class TourExecution {
  private readonly tourExecuting = inject(TourExecuting);
  private readonly explorerPositioning = inject(ExplorerPositioning);
  private readonly router = inject(Router);

  // The tour id from the route /exploration/execution/:tourId.
  readonly tourId = input.required<string>();

  // The server answers 204 with no body when there is no active session; the value is then null.
  protected readonly session = httpResource<TourExecutionDto | null>(
    () => `/api/exploration/tour-executions/active/${this.tourId()}`,
  );
  protected readonly sessionError = computed(() =>
    this.session.error()
      ? serverMessage(this.session.error(), 'The tour could not be loaded.')
      : null,
  );

  // The server answers 204 with no body when the explorer has no position yet; the value is then null.
  protected readonly position = httpResource<CoordinatesDto | null>(
    () => '/api/shared/positions/mine',
  );
  // While the read is pending or failed, there is no marker.
  protected readonly explorerPosition = computed(() =>
    this.position.hasValue() ? this.position.value() : null,
  );
  protected readonly loadError = computed(() =>
    this.position.error()
      ? serverMessage(this.position.error(), 'Your position could not be loaded.')
      : null,
  );
  protected readonly moving = signal(false);
  protected readonly moveError = signal<string | null>(null);

  // The next key point as a pin, while a session is active.
  protected readonly pins = computed<MapPin[]>(() => {
    const session = this.session.hasValue() ? this.session.value() : null;
    return session ? [session.nextKeyPoint.location] : [];
  });

  // The street route from the explorer to the next key point, while both exist.
  protected readonly route = computed<MapCoordinate[] | null>(() => {
    const session = this.session.hasValue() ? this.session.value() : null;
    const position = this.explorerPosition();
    return session && position ? [position, session.nextKeyPoint.location] : null;
  });
  protected readonly routeFailed = signal(false);

  protected readonly starting = signal(false);
  protected readonly startError = signal<string | null>(null);

  protected readonly confirmingAbandon = signal(false);
  protected readonly abandoning = signal(false);
  protected readonly abandonError = signal<string | null>(null);

  protected async onExplorerMoved(coordinates: MapCoordinate): Promise<void> {
    // A click while saving is ignored, so two saves are never sent at once.
    if (this.moving()) {
      return;
    }

    this.moving.set(true);
    this.moveError.set(null);
    try {
      await this.explorerPositioning.move(coordinates);
      this.position.reload();
    } catch (failure) {
      this.moveError.set(serverMessage(failure, 'Your position could not be saved.'));
    } finally {
      this.moving.set(false);
    }
  }

  protected async start(): Promise<void> {
    const position = this.explorerPosition();
    if (position === null) {
      return;
    }

    this.starting.set(true);
    this.startError.set(null);
    try {
      await this.tourExecuting.start(this.tourId(), position);
      this.session.reload();
    } catch (failure) {
      this.startError.set(serverMessage(failure, 'The tour could not be started.'));
    } finally {
      this.starting.set(false);
    }
  }

  protected askToAbandon(): void {
    this.abandonError.set(null);
    this.confirmingAbandon.set(true);
  }

  protected cancelAbandon(): void {
    this.abandonError.set(null);
    this.confirmingAbandon.set(false);
  }

  protected async abandon(): Promise<void> {
    this.abandoning.set(true);
    this.abandonError.set(null);
    try {
      await this.tourExecuting.abandon(this.tourId());
      await this.router.navigateByUrl('/exploration');
    } catch (failure) {
      this.abandonError.set(serverMessage(failure, 'The tour could not be abandoned.'));
    } finally {
      this.abandoning.set(false);
    }
  }
}
