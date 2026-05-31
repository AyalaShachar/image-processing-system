import { Component } from '@angular/core';
import { ImageUploadComponent } from './features/upload/image-upload.component';
import { PipelineStatusComponent } from './features/status/pipeline-status.component';
import { FinishedImagesComponent } from './features/gallery/finished-images.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [ImageUploadComponent, PipelineStatusComponent, FinishedImagesComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent {}
