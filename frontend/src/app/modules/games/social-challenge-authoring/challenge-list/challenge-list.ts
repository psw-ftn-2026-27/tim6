import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, max, maxLength, min, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import {
  GeographicMap,
  MapCoordinate,
  MapPin,
} from '../../../../shared/geographic-map/geographic-map';
import { serverMessage } from '../../../../shared/util/server-message';
import { LocationDto, SocialChallengeDto } from '../../api/games-api-types';
import { ChallengeAction, ChallengeCard } from '../challenge-card/challenge-card';
import { SocialChallengeAuthoring } from '../social-challenge-authoring';

@Component({
  imports: [ChallengeCard, FormField, GeographicMap, RouterLink],
  selector: 'app-challenge-list',
  styleUrl: './challenge-list.scss',
  templateUrl: './challenge-list.html',
})
export class ChallengeList {
  private readonly socialChallengeAuthoring = inject(SocialChallengeAuthoring);
  private readonly auth = inject(Auth);

  protected readonly isAdministrator = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );

  protected readonly challenges = httpResource<SocialChallengeDto[]>(
    () => '/api/games/social-challenges',
  );

  protected readonly editingChallengeId = signal<string | null>(null);
  protected readonly confirmingChallengeId = signal<string | null>(null);
  protected readonly confirmingAction = signal<ChallengeAction | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly editError = signal<string | null>(null);

  // A map click is not typed text, so the chosen location is kept next to the form.
  protected readonly editLocation = signal<LocationDto | null>(null);
  // The map shows the saved location as a pin; a click places the chosen one beside it.
  protected readonly editSavedPins = signal<MapPin[]>([]);

  // The time-window rules (start and end differ, 1 to 6 hours, may cross midnight) are
  // checked by the server; its message is shown in the edit form.
  protected readonly editForm = form(
    signal({
      name: '',
      start: '',
      end: '',
      requiredPeople: 2,
      rangeInMeters: 20,
      xp: 1,
      exitMessage: '',
    }),
    (path) => {
      required(path.name, { message: 'Naziv je obavezan.' });
      maxLength(path.name, 200, { message: 'Naziv može imati najviše 200 znakova.' });
      required(path.start, { message: 'Početak je obavezan.' });
      required(path.end, { message: 'Kraj je obavezan.' });
      required(path.requiredPeople, { message: 'Potreban broj ljudi je obavezan.' });
      min(path.requiredPeople, 2, { message: 'Potrebno je najmanje 2 osobe.' });
      max(path.requiredPeople, 30, { message: 'Može najviše 30 osoba.' });
      required(path.rangeInMeters, { message: 'Opseg je obavezan.' });
      min(path.rangeInMeters, 20, { message: 'Opseg je najmanje 20 m.' });
      max(path.rangeInMeters, 300, { message: 'Opseg je najviše 300 m.' });
      required(path.xp, { message: 'XP je obavezan.' });
      min(path.xp, 1, { message: 'XP mora biti veći od 0.' });
      required(path.exitMessage, { message: 'Izlazna poruka je obavezna.' });
    },
  );

  protected startEdit(challenge: SocialChallengeDto): void {
    this.cancelAction();
    this.editError.set(null);
    this.editingChallengeId.set(challenge.id);
    this.editLocation.set(challenge.location);
    this.editSavedPins.set(challenge.location === null ? [] : [challenge.location]);
    this.editForm().reset({
      name: challenge.name,
      start: challenge.timeWindow.start,
      end: challenge.timeWindow.end,
      requiredPeople: challenge.requiredPeople,
      rangeInMeters: challenge.rangeInMeters,
      xp: challenge.xp,
      exitMessage: challenge.exitMessage,
    });
  }

  protected cancelEdit(): void {
    this.editingChallengeId.set(null);
    this.editError.set(null);
  }

  protected async save(event: Event, challenge: SocialChallengeDto): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.editError.set(null);
    try {
      await this.socialChallengeAuthoring.update(challenge.id, {
        name: this.editForm.name().value(),
        timeWindow: { start: this.editForm.start().value(), end: this.editForm.end().value() },
        requiredPeople: this.editForm.requiredPeople().value(),
        rangeInMeters: this.editForm.rangeInMeters().value(),
        xp: this.editForm.xp().value(),
        exitMessage: this.editForm.exitMessage().value(),
        location: this.editLocation(),
      });
      this.editingChallengeId.set(null);
      this.challenges.reload();
    } catch (failure) {
      this.editError.set(serverMessage(failure, 'Izmena nije sačuvana.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected onLocationSelected(coordinate: MapCoordinate): void {
    this.editLocation.set({ latitude: coordinate.latitude, longitude: coordinate.longitude });
  }

  protected startAction(challengeId: string, action: ChallengeAction): void {
    this.cancelEdit();
    this.confirmingChallengeId.set(challengeId);
    this.confirmingAction.set(action);
  }

  protected cancelAction(): void {
    this.confirmingChallengeId.set(null);
    this.confirmingAction.set(null);
  }

  protected async runAction(challenge: SocialChallengeDto, action: ChallengeAction): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      switch (action) {
        case 'publish':
        case 'republish':
          await this.socialChallengeAuthoring.publish(challenge.id);
          break;
        case 'retire':
          await this.socialChallengeAuthoring.retire(challenge.id);
          break;
        case 'archive':
          await this.socialChallengeAuthoring.archive(challenge.id);
          break;
        case 'delete':
          await this.socialChallengeAuthoring.delete(challenge.id);
          break;
      }
      this.cancelAction();
      this.challenges.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Radnja nije izvršena.'));
    } finally {
      this.pending.set(false);
    }
  }
}
