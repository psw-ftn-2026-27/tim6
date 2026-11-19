import { DatePipe } from '@angular/common';
import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import {
  ConversationSummaryDto,
  UserSearchPageDto,
} from '../../api/social-api-types';
import { DirectConversationMessaging } from '../direct-conversation-messaging';

@Component({
  selector: 'app-conversation-list',
  imports: [DatePipe, FormField, RouterLink],
  templateUrl: './conversation-list.html',
  styleUrl: './conversation-list.scss',
})
export class ConversationList {
  private readonly messaging = inject(DirectConversationMessaging);
  private readonly router = inject(Router);
  private readonly auth = inject(Auth);

  protected readonly conversations = httpResource<PageResult<ConversationSummaryDto>>(
    () => '/api/social/conversations/mine?page=1&pageSize=50',
  );

  private readonly searchModel = signal({ searchTerm: '' });
  protected readonly searchForm = form(this.searchModel, (path) => {
    required(path.searchTerm, { message: 'Enter an email or part of an email.' });
  });
  private readonly submittedSearch = signal('');
  protected readonly users = httpResource<UserSearchPageDto>(() => {
    const searchTerm = this.submittedSearch();
    return searchTerm.length === 0
      ? undefined
      : `/api/social/conversations/users?searchTerm=${encodeURIComponent(searchTerm)}&page=1&pageSize=20`;
  });
  protected readonly availableUsers = computed(() =>
    (this.users.value()?.items ?? []).filter(
      (user) => user.userId !== this.auth.user()?.id,
    ),
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected search(event: Event): void {
    event.preventDefault();
    if (this.searchForm().valid()) {
      this.error.set(null);
      this.submittedSearch.set(this.searchForm.searchTerm().value().trim());
    }
  }

  protected async open(otherUserId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      const conversationId = await this.messaging.open(otherUserId);
      await this.router.navigate(['/social/messages', conversationId]);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not open the conversation.'));
    } finally {
      this.pending.set(false);
    }
  }
}
