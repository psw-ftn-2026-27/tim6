import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MarkdownComponent } from 'ngx-markdown';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { BlogDto, FollowStatusDto } from '../../api/social-api-types';
import { BlogComments, CommentEdit } from '../blog-comments/blog-comments';
import { BlogReading } from '../blog-reading';
import { FollowButton } from '../follow-button/follow-button';

@Component({
  imports: [BlogComments, FollowButton, MarkdownComponent, RouterLink],
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

  protected readonly followPending = signal(false);
  protected readonly error = signal<string | null>(null);

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

  protected async add(text: string): Promise<void> {
    this.error.set(null);
    try {
      await this.blogReading.addComment(this.id(), { text });
      this.detail.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not add the comment.'));
    }
  }

  protected async edit(edit: CommentEdit): Promise<void> {
    this.error.set(null);
    try {
      await this.blogReading.updateComment(this.id(), edit.commentId, { text: edit.text });
      this.detail.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not update the comment.'));
    }
  }

  protected async remove(commentId: string): Promise<void> {
    this.error.set(null);
    try {
      await this.blogReading.deleteComment(this.id(), commentId);
      this.detail.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the comment.'));
    }
  }
}
