import { httpResource } from '@angular/common/http';
import { Component, DestroyRef, computed, effect, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Analytics } from '../../../../shared/analytics/analytics';
import { CoordinatesDto } from '../../../../shared/api/coordinates-dto';
import { ExplorerPositioning } from '../../../../shared/explorer-positioning/explorer-positioning';
import {
  GeographicMap,
  MapCircle,
  MapCoordinate,
  MapPin,
} from '../../../../shared/geographic-map/geographic-map';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourExecutionDto, UnlockedKeyPointDto } from '../../api/exploration-api-types';
import { TourExecuting } from '../tour-executing';

// The distance at which the server counts a key point as reached.
const REACH_DISTANCE_METERS = 50;

// The session and the unlocked secrets are read again on every check, and every read brings
// new objects even when nothing changed. The map redraws, fits its view and asks for a route
// on every new value. A computed string is equal to the previous one when its text is the
// same, so the map inputs are built from such strings and change only when a location does.
function toKey(location: MapCoordinate): string {
  return `${location.latitude},${location.longitude}`;
}

function fromKey(key: string): MapCoordinate {
  const [latitude, longitude] = key.split(',').map(Number);
  return { latitude, longitude };
}

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
  private readonly analytics = inject(Analytics);

  // The tour id from the route /exploration/execution/:tourId.
  readonly tourId = input.required<string>();

  // The latest session that is active or completed. The server answers 204 with no body when
  // there is no session or the latest one was abandoned; the value is then null.
  protected readonly session = httpResource<TourExecutionDto | null>(
    () => `/api/exploration/tour-executions/current/${this.tourId()}`,
  );
  protected readonly sessionError = computed(() =>
    this.session.error()
      ? serverMessage(this.session.error(), 'The tour could not be loaded.')
      : null,
  );
  protected readonly isActive = computed(
    () => this.session.hasValue() && this.session.value()?.status === 'Active',
  );
  protected readonly isCompleted = computed(
    () => this.session.hasValue() && this.session.value()?.status === 'Completed',
  );
  // The key point reached last in the active session, with its secret.
  protected readonly reachedKeyPoint = computed(() =>
    this.isActive() ? (this.session.value()?.lastReachedKeyPoint ?? null) : null,
  );

  // The key points of this tour the explorer completed in any of their sessions.
  protected readonly unlockedSecrets = httpResource<UnlockedKeyPointDto[]>(
    () => `/api/exploration/tour-executions/unlocked-secrets/${this.tourId()}`,
  );
  protected readonly unlockedError = computed(() =>
    this.unlockedSecrets.error()
      ? serverMessage(this.unlockedSecrets.error(), 'The unlocked secrets could not be loaded.')
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
  // During an active session the check needs a position, and there is none yet.
  protected readonly missingPosition = computed(
    () => this.isActive() && this.position.hasValue() && this.position.value() === null,
  );
  protected readonly moving = signal(false);
  protected readonly moveError = signal<string | null>(null);

  // The next key point's location, while a session has one.
  private readonly nextLocationKey = computed(() => {
    const location = this.session.hasValue() ? this.session.value()?.nextKeyPoint?.location : null;
    return location ? toKey(location) : null;
  });
  private readonly nextLocation = computed<MapCoordinate | null>(() => {
    const key = this.nextLocationKey();
    return key === null ? null : fromKey(key);
  });

  // The locations of the completed key points, in the order of the unlocked list.
  private readonly completedLocationsKey = computed(() =>
    (this.unlockedSecrets.hasValue() ? this.unlockedSecrets.value() : [])
      .map((keyPoint) => toKey(keyPoint.location))
      .join(';'),
  );

  // The next key point as a pin without an id, so a click on it moves the explorer there.
  // Every completed key point is a pin whose id is its index in the unlocked list.
  protected readonly pins = computed<MapPin[]>(() => {
    const key = this.completedLocationsKey();
    const completedPins =
      key === ''
        ? []
        : key.split(';').map((location, index) => ({ id: String(index), ...fromKey(location) }));
    const next = this.nextLocation();
    return next ? [next, ...completedPins] : completedPins;
  });

  // The circle around the next key point, as far as the server counts it as reached.
  protected readonly circle = computed<MapCircle | null>(() => {
    const next = this.nextLocation();
    return next ? { center: next, radiusMeters: REACH_DISTANCE_METERS } : null;
  });

  // The street route from the explorer to the next key point, while both exist.
  protected readonly route = computed<MapCoordinate[] | null>(() => {
    const next = this.nextLocation();
    const position = this.explorerPosition();
    return next && position ? [position, next] : null;
  });
  protected readonly routeFailed = signal(false);

  // The completed key point whose pin was clicked last, picked from the loaded unlocked list.
  protected readonly selectedId = signal<string | null>(null);
  protected readonly selectedKeyPoint = computed(() => {
    const id = this.selectedId();
    if (id === null || !this.unlockedSecrets.hasValue()) {
      return null;
    }
    return this.unlockedSecrets.value()[Number(id)] ?? null;
  });

  protected readonly starting = signal(false);
  protected readonly startError = signal<string | null>(null);

  protected readonly confirmingAbandon = signal(false);
  protected readonly abandoning = signal(false);
  protected readonly abandonError = signal<string | null>(null);

  private intervalId: number | null = null;
  // True once this page has shown the session as active. Completion is recorded only when an
  // active session turns completed here, so reopening an already completed tour sends nothing.
  private sawActiveSession = false;
  protected readonly checking = signal(false);
  protected readonly checkError = signal<string | null>(null);

  constructor() {
    // Leaving the page stops the check.
    inject(DestroyRef).onDestroy(() => this.stopChecking());

    // The check is a timer outside the component tree, driven by the session's status: it
    // starts when the page shows an active session (after Start or on opening the page) and
    // stops when the session is completed, abandoned or can no longer be read.
    effect(() => {
      if (this.isActive()) {
        this.sawActiveSession = true;
        this.startChecking();
      } else {
        this.stopChecking();
        // The server completes the session on a position report; the page learns it on reload.
        if (this.sawActiveSession && this.isCompleted()) {
          this.sawActiveSession = false;
          this.analytics.tourCompleted(this.tourId());
        }
      }
    });
  }

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
      this.analytics.tourStarted(this.tourId());
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

  private startChecking(): void {
    this.stopChecking();
    this.intervalId = setInterval(() => this.check(), 10_000);
  }

  private stopChecking(): void {
    if (this.intervalId !== null) {
      clearInterval(this.intervalId);
      this.intervalId = null;
    }
    // Without a running check there is nothing to retry.
    this.checkError.set(null);
  }

  private async check(): Promise<void> {
    const position = this.explorerPosition();
    // While the previous check is pending, or while there is no position, this one is skipped.
    if (this.checking() || position === null) {
      return;
    }

    this.checking.set(true);
    this.checkError.set(null);
    try {
      await this.tourExecuting.reportPosition(this.tourId(), position);
      this.session.reload();
      this.unlockedSecrets.reload();
    } catch {
      // A failure and a session that is no longer active are handled alike: the page reads the
      // session again, and the check stops if it is no longer active.
      this.checkError.set("Couldn't check your position, retrying...");
      this.session.reload();
    } finally {
      this.checking.set(false);
    }
  }
}
