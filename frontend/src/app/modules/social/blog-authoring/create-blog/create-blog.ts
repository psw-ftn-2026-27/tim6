import { Component, DestroyRef, inject, signal } from '@angular/core';
import { FormField, form, pattern, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import {
  ImageUpload,
  ImageUploadError,
  UploadedImage,
  fileKey,
} from '../../../../shared/image-uploading/image-upload/image-upload';
import { ImageUploading } from '../../../../shared/image-uploading/image-uploading';
import { serverMessage } from '../../../../shared/util/server-message';
import { BlogAuthoring } from '../blog-authoring';

@Component({
  host: { '(window:pagehide)': 'onPageHide()' },
  imports: [FormField, ImageUpload, RouterLink],
  selector: 'app-create-blog',
  styleUrl: './create-blog.scss',
  templateUrl: './create-blog.html',
})
export class CreateBlog {
  private readonly blogAuthoring = inject(BlogAuthoring);
  private readonly imageUploading = inject(ImageUploading);
  private readonly router = inject(Router);

  protected readonly form = form(signal({ title: '', description: '' }), (path) => {
    // required rejects only an empty value; pattern also rejects one made of spaces only,
    // as the server does. pattern skips an empty value, so only one message shows.
    required(path.title, { message: 'Title is required.' });
    pattern(path.title, /\S/, { message: 'Title is required.' });
    required(path.description, { message: 'Description is required.' });
    pattern(path.description, /\S/, { message: 'Description is required.' });
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // The image section. On a create form every image in the list was uploaded from it.
  protected readonly images = signal<UploadedImage[]>([]);
  protected readonly pendingFiles = signal<File[]>([]);
  protected readonly imageErrors = signal<ImageUploadError[]>([]);

  constructor() {
    // Images uploaded but never saved are deleted when the page is left.
    inject(DestroyRef).onDestroy(() => this.imageUploading.deleteAll(this.uploaded(), false));
  }

  protected onPageHide(): void {
    this.imageUploading.deleteAll(this.uploaded(), true);
  }

  protected async uploadFiles(files: File[]): Promise<void> {
    this.imageErrors.set([]);
    for (const file of files) {
      this.pendingFiles.update((list) => [...list, file]);
      try {
        const reference = await this.imageUploading.upload(file);
        this.images.update((list) => [...list, { reference, fileKey: fileKey(file) }]);
      } catch (failure) {
        const reason = serverMessage(failure, 'Could not upload the image.');
        this.imageErrors.update((list) => [...list, { fileName: file.name, reason }]);
      } finally {
        this.pendingFiles.update((list) => list.filter((pendingFile) => pendingFile !== file));
      }
    }
  }

  protected async removeImage(reference: string): Promise<void> {
    this.imageErrors.set([]);
    try {
      await this.imageUploading.delete(reference);
      this.images.update((list) => list.filter((image) => image.reference !== reference));
    } catch (failure) {
      const reason = serverMessage(failure, 'Could not delete the image.');
      this.imageErrors.set([{ fileName: reference, reason }]);
    }
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.blogAuthoring.create({
        title: this.form.title().value(),
        description: this.form.description().value(),
        images: this.uploaded(),
      });
      // The images are saved now, so leaving the page must not delete them.
      this.images.set([]);
      await this.router.navigate(['/social/mine']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the blog.'));
    } finally {
      this.pending.set(false);
    }
  }

  private uploaded(): string[] {
    return this.images().map((image) => image.reference);
  }
}
