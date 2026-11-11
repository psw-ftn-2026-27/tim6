import { Component, inject, signal } from '@angular/core';
import { FormField, form, min, required } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourDifficulty } from '../../api/exploration-api-types';
import { TourAuthoring } from '../tour-authoring';

@Component({
  imports: [FormField],
  selector: 'app-create-tour',
  styleUrl: './create-tour.scss',
  templateUrl: './create-tour.html',
})
export class CreateTour {
  private readonly tourAuthoring = inject(TourAuthoring);
  private readonly router = inject(Router);

  protected readonly form = form(
    signal({
      name: '',
      description: '',
      difficulty: 'Easy' as TourDifficulty,
      tags: '',
      groupSizeMin: 1,
      groupSizeMax: 10,
    }),
    (path) => {
      required(path.name, { message: 'Name is required.' });
      required(path.description, { message: 'Description is required.' });
      required(path.groupSizeMin, { message: 'Group size from is required.' });
      min(path.groupSizeMin, 1, { message: 'Group size from must be at least 1.' });
      required(path.groupSizeMax, { message: 'Group size to is required.' });
      min(path.groupSizeMax, 1, { message: 'Group size to must be at least 1.' });
    },
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.create({
        name: this.form.name().value(),
        description: this.form.description().value(),
        difficulty: this.form.difficulty().value(),
        tags: this.form
          .tags()
          .value()
          .split(',')
          .map((tag) => tag.trim())
          .filter((tag) => tag !== ''),
        recommendedGroupSize: {
          min: this.form.groupSizeMin().value(),
          max: this.form.groupSizeMax().value(),
        },
      });
      await this.router.navigate(['/exploration/mine']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the tour.'));
    } finally {
      this.pending.set(false);
    }
  }
}
