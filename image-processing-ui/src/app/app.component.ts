import { Component, signal } from '@angular/core';
import { ImageUploadComponent } from './features/upload/image-upload.component';
import { PipelineStatusComponent } from './features/status/pipeline-status.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [ImageUploadComponent, PipelineStatusComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent {
  /** Bumped after each upload to signal child areas to refresh. */
  readonly refreshTick = signal(0);

  onUploaded(): void {
    this.refreshTick.update((n) => n + 1);
  }
}
