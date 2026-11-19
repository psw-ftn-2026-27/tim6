import { httpResource } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { FormField, form, maxLength, min, required } from '@angular/forms/signals';
import { Auth } from '../../../../core/auth/auth';
import {
  GeographicMap,
  MapCoordinate,
  MapPin,
} from '../../../../shared/geographic-map/geographic-map';
import {
  ImageUpload,
  ImageUploadError,
  UploadedImage,
  fileKey,
} from '../../../../shared/image-uploading/image-upload/image-upload';
import { ImageUploading } from '../../../../shared/image-uploading/image-uploading';
import { serverMessage } from '../../../../shared/util/server-message';
import { LocationChallengeDto, LocationDto } from '../../api/games-api-types';
import {
  LocationChallengeAction,
  LocationChallengeCard,
} from '../location-challenge-card/location-challenge-card';
import { LocationChallengeAuthoring } from '../location-challenge-authoring';

// One hidden point in the open form: its image section and its own map. The key tells the
// points apart while one is removed or its photo is still uploading.
interface HiddenPointEntry {
  key: number;
  images: UploadedImage[];
  pendingFiles: File[];
  imageErrors: ImageUploadError[];
  location: LocationDto | null;
  // The point's saved location, shown as a fixed pin while a draft is edited.
  savedPins: MapPin[];
}

@Component({
  host: { '(window:pagehide)': 'onPageHide()' },
  imports: [FormField, GeographicMap, ImageUpload, LocationChallengeCard],
  selector: 'app-location-challenge-list',
  styleUrl: './location-challenge-list.scss',
  templateUrl: './location-challenge-list.html',
})
export class LocationChallengeList {
  private readonly locationChallengeAuthoring = inject(LocationChallengeAuthoring);
  private readonly imageUploading = inject(ImageUploading);
  private readonly auth = inject(Auth);

