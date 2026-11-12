import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, max, maxLength, min, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { SocialChallengeAuthoring } from '../social-challenge-authoring';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-create-challenge',
  styleUrl: './create-challenge.scss',
  templateUrl: './create-challenge.html',
})
export class CreateChallenge {
  private readonly socialChallengeAuthoring = inject(SocialChallengeAuthoring);
  private readonly router = inject(Router);
  private readonly auth = inject(Auth);

  protected readonly isAdministrator = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );

  // The time-window rules (start and end differ, 1 to 6 hours, may cross midnight) are
  // checked by the server; its message is shown above the form.
  protected readonly form = form(
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

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.socialChallengeAuthoring.create({
        name: this.form.name().value(),
        timeWindow: { start: this.form.start().value(), end: this.form.end().value() },
        requiredPeople: this.form.requiredPeople().value(),
        rangeInMeters: this.form.rangeInMeters().value(),
        xp: this.form.xp().value(),
        exitMessage: this.form.exitMessage().value(),
      });
      await this.router.navigate(['/games/challenges']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Izazov nije sačuvan.'));
    } finally {
      this.pending.set(false);
    }
  }
}
