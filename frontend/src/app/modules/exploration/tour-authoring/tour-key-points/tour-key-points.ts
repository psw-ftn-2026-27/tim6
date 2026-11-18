import { httpResource } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, input, signal } from '@angular/core';
import { FormField, form, pattern, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { GeographicMap, MapCoordinate } from '../../../../shared/geographic-map/geographic-map';
import {
  ImageUpload,
  ImageUploadError,
  UploadedImage,
  fileKey,
} from '../../../../shared/image-uploading/image-upload/image-upload';
import { ImageUploading } from '../../../../shared/image-uploading/image-uploading';
import { serverMessage } from '../../../../shared/util/server-message';
import { KeyPointDto, TourKeyPointsDto } from '../../api/exploration-api-types';
import { TourAuthoring } from '../tour-authoring';

@Component({
  host: { '(window:pagehide)': 'onPageHide()' },
  imports: [FormField, GeographicMap, ImageUpload, RouterLink],
  selector: 'app-tour-key-points',
  styleUrl: './tour-key-points.scss',
  templateUrl: './tour-key-points.html',
})
export class TourKeyPoints {
  private readonly tourAuthoring = inject(TourAuthoring);
  private readonly imageUploading = inject(ImageUploading);

  // The tour id from the route /exploration/mine/:id/key-points.
  readonly id = input.required<string>();

  protected readonly tour = httpResource<TourKeyPointsDto>(
    () => `/api/exploration/tours/${this.id()}/key-points`,
  );
  protected readonly keyPoints = computed(() =>
    this.tour.hasValue() ? this.tour.value().keyPoints : [],
  );
  protected readonly isDraft = computed(
    () => this.tour.hasValue() && this.tour.value().status === 'Draft',
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // The key point being edited, or null when the form adds a new one.
  protected readonly editingId = signal<string | null>(null);
  // The location chosen on the map for the point in the form, or null before a click.
  protected readonly location = signal<MapCoordinate | null>(null);
  // The key point whose delete waits for the route without it, or null.
  private readonly deletingId = signal<string | null>(null);

  // The route the map calculates: the saved points with the change the author is making.
  // Its length is what the next command sends to the server.
  protected readonly route = computed<MapCoordinate[]>(() => {
    const saved = this.keyPoints();
    const deletingId = this.deletingId();
    if (deletingId !== null) {
      return saved.filter((point) => point.id !== deletingId).map((point) => point.location);
    }
    const location = this.location();
    if (location === null) {
      return saved.map((point) => point.location);
    }
    const editingId = this.editingId();
    if (editingId === null) {
      return [...saved.map((point) => point.location), location];
    }
    return saved.map((point) => (point.id === editingId ? location : point.location));
  });
  // The length of the route above, or null while the map is still calculating it.
  protected readonly routeLengthKm = signal<number | null>(null);
  protected readonly routeFailed = signal(false);

  protected readonly form = form(
    signal({ name: '', description: '', secret: '', isVisibleWithoutPurchase: false }),
    (path) => {
      // required rejects only an empty value; pattern also rejects one made of spaces only.
      required(path.name, { message: 'Name is required.' });
      pattern(path.name, /\S/, { message: 'Name is required.' });
      required(path.description, { message: 'Description is required.' });
      pattern(path.description, /\S/, { message: 'Description is required.' });
    },
  );

  // The image section of the form, with at most one image.
  protected readonly images = signal<UploadedImage[]>([]);
  protected readonly pendingFiles = signal<File[]>([]);
  protected readonly imageErrors = signal<ImageUploadError[]>([]);

  // References uploaded in the form and not saved yet. A saved point's image is never in
  // this list, so it is never deleted from the server here.
  private uploaded: string[] = [];

  constructor() {
    // Images uploaded but never saved are deleted when the page is left.
    inject(DestroyRef).onDestroy(() => this.imageUploading.deleteAll(this.uploaded, false));
  }

  protected onPageHide(): void {
    this.imageUploading.deleteAll(this.uploaded, true);
  }

  protected onLocationSelected(location: MapCoordinate): void {
    // Clicks while a command is pending would change the route the command is waiting for.
    if (this.pending()) {
      return;
    }
    this.routeLengthKm.set(null);
    this.location.set(location);
  }

  protected onRouteLength(lengthKm: number | null): void {
    this.routeLengthKm.set(lengthKm);
    this.routeFailed.set(false);
    // A delete waits for the length of the route without the deleted point.
    const deletingId = this.deletingId();
    if (deletingId !== null && lengthKm !== null) {
      this.sendRemove(deletingId, lengthKm);
    }
  }

  protected onRouteFailed(): void {
    this.routeFailed.set(true);
    // Without the length, the delete is not sent.
    if (this.deletingId() !== null) {
      this.deletingId.set(null);
      this.pending.set(false);
    }
  }

  // Fills the form with the key point; saving it then updates that point.
  protected startEdit(point: KeyPointDto): void {
    this.discardUploads();
    this.error.set(null);
    this.imageErrors.set([]);
    this.editingId.set(point.id);
    this.form().reset({
      name: point.name,
      description: point.description,
      secret: point.secret ?? '',
      isVisibleWithoutPurchase: point.isVisibleWithoutPurchase,
    });
    this.images.set([{ reference: point.image, fileKey: null }]);
    this.routeLengthKm.set(null);
    this.location.set(point.location);
  }

  protected cancelEdit(): void {
    this.discardUploads();
    this.error.set(null);
    this.resetForm();
  }

  // Sent right away, without confirmation; the map first calculates the route without the
  // point, and onRouteLength sends the command with that length.
  protected remove(keyPointId: string): void {
    this.pending.set(true);
    this.error.set(null);
    this.routeLengthKm.set(null);
    this.deletingId.set(keyPointId);
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
    // A saved image of the point is only removed from the list; saving the edit replaces it.
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

  protected async save(event: Event): Promise<void> {
    event.preventDefault();
    const location = this.location();
    const lengthInKilometers = this.routeLengthKm();
    if (location === null || lengthInKilometers === null || this.images().length === 0) {
      return;
    }
    const secret = this.form.secret().value().trim();
    const dto = {
      name: this.form.name().value(),
      description: this.form.description().value(),
      image: this.images()[0].reference,
      secret: secret === '' ? null : secret,
      location,
      isVisibleWithoutPurchase: this.form.isVisibleWithoutPurchase().value(),
      lengthInKilometers,
    };
    const editingId = this.editingId();
    this.pending.set(true);
    this.error.set(null);
    try {
      if (editingId === null) {
        await this.tourAuthoring.addKeyPoint(this.id(), dto);
      } else {
        await this.tourAuthoring.updateKeyPoint(this.id(), editingId, dto);
      }
      // The uploaded image is saved now, so resetting the form must not delete it.
      this.uploaded = [];
      this.resetForm();
      this.tour.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the key point.'));
    } finally {
      this.pending.set(false);
    }
  }

  private async sendRemove(keyPointId: string, lengthInKilometers: number): Promise<void> {
    this.deletingId.set(null);
    try {
      await this.tourAuthoring.removeKeyPoint(this.id(), keyPointId, { lengthInKilometers });
      // The point being edited no longer exists, so the form goes back to a new point.
      if (this.editingId() === keyPointId) {
        this.discardUploads();
        this.resetForm();
      }
      this.tour.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the key point.'));
    } finally {
      this.pending.set(false);
    }
  }

  // Back to an empty form for a new key point.
  private resetForm(): void {
    this.editingId.set(null);
    this.form().reset({ name: '', description: '', secret: '', isVisibleWithoutPurchase: false });
    this.images.set([]);
    this.imageErrors.set([]);
    this.routeLengthKm.set(null);
    this.location.set(null);
  }

  // Deletes the images uploaded in the form; a saved point keeps its image.
  private discardUploads(): void {
    this.imageUploading.deleteAll(this.uploaded, false);
    this.uploaded = [];
  }
}
