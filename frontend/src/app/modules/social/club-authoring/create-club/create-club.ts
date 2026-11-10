import { Component, inject, signal } from '@angular/core';
import { FormField, applyEach, form, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubAuthoring } from '../club-authoring';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-create-club',
  styleUrl: './create-club.scss',
  templateUrl: './create-club.html',
})
export class CreateClub {
  private readonly clubAuthoring = inject(ClubAuthoring);
  private readonly router = inject(Router);

  protected readonly form = form(
    signal({ name: '', description: '', images: [''] }),
    (path) => {
      required(path.name, { message: 'Name is required.' });
      required(path.description, { message: 'Description is required.' });
      applyEach(path.images, (image) => {
        required(image, { message: 'Image URL is required.' });
      });
    },
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected addImage(): void {
    this.form.images().value.update((images) => [...images, '']);
  }

  protected removeImage(index: number): void {
    this.form.images().value.update((images) => images.filter((_, i) => i !== index));
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    if (!this.form().valid()) {
      this.form().markAsTouched();
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubAuthoring.create(this.form().value());
      await this.router.navigate(['/social/clubs/mine']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the club.'));
    } finally {
      this.pending.set(false);
    }
  }
}
