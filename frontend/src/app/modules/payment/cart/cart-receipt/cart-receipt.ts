import { Component, input, output } from '@angular/core';
import { CartDto } from '../../api/payment-api-types';

@Component({
  selector: 'app-cart-receipt',
  styleUrl: './cart-receipt.scss',
  templateUrl: './cart-receipt.html',
})
export class CartReceipt {
  readonly cart = input.required<CartDto>();
  readonly pending = input(false);
  readonly removeItem = output<string>();
}
