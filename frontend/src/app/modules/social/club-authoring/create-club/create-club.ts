import { Component, DestroyRef, inject, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import {
  ImageUpload,
  ImageUploadError,
  UploadedImage,
  fileKey,
} from '../../../../shared/image-uploading/image-upload/image-upload';
import { ImageUploading } from '../../../../shared/image-uploading/image-uploading';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubAuthoring } from '../club-authoring';

@Component({
  imports: [FormField, ImageUpload, RouterLink],
  selector: 'app-create-club',
  styleUrl: './create-club.scss',
  templateUrl: './create-club.html',
  host: { '(window:pagehide)': 'onPageHide()' },
})
export class CreateClub {
  private readonly clubAuthoring = inject(ClubAuthoring);
  private readonly imageUploading = inject(ImageUploading);
  private readonly router = inject(Router);

  protected readonly maxImages = 5;

  protected readonly form = form(signal({ name: '', description: '' }), (path) => {
    required(path.name, { message: 'Name is required.' });
    required(path.description, { message: 'Description is required.' });
  });

  // On a create form every image in the list was uploaded by this form.
  protected readonly images = signal<UploadedImage[]>([]);
  protected readonly pendingFiles = signal<File[]>([]);
  protected readonly imageErrors = signal<ImageUploadError[]>([]);
  protected readonly imageMissing = signal(false);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  constructor() {
    inject(DestroyRef).onDestroy(() => this.imageUploading.deleteAll(this.references(), false));
  }

  protected onPageHide(): void {
    this.imageUploading.deleteAll(this.references(), true);
  }

  protected async uploadFiles(files: File[]): Promise<void> {
    this.imageErrors.set([]);
    this.pendingFiles.update((pending) => [...pending, ...files]);
    for (const file of files) {
      try {
        const reference = await this.imageUploading.upload(file);
        this.images.update((images) => [...images, { reference, fileKey: fileKey(file) }]);
      } catch (failure) {
        const reason = serverMessage(failure, 'Could not upload the image.');
        this.imageErrors.update((errors) => [...errors, { fileName: file.name, reason }]);
      } finally {
        this.pendingFiles.update((pending) => pending.filter((pendingFile) => pendingFile !== file));
      }
    }
  }

  protected async removeImage(reference: string): Promise<void> {
    this.imageErrors.set([]);
    try {
      await this.imageUploading.delete(reference);
      this.images.update((images) => images.filter((image) => image.reference !== reference));
    } catch (failure) {
      const reason = serverMessage(failure, 'Could not remove the image.');
      this.imageErrors.set([{ fileName: 'Image', reason }]);
    }
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    const imageMissing = this.images().length === 0;
    this.imageMissing.set(imageMissing);
    if (!this.form().valid() || imageMissing) {
      this.form().markAsTouched();
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubAuthoring.create({ ...this.form().value(), images: this.references() });
      // The club now owns the images, so leaving the page must not delete them.
      this.images.set([]);
      await this.router.navigate(['/social/clubs/mine']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the club.'));
    } finally {
      this.pending.set(false);
    }
  }

  private references(): string[] {
    return this.images().map((image) => image.reference);
  }
}