  protected readonly isAdministrator = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );

  protected readonly challenges = httpResource<LocationChallengeDto[]>(
    () => '/api/games/location-challenges',
  );

  // The form section is open for a new challenge (editingChallengeId is null) or for a draft.
  protected readonly formOpen = signal(false);
  protected readonly editingChallengeId = signal<string | null>(null);
  protected readonly confirmingChallengeId = signal<string | null>(null);
  protected readonly confirmingAction = signal<LocationChallengeAction | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly formError = signal<string | null>(null);

  // The rule on the number of hidden points (P4) is checked by the server; its message is
  // shown above the form. The start location is chosen on the map, not typed.
  protected readonly challengeForm = form(
    signal<{ name: string; xp: number; location: LocationDto | null }>({
      name: '',
      xp: 1,
      location: null,
    }),
    (path) => {
      required(path.name, { message: 'Name is required.' });
      maxLength(path.name, 200, { message: 'Name can have at most 200 characters.' });
      required(path.xp, { message: 'XP is required.' });
      min(path.xp, 1, { message: 'XP must be greater than 0.' });
    },
  );

  // The start location is drawn as a pin, so removing it leaves nothing on the map.
  protected readonly startPins = computed<MapPin[]>(() => {
    const location = this.challengeForm.location().value();
    return location === null ? [] : [location];
  });

  protected readonly hiddenPoints = signal<HiddenPointEntry[]>([]);
  // Keys are never reused, so an upload that finishes after its point is gone finds nothing.
  private nextKey = 0;

  protected readonly uploadPending = computed(() =>
    this.hiddenPoints().some((point) => point.pendingFiles.length > 0),
  );
  protected readonly pointsIncomplete = computed(() =>
    this.hiddenPoints().some((point) => point.images.length === 0 || point.location === null),
  );

  // References uploaded in the open form and not saved yet. A saved point's photo is never
  // in this list, so it is never deleted from the server here.
  private uploaded: string[] = [];

  constructor() {
    // Photos uploaded but never saved are deleted when the page is left.
    inject(DestroyRef).onDestroy(() => this.imageUploading.deleteAll(this.uploaded, false));
  }

  protected onPageHide(): void {
    this.imageUploading.deleteAll(this.uploaded, true);
  }

  protected startCreate(): void {
    this.cancelAction();
    this.discardUploads();
    this.error.set(null);
    this.formError.set(null);
    this.editingChallengeId.set(null);
    this.challengeForm().reset({ name: '', xp: 1, location: null });
    this.hiddenPoints.set([]);
    this.formOpen.set(true);
  }

  // The list returns whole challenges, so the form is filled from the clicked card's data.
  protected startEdit(challenge: LocationChallengeDto): void {
    this.cancelAction();
    this.discardUploads();
    this.error.set(null);
    this.formError.set(null);
    this.editingChallengeId.set(challenge.id);
    this.challengeForm().reset({
      name: challenge.name,
      xp: challenge.xp,
      location: challenge.location,
    });
    this.hiddenPoints.set(
      challenge.hiddenPoints.map((point) => ({
        key: this.nextKey++,
        images: [{ reference: point.image, fileKey: null }],
        pendingFiles: [],
        imageErrors: [],
        location: point.location,
        savedPins: [point.location],
      })),
    );
    this.formOpen.set(true);
  }

  protected cancelForm(): void {
    this.discardUploads();
    this.closeForm();
  }

  // A map click is not typed text, so the page stores the chosen location in the form.
  protected selectLocation(coordinate: MapCoordinate): void {
    this.challengeForm
      .location()
      .value.set({ latitude: coordinate.latitude, longitude: coordinate.longitude });
  }

  // The draft is saved with an empty location, which the server removes.
  protected removeLocation(): void {
    this.challengeForm.location().value.set(null);
  }

  protected addPoint(): void {
    this.hiddenPoints.update((points) => [
      ...points,
      {
        key: this.nextKey++,
        images: [],
        pendingFiles: [],
        imageErrors: [],
        location: null,
        savedPins: [],
      },
    ]);
  }

  // The point's photo uploaded in this form is deleted at once; a saved photo stays.
  protected removePoint(key: number): void {
    const point = this.hiddenPoints().find((candidate) => candidate.key === key);
    if (point === undefined) {
      return;
    }
    const references = point.images
      .map((image) => image.reference)
      .filter((reference) => this.uploaded.includes(reference));
    this.imageUploading.deleteAll(references, false);
    this.uploaded = this.uploaded.filter((reference) => !references.includes(reference));
    this.hiddenPoints.update((points) => points.filter((candidate) => candidate.key !== key));
  }

  protected selectPointLocation(key: number, coordinate: MapCoordinate): void {
    this.updatePoint(key, (point) => ({
      ...point,
      location: { latitude: coordinate.latitude, longitude: coordinate.longitude },
    }));
  }

  protected async uploadFiles(key: number, files: File[]): Promise<void> {
    this.updatePoint(key, (point) => ({ ...point, imageErrors: [] }));
    for (const file of files) {
      this.updatePoint(key, (point) => ({ ...point, pendingFiles: [...point.pendingFiles, file] }));
      try {
        const reference = await this.imageUploading.upload(file);
        // The form was closed or changed while the photo uploaded, so nothing will save it.
        if (!this.hiddenPoints().some((point) => point.key === key)) {
          this.imageUploading.deleteAll([reference], false);
          continue;
        }
        this.uploaded.push(reference);
        this.updatePoint(key, (point) => ({
          ...point,
          images: [...point.images, { reference, fileKey: fileKey(file) }],
        }));
      } catch (failure) {
        const reason = serverMessage(failure, 'Could not upload the photo.');
        this.updatePoint(key, (point) => ({
          ...point,
          imageErrors: [...point.imageErrors, { fileName: file.name, reason }],
        }));
      } finally {
        this.updatePoint(key, (point) => ({
          ...point,
          pendingFiles: point.pendingFiles.filter((pendingFile) => pendingFile !== file),
        }));
      }
    }
  }

  protected async removeImage(key: number, reference: string): Promise<void> {
    this.updatePoint(key, (point) => ({ ...point, imageErrors: [] }));
    // A saved photo of the point is only removed from the form; saving the edit replaces it.
    if (this.uploaded.includes(reference)) {
      try {
        await this.imageUploading.delete(reference);
        this.uploaded = this.uploaded.filter((uploaded) => uploaded !== reference);
      } catch (failure) {
        const reason = serverMessage(failure, 'Could not delete the photo.');
        this.updatePoint(key, (point) => ({
          ...point,
          imageErrors: [{ fileName: 'Photo', reason }],
        }));
        return;
      }
    }
    this.updatePoint(key, (point) => ({
      ...point,
      images: point.images.filter((image) => image.reference !== reference),
    }));
  }

  protected async save(event: Event): Promise<void> {
    event.preventDefault();
    if (this.pointsIncomplete() || this.uploadPending()) {
      return;
    }
    const value = this.challengeForm().value();
    const dto = {
      name: value.name,
      xp: value.xp,
      location: value.location,
      hiddenPoints: this.hiddenPoints().map((point) => ({
        image: point.images[0].reference,
        location: point.location as LocationDto,
      })),
    };
    this.pending.set(true);
    this.formError.set(null);
    try {
      const challengeId = this.editingChallengeId();
      if (challengeId === null) {
        await this.locationChallengeAuthoring.create(dto);
      } else {
        await this.locationChallengeAuthoring.update(challengeId, dto);
      }
      // The uploaded photos are saved now, so closing the form must not delete them.
      this.uploaded = [];
      this.closeForm();
      this.challenges.reload();
    } catch (failure) {
      this.formError.set(serverMessage(failure, 'The challenge was not saved.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected startAction(challengeId: string, action: LocationChallengeAction): void {
    this.cancelForm();
    this.confirmingChallengeId.set(challengeId);
    this.confirmingAction.set(action);
  }

  protected cancelAction(): void {
    this.confirmingChallengeId.set(null);
    this.confirmingAction.set(null);
  }

  protected async runAction(
    challenge: LocationChallengeDto,
    action: LocationChallengeAction,
  ): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      switch (action) {
        case 'publish':
        case 'republish':
          await this.locationChallengeAuthoring.publish(challenge.id);
          break;
        case 'retire':
          await this.locationChallengeAuthoring.retire(challenge.id);
          break;
        case 'archive':
          await this.locationChallengeAuthoring.archive(challenge.id);
          break;
        case 'delete':
          await this.locationChallengeAuthoring.delete(challenge.id);
          break;
      }
      this.cancelAction();
      this.challenges.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'The action was not completed.'));
    } finally {
      this.pending.set(false);
    }
  }

  private closeForm(): void {
    this.formOpen.set(false);
    this.editingChallengeId.set(null);
    this.formError.set(null);
    this.hiddenPoints.set([]);
  }

  // Deletes the photos uploaded in the open form; a saved challenge keeps its photos.
  private discardUploads(): void {
    this.imageUploading.deleteAll(this.uploaded, false);
    this.uploaded = [];
  }

  private updatePoint(key: number, change: (point: HiddenPointEntry) => HiddenPointEntry): void {
    this.hiddenPoints.update((points) =>
      points.map((point) => (point.key === key ? change(point) : point)),
    );
  }
}
