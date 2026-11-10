import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, applyEach, form, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubDto } from '../../api/social-api-types';
import { ClubAuthoring } from '../club-authoring';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-my-clubs',
  styleUrl: './my-clubs.scss',
  templateUrl: './my-clubs.html',
})
export class MyClubs {
  private readonly clubAuthoring = inject(ClubAuthoring);

  protected readonly clubs = httpResource<ClubDto[]>(() => '/api/social/clubs/mine');

  protected readonly editingClubId = signal<string | null>(null);
  protected readonly confirmingClubId = signal<string | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly editError = signal<string | null>(null);

  protected readonly editForm = form(
    signal({ name: '', description: '', images: [''] }),
    (path) => {
      required(path.name, { message: 'Name is required.' });
      required(path.description, { message: 'Description is required.' });
      applyEach(path.images, (image) => {
        required(image, { message: 'Image URL is required.' });
      });
    },
  );

  protected startEdit(club: ClubDto): void {
    this.confirmingClubId.set(null);
    this.editError.set(null);
    this.editingClubId.set(club.id);
    this.editForm().reset({
      name: club.name,
      description: club.description,
      images: [...club.images],
    });
  }

  protected cancelEdit(): void {
    this.editingClubId.set(null);
    this.editError.set(null);
  }

  protected addImage(): void {
    this.editForm.images().value.update((images) => [...images, '']);
  }

  protected removeImage(index: number): void {
    this.editForm.images().value.update((images) => images.filter((_, i) => i !== index));
  }

  protected async save(event: Event, clubId: string): Promise<void> {
    event.preventDefault();
    if (!this.editForm().valid()) {
      this.editForm().markAsTouched();
      return;
    }
    this.pending.set(true);
    this.editError.set(null);
    try {
      await this.clubAuthoring.update(clubId, this.editForm().value());
      this.editingClubId.set(null);
      this.clubs.reload();
    } catch (failure) {
      this.editError.set(serverMessage(failure, 'Could not update the club.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected startDelete(clubId: string): void {
    this.cancelEdit();
    this.confirmingClubId.set(clubId);
  }

  protected async remove(clubId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubAuthoring.delete(clubId);
      this.confirmingClubId.set(null);
      this.clubs.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the club.'));
    } finally {
      this.pending.set(false);
    }
  }
}
