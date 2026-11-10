import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourDifficulty, TourPreferenceDto, TransportMode } from '../../api/exploration-api-types';
import { TourPreferenceEditing } from '../tour-preference-editing';

@Component({
  imports: [FormField],
  selector: 'app-my-preferences',
  styleUrl: './my-preferences.scss',
  templateUrl: './my-preferences.html',
})
export class MyPreferences {
  private readonly tourPreferenceEditing = inject(TourPreferenceEditing);

  // The server answers 204 with no body when the tourist has no preference; the value is then null.
  protected readonly preference = httpResource<TourPreferenceDto | null>(
    () => '/api/exploration/tour-preferences/mine',
  );

  protected readonly editing = signal(false);
  protected readonly pending = signal(false);
  protected readonly saveError = signal<string | null>(null);
  protected readonly deleteError = signal<string | null>(null);

  protected readonly form = form(
    signal({
      easy: false,
      moderate: false,
      hard: false,
      transport: 'Walking' as TransportMode,
      tags: '',
    }),
  );

  protected startEdit(preference: TourPreferenceDto): void {
    this.saveError.set(null);
    this.deleteError.set(null);
    this.editing.set(true);
    this.form().reset({
      easy: preference.difficulties.includes('Easy'),
      moderate: preference.difficulties.includes('Moderate'),
      hard: preference.difficulties.includes('Hard'),
      transport: preference.transport,
      tags: preference.tags.join(', '),
    });
  }

  protected async save(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.saveError.set(null);
    this.deleteError.set(null);
    try {
      await this.tourPreferenceEditing.save({
        difficulties: this.selectedDifficulties(),
        transport: this.form.transport().value(),
        tags: this.form
          .tags()
          .value()
          .split(',')
          .map((tag) => tag.trim())
          .filter((tag) => tag !== ''),
      });
      this.editing.set(false);
      this.preference.reload();
    } catch (failure) {
      this.saveError.set(serverMessage(failure, 'Could not save your preferences.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async delete(): Promise<void> {
    this.pending.set(true);
    this.saveError.set(null);
    this.deleteError.set(null);
    try {
      await this.tourPreferenceEditing.delete();
      this.editing.set(false);
      this.form().reset({ easy: false, moderate: false, hard: false, transport: 'Walking', tags: '' });
      this.preference.reload();
    } catch (failure) {
      this.deleteError.set(serverMessage(failure, 'Could not delete your preferences.'));
    } finally {
      this.pending.set(false);
    }
  }

  private selectedDifficulties(): TourDifficulty[] {
    const difficulties: TourDifficulty[] = [];
    if (this.form.easy().value()) {
      difficulties.push('Easy');
    }
    if (this.form.moderate().value()) {
      difficulties.push('Moderate');
    }
    if (this.form.hard().value()) {
      difficulties.push('Hard');
    }
    return difficulties;
  }
}
