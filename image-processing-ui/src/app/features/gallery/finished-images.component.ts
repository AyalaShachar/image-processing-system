import { Component, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, of, switchMap, timer } from 'rxjs';
import { ImageService } from '../../core/image.service';
import { ImageDetails, ImageListItem, ImageStatus } from '../../core/models';

const POLL_INTERVAL_MS = 3000;

/** Gallery of processed images, shown at their new width/height. */
@Component({
  selector: 'app-finished-images',
  standalone: true,
  template: `
    <section class="card">
      <h2>Processed images ({{ images().length }})</h2>

      @if (images().length === 0) {
        <p class="empty">No processed images yet. Upload one to get started.</p>
      } @else {
        <div class="grid">
          @for (img of images(); track img.id) {
            <figure class="item" (click)="select(img)">
              <div class="thumb">
                @if (isError(img)) {
                  <div class="broken">⚠</div>
                } @else {
                  <img [src]="url(img.id)" [width]="img.width" [height]="img.height" [alt]="img.fileName" />
                }
              </div>
              <figcaption>
                <span class="name" [title]="img.fileName">{{ img.fileName }}</span>
                <span class="dims">{{ img.width }} × {{ img.height }}</span>
                <span class="badge" [class.error]="isError(img)">
                  {{ isError(img) ? 'Process error' : 'Finished' }}
                </span>
              </figcaption>
            </figure>
          }
        </div>
      }
    </section>

    @if (selected(); as detail) {
      <div class="overlay" (click)="close()">
        <div class="modal" (click)="$event.stopPropagation()">
          <header>
            <h3>{{ detail.fileName }}</h3>
            <button class="close" (click)="close()">✕</button>
          </header>
          <p class="meta">{{ detail.width }} × {{ detail.height }} · {{ detail.extension }} · {{ statusText(detail.status) }}</p>
          <h4>Pipeline flow</h4>
          <ol class="flow">
            @for (step of detail.pipelineHistory; track $index) {
              <li>{{ step }}</li>
            }
          </ol>
        </div>
      </div>
    }
  `,
  styles: [`
    .empty { color: #8a94a6; }
    .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(140px, 1fr)); gap: 1rem; }
    .item { margin: 0; cursor: pointer; border-radius: 8px; transition: transform .12s; }
    .item:hover { transform: translateY(-2px); }
    .thumb {
      display: flex; align-items: center; justify-content: center;
      height: 150px; overflow: auto; background: #f1f4f9; border-radius: 8px;
    }
    .thumb img { display: block; }
    .broken { font-size: 2rem; color: #ef4444; }
    figcaption { display: flex; flex-direction: column; gap: .15rem; padding: .4rem .1rem; }
    .name { font-size: .82rem; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .dims { font-size: .78rem; color: #6b7689; }
    .badge {
      align-self: flex-start; font-size: .7rem; padding: .1rem .45rem; border-radius: 999px;
      background: #dcfce7; color: #166534;
    }
    .badge.error { background: #fee2e2; color: #991b1b; }

    .overlay {
      position: fixed; inset: 0; background: rgba(16, 24, 40, .5);
      display: flex; align-items: center; justify-content: center; z-index: 10;
    }
    .modal {
      background: #fff; border-radius: 12px; padding: 1.25rem; width: min(420px, 90vw);
      max-height: 85vh; overflow-y: auto;
    }
    .modal header { display: flex; justify-content: space-between; align-items: center; }
    .modal h3 { margin: 0; font-size: 1.1rem; word-break: break-all; }
    .close { border: none; background: transparent; font-size: 1.1rem; cursor: pointer; color: #6b7689; }
    .meta { color: #6b7689; font-size: .85rem; margin: .25rem 0 1rem; }
    .modal h4 { margin: 0 0 .5rem; font-size: .9rem; }
    .flow { margin: 0; padding-left: 1.2rem; display: flex; flex-direction: column; gap: .3rem; }
    .flow li { color: #1f2a44; }
  `],
})
export class FinishedImagesComponent {
  private readonly imageService = inject(ImageService);

  readonly images = toSignal(
    timer(0, POLL_INTERVAL_MS).pipe(
      switchMap(() => this.imageService.getAll().pipe(catchError(() => of([] as ImageListItem[])))),
    ),
    { initialValue: [] as ImageListItem[] },
  );

  readonly selected = signal<ImageDetails | null>(null);

  isError = (img: ImageListItem) => img.status === ImageStatus.ProcessError;

  url(id: string): string {
    return this.imageService.downloadUrl(id);
  }

  select(img: ImageListItem): void {
    // Fetch full details (including pipeline history) for the flow view.
    this.imageService.getById(img.id).subscribe({
      next: (detail) => this.selected.set(detail),
      error: () => this.selected.set(null),
    });
  }

  close(): void {
    this.selected.set(null);
  }

  statusText(status: ImageStatus): string {
    return status === ImageStatus.ProcessError ? 'Process error' : 'Finished';
  }
}
