import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from './api.config';
import { PipelineStatus } from './models';

/** Talks to the /pipelines endpoint of the API. */
@Injectable({ providedIn: 'root' })
export class PipelineService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/pipelines`;

  /** Current active pipelines and how many images each is processing. */
  getActive(): Observable<PipelineStatus[]> {
    return this.http.get<PipelineStatus[]>(this.baseUrl);
  }
}
