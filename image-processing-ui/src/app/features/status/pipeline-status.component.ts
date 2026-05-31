import { Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, forkJoin, of, switchMap, timer } from 'rxjs';
import { ImageService } from '../../core/image.service';
import { PipelineService } from '../../core/pipeline.service';
import { ImageListItem, ImageStatus, PipelineStatus } from '../../core/models';
import { PieChartComponent } from './pie-chart.component';

interface StatusSnapshot {
  pipelines: PipelineStatus[];
  images: ImageListItem[];
}

const POLL_INTERVAL_MS = 3000;
const EMPTY: StatusSnapshot = { pipelines: [], images: [] };

/** Status area: two live pie charts refreshed every few seconds. */
@Component({
  selector: 'app-pipeline-status',
  standalone: true,
  imports: [PieChartComponent],
  template: `
    <section class="card">
      <h2>Live status</h2>
      <div class="charts">
        <div class="chart">
          <h3>Active images per pipeline</h3>
          <app-pie-chart
            [labels]="pipelineLabels()"
            [data]="pipelineData()"
            [colors]="pipelineColors"
            emptyText="No active pipelines" />
        </div>
        <div class="chart">
          <h3>Images by status</h3>
          <app-pie-chart
            [labels]="statusLabels"
            [data]="statusData()"
            [colors]="statusColors"
            emptyText="No images yet" />
        </div>
      </div>
    </section>
  `,
  styles: [`
    :host { display: block; height: 100%; }
    .card { height: 100%; }
    .charts { display: grid; grid-template-columns: minmax(0, 1fr) minmax(0, 1fr); gap: 1rem; }
    .chart { min-width: 0; }
    .chart h3 { margin: 0 0 .5rem; font-size: .9rem; font-weight: 600; color: #5b6573; text-align: center; }
    @media (max-width: 1100px) { .charts { grid-template-columns: minmax(0, 1fr); } }
  `],
})
export class PipelineStatusComponent {
  private readonly images = inject(ImageService);
  private readonly pipelines = inject(PipelineService);

  // Poll both endpoints every few seconds; keep polling even if a tick fails.
  private readonly snapshot = toSignal(
    timer(0, POLL_INTERVAL_MS).pipe(
      switchMap(() =>
        forkJoin({
          pipelines: this.pipelines.getActive(),
          images: this.images.getAll(),
        }).pipe(catchError(() => of(EMPTY))),
      ),
    ),
    { initialValue: EMPTY },
  );

  // Chart 1 — active images per pipeline.
  readonly pipelineColors = ['#4f7cff', '#22c55e', '#f59e0b', '#a855f7'];
  readonly pipelineLabels = computed(() => this.snapshot().pipelines.map((p) => p.pipeline));
  readonly pipelineData = computed(() => this.snapshot().pipelines.map((p) => p.activeImages));

  // Chart 2 — images by status. InProcess is derived from active pipeline totals.
  readonly statusLabels = ['In process', 'Finished', 'Process error'];
  readonly statusColors = ['#4f7cff', '#22c55e', '#ef4444'];
  readonly statusData = computed(() => {
    const snap = this.snapshot();
    const inProcess = snap.pipelines.reduce((sum, p) => sum + p.activeImages, 0);
    const finished = snap.images.filter((i) => i.status === ImageStatus.Finished).length;
    const errored = snap.images.filter((i) => i.status === ImageStatus.ProcessError).length;
    return [inProcess, finished, errored];
  });
}
