import { Component, input, output, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { CommentDto } from '../../api/social-api-types';

export interface CommentEdit {
  commentId: string;
  text: string;
}

@Component({
  imports: [FormField],
  selector: 'app-blog-comments',
  styleUrl: './blog-comments.scss',
  templateUrl: './blog-comments.html',
})
export class BlogComments {
  readonly comments = input.required<CommentDto[]>();
  readonly currentUserId = input<string | null>(null);
  // False when the blog no longer accepts comments (it is not published): the comments
  // are shown, but there is no form for a new one and no edit or delete buttons.
  readonly commentsOpen = input.required<boolean>();
  // The moment the blog was loaded, in milliseconds. A comment can be changed only while
  // this moment is before its editableUntil; the server checks the deadline again.
  readonly loadedAt = input.required<number>();
  // The comment whose edit form is open, and the server's message for its last failed save.
  readonly editingCommentId = input<string | null>(null);
  readonly editError = input<string | null>(null);

  readonly addComment = output<string>();
  readonly startEdit = output<string>();
  readonly cancelEdit = output<void>();
  readonly editComment = output<CommentEdit>();
  readonly deleteComment = output<string>();

  protected readonly form = form(signal({ text: '' }), (path) => {
    required(path.text, { message: 'A comment cannot be empty.' });
  });

  protected readonly editForm = form(signal({ text: '' }), (path) => {
    required(path.text, { message: 'A comment cannot be empty.' });
  });

  protected canChange(comment: CommentDto): boolean {
    return (
      this.commentsOpen() &&
      comment.userId === this.currentUserId() &&
      this.loadedAt() < new Date(comment.editableUntil).getTime()
    );
  }

  // Local time without seconds, for example 17.11.2026. 17:45.
  protected formatTime(value: string): string {
    const date = new Date(value);
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${day}.${month}.${date.getFullYear()}. ${hours}:${minutes}`;
  }

  protected submitComment(event: Event): void {
    event.preventDefault();
    this.addComment.emit(this.form.text().value());
    this.form().reset({ text: '' });
  }

  protected openEdit(comment: CommentDto): void {
    this.editForm().reset({ text: comment.text });
    this.startEdit.emit(comment.id);
  }

  // The form stays open: the page closes it once the server accepts the edit.
  protected saveEdit(event: Event, commentId: string): void {
    event.preventDefault();
    this.editComment.emit({ commentId, text: this.editForm.text().value() });
  }
}
