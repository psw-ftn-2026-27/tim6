import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form, max, min, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { SocialChallengeDto } from '../../api/games-api-types';
import { ChallengeCard } from '../challenge-card/challenge-card';
import { SocialChallengeAuthoring } from '../social-challenge-authoring';

@Component({
  imports: [ChallengeCard, FormField, RouterLink],
  selector: 'app-challenge-list',
  styleUrl: './challenge-list.scss',
  templateUrl: './challenge-list.html',
})
export class ChallengeList {
  private readonly socialChallengeAuthoring = inject(SocialChallengeAuthoring);

  protected readonly challenges = httpResource<SocialChallengeDto[]>(
    () => '/api/games/social-challenges',
  );

  protected readonly editingChallengeId = signal<string | null>(null);
  protected readonly confirmingChallengeId = signal<string | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly editError = signal<string | null>(null);

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
    this.confirmingChallengeId.set(null);
    this.editError.set(null);
    this.editingChallengeId.set(challenge.id);
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
      });
      this.editingChallengeId.set(null);
      this.challenges.reload();
    } catch (failure) {
      this.editError.set(serverMessage(failure, 'Izmena nije sačuvana.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected startDelete(challengeId: string): void {
    this.cancelEdit();
    this.confirmingChallengeId.set(challengeId);
  }

  protected async remove(challenge: SocialChallengeDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.socialChallengeAuthoring.delete(challenge.id);
      this.confirmingChallengeId.set(null);
      this.challenges.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Izazov nije obrisan.'));
    } finally {
      this.pending.set(false);
    }
  }
}
