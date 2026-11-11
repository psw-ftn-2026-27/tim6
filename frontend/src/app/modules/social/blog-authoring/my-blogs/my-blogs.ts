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
import { BlogDto } from '../../api/social-api-types';
import { BlogAuthoring } from '../blog-authoring';

@Component({
  host: { '(window:pagehide)': 'onPageHide()' },
  imports: [FormField, ImageUpload, RouterLink],
  selector: 'app-my-blogs',
  styleUrl: './my-blogs.scss',
  templateUrl: './my-blogs.html',
})
export class MyBlogs {
  private readonly blogAuthoring = inject(BlogAuthoring);
  private readonly imageUploading = inject(ImageUploading);

  protected readonly blogs = httpResource<BlogDto[]>(() => '/api/social/blogs/mine');

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // The blog whose edit section is open under its row, or null when none is open.
  protected readonly selectedBlogId = signal<string | null>(null);
  protected readonly editError = signal<string | null>(null);

  protected readonly form = form(signal({ title: '', description: '' }), (path) => {
    required(path.title, { message: 'Title is required.' });
    required(path.description, { message: 'Description is required.' });
  });

  // The image section of the edit form.
  protected readonly images = signal<UploadedImage[]>([]);
  protected readonly pendingFiles = signal<File[]>([]);
  protected readonly imageErrors = signal<ImageUploadError[]>([]);

  // References uploaded in the current edit and not saved yet. The blog's saved images are
  // never in this list, so they are never deleted from the server here.
  private uploaded: string[] = [];

  constructor() {
    // Images uploaded but never saved are deleted when the page is left.
    inject(DestroyRef).onDestroy(() => this.imageUploading.deleteAll(this.uploaded, false));
  }

  protected onPageHide(): void {
    this.imageUploading.deleteAll(this.uploaded, true);
  }

  protected async publish(blogId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.blogAuthoring.publish(blogId);
      this.blogs.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not publish the blog.'));
    } finally {
      this.pending.set(false);
    }
  }

  // Opens the edit section under the blog's row, filled from the row. An edit open on
  // another row is cancelled first.
  protected startEdit(blog: BlogDto): void {
    this.discardUploads();
    this.selectedBlogId.set(blog.id);
    this.editError.set(null);
    this.imageErrors.set([]);
    this.form().reset({ title: blog.title, description: blog.description });
    this.images.set(blog.images.map((reference) => ({ reference, fileKey: null })));
  }

  protected cancelEdit(): void {
    this.discardUploads();
    this.selectedBlogId.set(null);
  }

  protected async uploadFiles(files: File[]): Promise<void> {
    this.imageErrors.set([]);
    for (const file of files) {
      this.pendingFiles.update((list) => [...list, file]);
      try {
        const reference = await this.imageUploading.upload(file);
        this.uploaded.push(reference);
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
    // A saved image of the blog is only removed from the list; saving the edit removes it.
    if (!this.uploaded.includes(reference)) {
      this.images.update((list) => list.filter((image) => image.reference !== reference));
      return;
    }
    try {
      await this.imageUploading.delete(reference);
      this.uploaded = this.uploaded.filter((uploaded) => uploaded !== reference);
      this.images.update((list) => list.filter((image) => image.reference !== reference));
    } catch (failure) {
      const reason = serverMessage(failure, 'Could not delete the image.');
      this.imageErrors.set([{ fileName: reference, reason }]);
    }
  }

  protected async saveEdit(event: Event): Promise<void> {
    event.preventDefault();
    const blogId = this.selectedBlogId();
    if (blogId === null) {
      return;
    }
    this.pending.set(true);
    this.editError.set(null);
    try {
      await this.blogAuthoring.update(blogId, {
        title: this.form.title().value(),
        description: this.form.description().value(),
        images: this.images().map((image) => image.reference),
      });
      // The uploaded images are saved now, so closing the section must not delete them.
      this.uploaded = [];
      this.selectedBlogId.set(null);
      this.blogs.reload();
    } catch (failure) {
      this.editError.set(serverMessage(failure, 'Could not save the blog.'));
    } finally {
      this.pending.set(false);
    }
  }

  // Deletes the images uploaded in the current edit; the blog keeps its saved images.
  private discardUploads(): void {
    this.imageUploading.deleteAll(this.uploaded, false);
    this.uploaded = [];
  }
}
