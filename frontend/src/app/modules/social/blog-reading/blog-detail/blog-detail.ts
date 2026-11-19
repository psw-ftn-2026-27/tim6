import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MarkdownComponent } from 'ngx-markdown';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { BlogDto, FollowStatusDto, MyVoteDto, VoteSign } from '../../api/social-api-types';
import { BlogComments, CommentEdit } from '../blog-comments/blog-comments';
import { BlogReading } from '../blog-reading';
import { BlogVote } from '../blog-vote/blog-vote';
import { FollowButton } from '../follow-button/follow-button';

@Component({
  imports: [BlogComments, BlogVote, FollowButton, MarkdownComponent, RouterLink],
  selector: 'app-blog-detail',
  styleUrl: './blog-detail.scss',
  templateUrl: './blog-detail.html',
})
export class BlogDetail {
  private readonly blogReading = inject(BlogReading);
  protected readonly auth = inject(Auth);

  readonly id = input.required<string>();

  protected readonly detail = httpResource<BlogDto>(() => `/api/social/blogs/${this.id()}`);

  protected readonly followStatus = httpResource<FollowStatusDto>(() => {
    const authorId = this.detail.hasValue() ? this.detail.value().authorId : undefined;
    const canFollowAuthor =
      this.auth.isLoggedIn() && authorId !== undefined && authorId !== this.auth.user()?.id;
    return canFollowAuthor ? `/api/social/users/${authorId}/follow-status` : undefined;
  });

  protected readonly myVote = httpResource<MyVoteDto>(() =>
    this.auth.isLoggedIn() ? `/api/social/blogs/${this.id()}/votes/mine` : undefined,
  );

  // The moment the blog was last loaded. Reading the blog makes this recompute whenever a load
  // or reload delivers it, so the edit and delete buttons are re-evaluated then, not by a clock.
  protected readonly loadedAt = computed(() => {
    const blog = this.detail.hasValue() ? this.detail.value() : undefined;
    return blog ? Date.now() : 0;
  });

  protected readonly followPending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly editingCommentId = signal<string | null>(null);
  protected readonly editError = signal<string | null>(null);

  protected async follow(recipientId: string): Promise<void> {
    this.followPending.set(true);
    this.error.set(null);
    try {
      await this.blogReading.sendFollowRequest(recipientId);
      this.followStatus.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not send the follow request.'));
    } finally {
      this.followPending.set(false);
    }
  }

  // The score and the highlighted button change only when the reloads bring them from the server.
  protected async vote(sign: VoteSign): Promise<void> {
    this.error.set(null);
    try {
      await this.blogReading.castVote(this.id(), { sign });
      this.detail.reload();
      this.myVote.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not cast the vote.'));
    }
  }

  protected async add(text: string): Promise<void> {
    this.error.set(null);
    try {
      await this.blogReading.addComment(this.id(), { text });
      this.closeEdit();
      this.detail.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not add the comment.'));
    }
  }

  protected startEdit(commentId: string): void {
    this.editingCommentId.set(commentId);
    this.editError.set(null);
  }

  protected closeEdit(): void {
    this.editingCommentId.set(null);
    this.editError.set(null);
  }

  // A rejected edit keeps the form open with the typed text and shows the message below it.
  protected async edit(edit: CommentEdit): Promise<void> {
    this.error.set(null);
    this.editError.set(null);
    try {
      await this.blogReading.updateComment(this.id(), edit.commentId, { text: edit.text });
      this.closeEdit();
      this.detail.reload();
    } catch (failure) {
      this.editError.set(serverMessage(failure, 'Could not update the comment.'));
    }
  }

  protected async remove(commentId: string): Promise<void> {
    this.error.set(null);
    try {
      await this.blogReading.deleteComment(this.id(), commentId);
      this.closeEdit();
      this.detail.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the comment.'));
    }
  }
}
