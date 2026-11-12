import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { marked } from 'marked';
import { BlogDto } from '../../api/social-api-types';

const PREVIEW_WORD_LIMIT = 30;

@Component({
  imports: [RouterLink],
  selector: 'app-blog-card',
  styleUrl: './blog-card.scss',
  templateUrl: './blog-card.html',
})
export class BlogCard {
  readonly blog = input.required<BlogDto>();

  protected readonly preview = computed(() => {
    const html = marked.parse(this.blog().description, { async: false });
    const plainText = new DOMParser().parseFromString(html, 'text/html').body.textContent ?? '';
    const words = plainText.split(/\s+/).filter((word) => word !== '');
    if (words.length <= PREVIEW_WORD_LIMIT) {
      return words.join(' ');
    }
    return words.slice(0, PREVIEW_WORD_LIMIT).join(' ') + '…';
  });
}
