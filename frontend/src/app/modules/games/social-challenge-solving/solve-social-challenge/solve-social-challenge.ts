import { httpResource } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CoordinatesDto } from '../../../../shared/api/coordinates-dto';
import { ExplorerPositioning } from '../../../../shared/explorer-positioning/explorer-positioning';
import {
  GeographicMap,
  MapCircle,
  MapCoordinate,
} from '../../../../shared/geographic-map/geographic-map';
import { serverMessage } from '../../../../shared/util/server-message';
import { SocialChallengeRunStateDto } from '../../api/games-api-types';
import { SocialChallengeSolving } from '../social-challenge-solving';

// The run of a social challenge the explorer activated on the challenge map. Every 10 s the
// page reports the explorer's position and reads the run state again. Opening the page does
// not activate the challenge; it shows whatever state the server returns.
@Component({
  imports: [GeographicMap, RouterLink],
  selector: 'app-solve-social-challenge',
  styleUrl: './solve-social-challenge.scss',
  templateUrl: './solve-social-challenge.html',
})
export class SolveSocialChallenge {
  private readonly explorerPositioning = inject(ExplorerPositioning);
  private readonly socialChallengeSolving = inject(SocialChallengeSolving);

  // The challenge id from the route /games/social-challenges/:id/solve.
  readonly id = input.required<string>();

  // The server answers 204 with no body when the explorer has no position yet; the value is then null.
  protected readonly position = httpResource<CoordinatesDto | null>(
    () => '/api/shared/positions/mine',
  );
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

  protected readonly runState = httpResource<SocialChallengeRunStateDto>(
    () => `/api/games/social-challenges/${this.id()}/run-state`,
  );
  protected readonly state = computed(() =>
    this.runState.hasValue() ? this.runState.value() : null,
  );
  protected readonly runStateError = computed(() =>
    this.runState.error()
      ? serverMessage(this.runState.error(), 'The challenge could not be loaded.')
      : null,
  );
  // A circle of the challenge's range around it.
  protected readonly rangeCircle = computed<MapCircle | null>(() => {
    const state = this.state();
    return state === null || state.location === null
      ? null
      : { center: state.location, radiusMeters: state.rangeInMeters };
  });

  private intervalId: number | null = null;
  protected readonly checking = signal(false);
  protected readonly checkError = signal<string | null>(null);

  protected readonly rejoining = signal(false);
  protected readonly rejoinError = signal<string | null>(null);

  constructor() {
    // The run started with the activation on the map, so the check starts with the page.
    this.startChecking();
    // Leaving the page stops the check.
    inject(DestroyRef).onDestroy(() => this.stopChecking());
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

  // Joins the run again with the current position, then reads the run state and restarts the check.
  protected async rejoin(): Promise<void> {
    const position = this.explorerPosition();
    if (this.rejoining() || position === null) {
      return;
    }

    this.rejoining.set(true);
    this.rejoinError.set(null);
    try {
      await this.socialChallengeSolving.activate(this.id(), {
        latitude: position.latitude,
        longitude: position.longitude,
      });
      this.runState.reload();
      this.startChecking();
    } catch (failure) {
      this.rejoinError.set(serverMessage(failure, 'You could not rejoin the challenge.'));
    } finally {
      this.rejoining.set(false);
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
  }

  private async check(): Promise<void> {
    const state = this.state();
    // The check serves an active run only: it stops once the run is solved, left or closed.
    if (state !== null && state.status !== 'Active') {
      this.stopChecking();
      return;
    }

    const position = this.explorerPosition();
    // While the previous check is pending, or while there is no run state or position, this one is skipped.
    if (this.checking() || state === null || position === null) {
      return;
    }

    this.checking.set(true);
    this.checkError.set(null);
    try {
      await this.socialChallengeSolving.checkPosition(this.id(), {
        latitude: position.latitude,
        longitude: position.longitude,
      });
      this.runState.reload();
    } catch (failure) {
      this.checkError.set(serverMessage(failure, 'Your position could not be checked.'));
    } finally {
      this.checking.set(false);
    }
  }
}
