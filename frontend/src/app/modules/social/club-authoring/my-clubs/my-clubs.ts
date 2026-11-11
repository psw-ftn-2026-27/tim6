import { httpResource } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import {
  ImageUpload,
  ImageUploadError,
  UploadedImage,
  fileKey,
} from '../../../../shared/image-uploading/image-upload/image-upload';
import { ImageUploading } from '../../../../shared/image-uploading/image-uploading';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubDto } from '../../api/social-api-types';
import { ClubAuthoring } from '../club-authoring';

@Component({
  imports: [FormField, ImageUpload, RouterLink],
  selector: 'app-my-clubs',
  styleUrl: './my-clubs.scss',
  templateUrl: './my-clubs.html',
  host: { '(window:pagehide)': 'onPageHide()' },
})
export class MyClubs {
  private readonly clubAuthoring = inject(ClubAuthoring);
  private readonly imageUploading = inject(ImageUploading);

  protected readonly clubs = httpResource<ClubDto[]>(() => '/api/social/clubs/mine');

  protected readonly maxImages = 5;

  protected readonly editingClubId = signal<string | null>(null);
  protected readonly confirmingClubId = signal<string | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly editError = signal<string | null>(null);

  protected readonly editForm = form(signal({ name: '', description: '' }), (path) => {
    required(path.name, { message: 'Name is required.' });
    required(path.description, { message: 'Description is required.' });
  });

  protected readonly editImages = signal<UploadedImage[]>([]);
  protected readonly editPendingFiles = signal<File[]>([]);
  protected readonly editImageErrors = signal<ImageUploadError[]>([]);
  protected readonly editImageMissing = signal(false);

  // References uploaded in the open edit form and not saved yet.
  private uploaded: string[] = [];

  constructor() {
    inject(DestroyRef).onDestroy(() => this.imageUploading.deleteAll(this.uploaded, false));
  }

  protected onPageHide(): void {
    this.imageUploading.deleteAll(this.uploaded, true);
  }

  protected startEdit(club: ClubDto): void {
    this.cancelEdit();
    this.confirmingClubId.set(null);
    this.editingClubId.set(club.id);
    this.editForm().reset({ name: club.name, description: club.description });
    this.editImages.set(club.images.map((reference) => ({ reference, fileKey: null })));
  }

  // The edit form lives on this page, so closing it does not destroy the page. The images
  // it uploaded and did not save are deleted here instead.
  protected cancelEdit(): void {
    this.imageUploading.deleteAll(this.uploaded, false);
    this.uploaded = [];
    this.editingClubId.set(null);
    this.editError.set(null);
    this.editImageErrors.set([]);
    this.editImageMissing.set(false);
  }

  protected async uploadFiles(files: File[]): Promise<void> {
    this.editImageErrors.set([]);
    this.editPendingFiles.update((pending) => [...pending, ...files]);
    for (const file of files) {
      try {
        const reference = await this.imageUploading.upload(file);
        this.uploaded = [...this.uploaded, reference];
        this.editImages.update((images) => [...images, { reference, fileKey: fileKey(file) }]);
      } catch (failure) {
        const reason = serverMessage(failure, 'Could not upload the image.');
        this.editImageErrors.update((errors) => [...errors, { fileName: file.name, reason }]);
      } finally {
        this.editPendingFiles.update((pending) =>
          pending.filter((pendingFile) => pendingFile !== file),
        );
      }
    }
  }

  // An image of the saved club is only taken off the list, because Cancel must be able to
  // bring it back; its file is deleted after a successful save. One this form uploaded is
  // deleted from the server right away.
  protected async removeImage(reference: string): Promise<void> {
    this.editImageErrors.set([]);
    if (this.uploaded.includes(reference)) {
      try {
        await this.imageUploading.delete(reference);
        this.uploaded = this.uploaded.filter((uploaded) => uploaded !== reference);
      } catch (failure) {
        const reason = serverMessage(failure, 'Could not remove the image.');
        this.editImageErrors.set([{ fileName: 'Image', reason }]);
        return;
      }
    }
    this.editImages.update((images) => images.filter((image) => image.reference !== reference));
  }

  protected async save(event: Event, club: ClubDto): Promise<void> {
    event.preventDefault();
    const imageMissing = this.editImages().length === 0;
    this.editImageMissing.set(imageMissing);
    if (!this.editForm().valid() || imageMissing) {
      this.editForm().markAsTouched();
      return;
    }
    this.pending.set(true);
    this.editError.set(null);
    try {
      const images = this.editImages().map((image) => image.reference);
      await this.clubAuthoring.update(club.id, { ...this.editForm().value(), images });
      // The club now owns the images, so closing the form must not delete them.
      this.uploaded = [];
      // The images taken off the club are no longer used by anyone.
      const removed = club.images.filter((reference) => !images.includes(reference));
      this.imageUploading.deleteAll(removed, false);
      this.cancelEdit();
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

  protected async remove(club: ClubDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubAuthoring.delete(club.id);
      // Only after the club is gone are its images no longer used.
      this.imageUploading.deleteAll(club.images, false);
      this.confirmingClubId.set(null);
      this.clubs.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the club.'));
    } finally {
      this.pending.set(false);
    }
  }
}
