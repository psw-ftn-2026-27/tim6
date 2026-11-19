import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CoordinatesDto } from '../../../../shared/api/coordinates-dto';
import { ExplorerPositioning } from '../../../../shared/explorer-positioning/explorer-positioning';
import {
  GeographicMap,
  MapCircle,
  MapCoordinate,
  MapPin,
} from '../../../../shared/geographic-map/geographic-map';
import { serverMessage } from '../../../../shared/util/server-message';
import { ChallengeOnMapDto } from '../../api/games-api-types';
import { ChallengeMapCommands } from '../challenge-map';
import { ChallengeDetails } from '../challenge-details/challenge-details';

// The published challenges on a map. A click on a pin shows that challenge below the map;
// a click elsewhere marks where the explorer is.
@Component({
  imports: [ChallengeDetails, GeographicMap, RouterLink],
  selector: 'app-challenge-map',
  styleUrl: './challenge-map.scss',
  templateUrl: './challenge-map.html',
})
export class ChallengeMap {
  private readonly explorerPositioning = inject(ExplorerPositioning);
  private readonly challengeMapCommands = inject(ChallengeMapCommands);
  private readonly router = inject(Router);

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

  // The challenges are shown with or without a position; they load again whenever the position changes.
  protected readonly challenges = httpResource<ChallengeOnMapDto[]>(() => {
    if (this.position.isLoading()) {
      return undefined;
    }
    const position = this.explorerPosition();
    return position === null
      ? '/api/games/challenge-map'
      : `/api/games/challenge-map?latitude=${position.latitude}&longitude=${position.longitude}`;
  });
  protected readonly challengesError = computed(() =>
    this.challenges.error()
      ? serverMessage(this.challenges.error(), 'The challenges could not be loaded.')
      : null,
  );
  protected readonly pins = computed<MapPin[]>(() =>
    (this.challenges.hasValue() ? this.challenges.value() : []).map((challenge) => ({
      id: challenge.id,
      latitude: challenge.location.latitude,
      longitude: challenge.location.longitude,
    })),
  );

  // The challenge as it was when its pin was clicked. A new position changes the address of the
  // challenges, so the list is empty until the server answers; the section shows this meanwhile,
  // with activation off until the server says again whether it is possible.
  private readonly picked = signal<ChallengeOnMapDto | null>(null);
  // The selected challenge is taken from the list read last, so it shows the new distance after a move.
  protected readonly selected = computed(() => {
    const picked = this.picked();
    if (picked === null) {
      return null;
    }
    return this.challenges.hasValue()
      ? (this.challenges.value().find((challenge) => challenge.id === picked.id) ?? null)
      : { ...picked, canActivate: false };
  });
  // A circle of the selected challenge's activation radius around it.
  protected readonly activationCircle = computed<MapCircle | null>(() => {
    const challenge = this.selected();
    return challenge === null
      ? null
      : { center: challenge.location, radiusMeters: challenge.activationRadiusInMeters };
  });

  protected readonly activating = signal(false);
  // A refused activation, shown in the section of the selected challenge.
  protected readonly activateError = signal<string | null>(null);

  protected onPinSelected(id: string): void {
    const challenges = this.challenges.hasValue() ? this.challenges.value() : [];
    this.picked.set(challenges.find((challenge) => challenge.id === id) ?? null);
    this.activateError.set(null);
  }

  // Activates a social challenge with the current position and opens its solving page;
  // a refusal keeps the explorer on the map.
  protected async onActivate(id: string): Promise<void> {
    const position = this.explorerPosition();
    if (this.activating() || position === null) {
      return;
    }

    this.activating.set(true);
    this.activateError.set(null);
    try {
      await this.challengeMapCommands.activateSocialChallenge(id, {
        latitude: position.latitude,
        longitude: position.longitude,
      });
      await this.router.navigate(['/games/social-challenges', id, 'solve']);
    } catch (failure) {
      this.activateError.set(serverMessage(failure, 'The challenge could not be activated.'));
    } finally {
      this.activating.set(false);
    }
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
}
