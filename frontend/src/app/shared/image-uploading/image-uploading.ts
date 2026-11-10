import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ImageReferenceDto } from '../api/image-reference-dto';

const BASE_URL = '/api/shared/images';

@Injectable({ providedIn: 'root' })
export class ImageUploading {
  private readonly http = inject(HttpClient);

  async upload(file: File): Promise<string> {
    const body = new FormData();
    body.append('file', file);
    const dto = await firstValueFrom(this.http.post<ImageReferenceDto>(BASE_URL, body));
    return dto.reference;
  }

  async delete(reference: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(reference));
  }

  // Called when the page is left, so nobody waits for the result. A failed delete only
  // leaves an unused file on the server. keepalive lets the browser finish the request
  // after the tab is closed.
  deleteAll(references: string[], keepalive: boolean): void {
    for (const reference of references) {
      firstValueFrom(this.http.delete<void>(reference, { keepalive })).catch(() => undefined);
    }
  }
}
