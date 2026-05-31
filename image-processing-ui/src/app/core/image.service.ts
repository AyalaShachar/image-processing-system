import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from './api.config';
import { ImageDetails, ImageListItem } from './models';

/** Talks to the /images endpoints of the API. */
@Injectable({ providedIn: 'root' })
export class ImageService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/images`;

  /** Uploads an image; the server starts processing it asynchronously. */
  upload(file: File): Observable<ImageDetails> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<ImageDetails>(this.baseUrl, form);
  }

  /** All images that finished or errored (for display). */
  getAll(): Observable<ImageListItem[]> {
    return this.http.get<ImageListItem[]>(this.baseUrl);
  }

  /** Full details of a single image, including its pipeline history. */
  getById(id: string): Observable<ImageDetails> {
    return this.http.get<ImageDetails>(`${this.baseUrl}/${id}`);
  }

  /** Absolute URL to download/display the stored file. */
  downloadUrl(id: string): string {
    return `${this.baseUrl}/${id}/download`;
  }
}
