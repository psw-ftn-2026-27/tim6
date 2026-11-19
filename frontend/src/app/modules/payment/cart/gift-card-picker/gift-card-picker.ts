import { DatePipe } from '@angular/common';
import { Component, computed, input, output } from '@angular/core';
import { GiftCardDto } from '../../api/payment-api-types';

@Component({
  imports: [DatePipe],
  selector: 'app-gift-card-picker',
  styleUrl: './gift-card-picker.scss',
  templateUrl: './gift-card-picker.html',
})
export class GiftCardPicker {
  readonly cards = input.required<GiftCardDto[]>();
  readonly selectedGiftCardId = input<string | null>(null);
  readonly page = input.required<number>();
  readonly hasPrevious = input(false);
  readonly hasNext = input(false);
  readonly pending = input(false);

  readonly giftCardSelected = output<string>();
  readonly selectionRemoved = output<void>();
  readonly pageChanged = output<number>();

  protected readonly selectedGiftCard = computed(() =>
    this.cards().find((giftCard) => giftCard.id === this.selectedGiftCardId()),
  );
}
