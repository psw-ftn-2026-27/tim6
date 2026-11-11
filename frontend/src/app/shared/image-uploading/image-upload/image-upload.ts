import { Component, computed, input, output, signal } from '@angular/core';

export interface ImageUploadError {
  fileName: string;
  reason: string;
}

// An accepted image in the list. fileKey says which file it was uploaded from, so the same
// file is not added twice; it is null for an image the form did not upload (an image of
// already saved content).
export interface UploadedImage {
  reference: string;
  fileKey: string | null;
}

// Two selections are the same file when name, size and last change are all equal.
export function fileKey(file: File): string {
  return `${file.name}|${file.size}|${file.lastModified}`;
}

const MAX_FILE_SIZE_IN_BYTES = 5 * 1024 * 1024;

@Component({
  selector: 'app-image-upload',
  styleUrl: './image-upload.scss',
  templateUrl: './image-upload.html',
})
export class ImageUpload {
  readonly images = input.required<UploadedImage[]>();
  readonly pending = input.required<File[]>();
  readonly errors = input.required<ImageUploadError[]>();
  readonly maxImages = input.required<number>();

  readonly filesSelected = output<File[]>();
  readonly removeImage = output<string>();

  // Files rejected here never reach the server, so the page does not know about them.
  private readonly rejected = signal<ImageUploadError[]>([]);

  protected readonly remaining = computed(
    () => this.maxImages() - this.images().length - this.pending().length,
  );
  protected readonly allErrors = computed(() => [...this.errors(), ...this.rejected()]);

  // Errors describe the latest action only, so a new selection or a removal clears them.
  protected remove(reference: string): void {
    this.rejected.set([]);
    this.removeImage.emit(reference);
  }

  protected selectFiles(event: Event): void {
    this.rejected.set([]);
    const picker = event.target as HTMLInputElement;
    const files = Array.from(picker.files ?? []);
    picker.value = '';

    // A file already in the list, uploading, or picked twice in this selection is skipped
    // silently: it does not reach the server and shows no error.
    const taken = new Set<string>([
      ...this.images().flatMap((image) => (image.fileKey === null ? [] : [image.fileKey])),
      ...this.pending().map(fileKey),
    ]);

    const selected: File[] = [];
    const rejected: ImageUploadError[] = [];
    for (const file of files) {
      if (taken.has(fileKey(file))) {
        continue;
      }
      taken.add(fileKey(file));

      if (file.size > MAX_FILE_SIZE_IN_BYTES) {
        rejected.push({ fileName: file.name, reason: 'The image is larger than 5 MB.' });
      } else if (selected.length >= this.remaining()) {
        rejected.push({
          fileName: file.name,
          reason: `At most ${this.maxImages()} images are allowed.`,
        });
      } else {
        selected.push(file);
      }
    }

    this.rejected.update((list) => [...list, ...rejected]);
    if (selected.length > 0) {
      this.filesSelected.emit(selected);
    }
  }
}
