import { Component, input } from '@angular/core';
import { GiftCardDto, GiftCardStatus } from '../../api/payment-api-types';

const DATE_TIME_FORMATTER = new Intl.DateTimeFormat('en-US', {
  dateStyle: 'medium',
  timeStyle: 'short',
});

@Component({
  selector: 'app-gift-card-card',
  styleUrl: './gift-card-card.scss',
  templateUrl: './gift-card-card.html',
})
export class GiftCardCard {
  readonly giftCard = input.required<GiftCardDto>();
  readonly personLabel = input.required<string>();
  readonly personEmail = input.required<string | null>();

  protected statusLabel(status: GiftCardStatus): string {
    switch (status) {
      case 'Valid':
        return 'valid';
      case 'Redeemed':
        return 'redeemed';
      case 'Expired':
        return 'expired';
    }
  }

  protected dateTime(value: string): string {
    return DATE_TIME_FORMATTER.format(new Date(value));
  }
}
