import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { Auth } from '../../../../core/auth/auth';
import { CoordinatesDto } from '../../../../shared/api/coordinates-dto';
import { PageResult } from '../../../../shared/api/page-result';
import { ExplorerPositioning } from '../../../../shared/explorer-positioning/explorer-positioning';
import {
  GeographicMap,
  MapCircle,
  MapCoordinate,
  MapPin,
} from '../../../../shared/geographic-map/geographic-map';
import { serverMessage } from '../../../../shared/util/server-message';
import {
  AvailableLocationChallengeDto,
  LocationChallengeAttemptDto,
} from '../../api/games-api-types';
import { AvailableChallengeCard } from '../available-challenge-card/available-challenge-card';
import { LocationChallengeSolving } from '../location-challenge-solving';

@Component({
  imports: [AvailableChallengeCard, GeographicMap],
  selector: 'app-available-challenges',
  styleUrl: './available-challenges.scss',
  templateUrl: './available-challenges.html',
})
export class AvailableChallenges {
  private readonly locationChallengeSolving = inject(LocationChallengeSolving);
  private readonly explorerPositioning = inject(ExplorerPositioning);
  private readonly auth = inject(Auth);

  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );

  protected readonly challenges = httpResource<PageResult<AvailableLocationChallengeDto>>(
    () => '/api/games/location-challenges/available?page=1&pageSize=20',
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

  // The challenge shown in the active section, kept from its card, so the section keeps its
  // name after a solved challenge leaves the list.
  protected readonly activeChallenge = signal<AvailableLocationChallengeDto | null>(null);
  // The attempt is read only once a challenge is active. The server answers 204 with no body
  // when there is no attempt; the value is then null.
  protected readonly attempt = httpResource<LocationChallengeAttemptDto | null>(() => {
    const challenge = this.activeChallenge();
    return challenge ? `/api/games/location-challenges/${challenge.id}/attempt` : undefined;
  });
  protected readonly attemptError = computed(() =>
    this.attempt.error()
      ? serverMessage(this.attempt.error(), 'The attempt could not be loaded.')
      : null,
  );

  protected readonly activatingId = signal<string | null>(null);
  // The rejected activation, shown on the card of that challenge.
  protected readonly activateError = signal<{ challengeId: string; message: string } | null>(null);

  protected readonly checking = signal(false);
  protected readonly checkError = signal<string | null>(null);

  // Start locations of the listed challenges, plus the revealed points of the active attempt.
  protected readonly pins = computed<MapPin[]>(() => {
    const starts = (this.challenges.hasValue() ? this.challenges.value().items : []).map(
      (challenge) => challenge.location,
    );
    const attempt = this.attempt.hasValue() ? this.attempt.value() : null;
    const revealed = (attempt?.hiddenPoints ?? [])
      .map((point) => point.location)
      .filter((location) => location !== null);
    return [...starts, ...revealed];
  });

  // The circle exactly as the server saved it at activation.
  protected readonly circle = computed<MapCircle | null>(() => {
    const attempt = this.attempt.hasValue() ? this.attempt.value() : null;
    return attempt ? { center: attempt.center, radiusMeters: attempt.radiusInMeters } : null;
  });

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

  // Activate and Continue call the same command; the server decides whether the attempt is new.
  protected async activate(challenge: AvailableLocationChallengeDto): Promise<void> {
    const position = this.explorerPosition();
    if (position === null) {
      return;
    }

    this.activatingId.set(challenge.id);
    this.activateError.set(null);
    try {
      await this.locationChallengeSolving.activate(challenge.id, position);
      this.checkError.set(null);
      if (this.activeChallenge()?.id === challenge.id) {
        this.attempt.reload();
      } else {
        // A new active challenge changes the attempt address, so its resource reads it anew.
        this.activeChallenge.set(challenge);
      }
    } catch (failure) {
      this.activateError.set({
        challengeId: challenge.id,
        message: serverMessage(failure, 'The challenge could not be activated.'),
      });
    } finally {
      this.activatingId.set(null);
    }
  }

  protected async checkLocation(): Promise<void> {
    const challenge = this.activeChallenge();
    const position = this.explorerPosition();
    if (challenge === null || position === null) {
      return;
    }

    this.checking.set(true);
    this.checkError.set(null);
    try {
      await this.locationChallengeSolving.checkLocation(challenge.id, position);
      this.attempt.reload();
      // The server decides whether this check solved the challenge; a solved one leaves the list.
      this.challenges.reload();
    } catch (failure) {
      this.checkError.set(serverMessage(failure, 'Your location could not be checked.'));
    } finally {
      this.checking.set(false);
    }
  }

  // Closes the active section and shows the list again. The attempt stays saved on the server,
  // so the reloaded list marks it and offers Continue.
  protected closeSection(): void {
    this.activeChallenge.set(null);
    this.checkError.set(null);
    this.activateError.set(null);
    this.challenges.reload();
  }

  protected cardError(challengeId: string): string | null {
    const error = this.activateError();
    return error !== null && error.challengeId === challengeId ? error.message : null;
  }
}
