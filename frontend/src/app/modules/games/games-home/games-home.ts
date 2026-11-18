import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../core/auth/auth';

@Component({
  imports: [RouterLink],
  selector: 'app-games-home',
  template: `
    <div class="stack">
      <h1>Games</h1>
      @if (isAdministrator()) {
        <div class="actions">
          <a class="button" routerLink="/games/challenges">Upravljanje izazovima</a>
          <a class="button" routerLink="/games/quizzes">Upravljanje kvizovima</a>
        </div>
      } @else {
        <div class="empty">
          <p>Ovde još nema igara za tebe. Kvizovima i izazovima za sada upravlja administrator.</p>
        </div>
      }
    </div>
  `,
})
export class GamesHome {
  private readonly auth = inject(Auth);

  protected readonly isAdministrator = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );
}
