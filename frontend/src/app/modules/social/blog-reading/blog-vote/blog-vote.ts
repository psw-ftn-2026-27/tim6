import { Component, input, output } from '@angular/core';
import { VoteSign } from '../../api/social-api-types';

@Component({
  selector: 'app-blog-vote',
  styleUrl: './blog-vote.scss',
  templateUrl: './blog-vote.html',
})
export class BlogVote {
  readonly score = input.required<number>();
  // False for a reader who cannot vote (not logged in, or the blog is not published):
  // the section then shows only the score.
  readonly canVote = input.required<boolean>();
  // The current user's vote, highlighted; null when they have not voted.
  readonly mySign = input<VoteSign | null>(null);

  readonly vote = output<VoteSign>();
}
