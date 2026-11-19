import { httpResource } from '@angular/common/http';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PurchaseDto } from '../../api/payment-api-types';

@Component({
  imports: [RouterLink],
  selector: 'app-my-purchases',
  styleUrl: './my-purchases.scss',
  templateUrl: './my-purchases.html',
})
export class MyPurchases {
  // The list of one explorer is small, so the server sends it whole, without pages.
  protected readonly purchases = httpResource<PurchaseDto[]>(() => '/api/payment/purchases/mine');

  // The server sends UTC time as "2026-11-12T09:30:00..."; the page shows "2026-11-12 09:30".
  protected time(value: string): string {
    return value.slice(0, 16).replace('T', ' ');
  }
}
