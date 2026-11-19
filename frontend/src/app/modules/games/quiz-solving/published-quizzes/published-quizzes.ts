import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CoordinatesDto } from '../../../../shared/api/coordinates-dto';
import { ExplorerPositioning } from '../../../../shared/explorer-positioning/explorer-positioning';
import {
  GeographicMap,
  MapCircle,
  MapCoordinate,
} from '../../../../shared/geographic-map/geographic-map';
import { serverMessage } from '../../../../shared/util/server-message';
import { PublishedQuizDto } from '../../api/games-api-types';

// Mirrors the quiz activation radius the server enforces; used only to draw the circle.
const QUIZ_ACTIVATION_RADIUS_METERS = 50;

// A temporary list of published quizzes; the challenge map replaces it later.
@Component({
  imports: [GeographicMap, RouterLink],
  selector: 'app-published-quizzes',
  styleUrl: './published-quizzes.scss',
  templateUrl: './published-quizzes.html',
})
export class PublishedQuizzes {
  private readonly explorerPositioning = inject(ExplorerPositioning);

  // The server answers 204 with no body when the explorer has no position yet; the value is then null.
  protected readonly position = httpResource<CoordinatesDto | null>(
    () => '/api/shared/positions/mine',
  );
  protected readonly explorerPosition = computed(() =>
    this.position.hasValue() ? this.position.value() : null,
  );
  protected readonly loadError = computed(() =>
    this.position.error()
      ? serverMessage(this.position.error(), 'Your position could not be loaded.')
      : null,
  );
  // A circle of the quiz activation radius around the explorer, so the map centres on them.
  protected readonly activationCircle = computed<MapCircle | null>(() => {
    const position = this.explorerPosition();
    return position === null
      ? null
      : { center: position, radiusMeters: QUIZ_ACTIVATION_RADIUS_METERS };
  });
  protected readonly moving = signal(false);
  protected readonly moveError = signal<string | null>(null);

  // The list is loaded only once a position exists, and again whenever the position changes.
  protected readonly quizzes = httpResource<PublishedQuizDto[]>(() => {
    const position = this.explorerPosition();
    return position === null
      ? undefined
      : `/api/games/quizzes/published?latitude=${position.latitude}&longitude=${position.longitude}`;
  });
  protected readonly listError = computed(() =>
    this.quizzes.error()
      ? serverMessage(this.quizzes.error(), 'The quizzes could not be loaded.')
      : null,
  );

  protected async onExplorerMoved(coordinates: MapCoordinate): Promise<void> {
    // A click while saving is ignored, so two saves are never sent at once.
    if (this.moving()) {
      return;
    }

    this.moving.set(true);
    this.moveError.set(null);
    try {
      await this.explorerPositioning.move(coordinates);
      this.position.reload();
    } catch (failure) {
      this.moveError.set(serverMessage(failure, 'Your position could not be saved.'));
    } finally {
      this.moving.set(false);
    }
  }
}
