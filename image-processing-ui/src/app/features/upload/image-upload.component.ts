import { Component, inject, output, signal } from '@angular/core';
import { ImageService } from '../../core/image.service';
import { ImageDetails } from '../../core/models';

/** Upload area: pick an image and send it to the server for processing. */
@Component({
  selector: 'app-image-upload',
  standalone: true,
  template: `
    <section class="card upload">
      <h2>Upload an image</h2>

      <label class="dropzone" [class.busy]="uploading()">
        <input type="file" accept="image/*" (change)="onFileSelected($event)" [disabled]="uploading()" />
        @if (previewUrl()) {
          <img class="preview" [src]="previewUrl()" alt="preview" />
        } @else {
          <span class="hint">Click to choose an image</span>
        }
      </label>

      @if (fileName()) {
        <p class="filename">{{ fileName() }}</p>
      }

      <button class="primary" (click)="upload()" [disabled]="!file() || uploading()">
        {{ uploading() ? 'Uploading…' : 'Upload & process' }}
      </button>

      @if (message()) {
        <p class="message" [class.error]="isError()">{{ message() }}</p>
      }
    </section>
  `,
  styles: [`
    :host { display: block; height: 100%; }
    .upload { display: flex; flex-direction: column; gap: 0.75rem; height: 100%; }
    .dropzone {
      position: relative; display: flex; align-items: center; justify-content: center;
      min-height: 160px; border: 2px dashed #b9c2d0; border-radius: 10px;
      cursor: pointer; overflow: hidden; background: #f7f9fc; transition: border-color .2s;
    }
    .dropzone:hover { border-color: #4f7cff; }
    .dropzone.busy { opacity: .6; pointer-events: none; }
    .dropzone input { position: absolute; inset: 0; opacity: 0; cursor: pointer; }
    .hint { color: #8a94a6; }
    .preview { max-height: 160px; max-width: 100%; object-fit: contain; }
    .filename { margin: 0; font-size: .85rem; color: #5b6573; word-break: break-all; }
    button.primary {
      align-self: flex-start; padding: .55rem 1.1rem; border: none; border-radius: 8px;
      background: #4f7cff; color: #fff; font-weight: 600; cursor: pointer;
    }
    button.primary:disabled { background: #b9c2d0; cursor: not-allowed; }
    .message { margin: 0; font-size: .9rem; color: #1a7f37; }
    .message.error { color: #c1121f; }
  `],
})
export class ImageUploadComponent {
  private readonly imageService = inject(ImageService);

  /** Emitted after a successful upload, so the dashboard can refresh. */
  readonly uploaded = output<ImageDetails>();

  readonly file = signal<File | null>(null);
  readonly fileName = signal<string>('');
  readonly previewUrl = signal<string | null>(null);
  readonly uploading = signal(false);
  readonly message = signal<string>('');
  readonly isError = signal(false);

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const selected = input.files?.[0] ?? null;
    this.file.set(selected);
    this.fileName.set(selected?.name ?? '');
    this.message.set('');
    this.previewUrl.set(selected ? URL.createObjectURL(selected) : null);
  }

  upload(): void {
    const file = this.file();
    if (!file) return;

    this.uploading.set(true);
    this.message.set('');
    this.imageService.upload(file).subscribe({
      next: (image) => {
        this.uploading.set(false);
        this.isError.set(false);
        this.message.set(`"${image.fileName}" uploaded (${image.width}×${image.height}). Processing…`);
        this.reset();
        this.uploaded.emit(image);
      },
      error: () => {
        this.uploading.set(false);
        this.isError.set(true);
        this.message.set('Upload failed. Is the server running?');
      },
    });
  }

  private reset(): void {
    this.file.set(null);
    this.fileName.set('');
    this.previewUrl.set(null);
  }
}
