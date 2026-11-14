import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { serverMessage } from '../../../../shared/util/server-message';
import { FollowerDto } from '../../api/social-api-types';
import { FollowTabs } from '../follow-tabs/follow-tabs';
import { Following } from '../following';

@Component({
  imports: [FollowTabs],
  selector: 'app-followers',
  styleUrl: './followers.scss',
  templateUrl: './followers.html',
})
export class Followers {
  private readonly following = inject(Following);

  protected readonly followers = httpResource<FollowerDto[]>(() => '/api/social/followers');

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async remove(followerId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.following.removeFollower(followerId);
      this.followers.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not remove the follower.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async followBack(userId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.following.sendFollowRequest(userId);
      this.followers.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not follow back.'));
    } finally {
      this.pending.set(false);
    }
  }
}
