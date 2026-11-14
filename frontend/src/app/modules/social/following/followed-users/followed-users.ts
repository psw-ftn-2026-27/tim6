import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { serverMessage } from '../../../../shared/util/server-message';
import { FollowedUserDto } from '../../api/social-api-types';
import { FollowTabs } from '../follow-tabs/follow-tabs';
import { Following } from '../following';

@Component({
  imports: [FollowTabs],
  selector: 'app-followed-users',
  styleUrl: './followed-users.scss',
  templateUrl: './followed-users.html',
})
export class FollowedUsers {
  private readonly following = inject(Following);

  protected readonly followedUsers = httpResource<FollowedUserDto[]>(
    () => '/api/social/following',
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async unfollow(userId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.following.unfollow(userId);
      this.followedUsers.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not stop following the user.'));
    } finally {
      this.pending.set(false);
    }
  }
}
