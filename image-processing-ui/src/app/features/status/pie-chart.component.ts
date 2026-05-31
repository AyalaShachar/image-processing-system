import {
  AfterViewInit, Component, ElementRef, OnDestroy, computed, effect, input, viewChild,
} from '@angular/core';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

/** Small reusable doughnut chart wrapping Chart.js. Driven by signal inputs. */
@Component({
  selector: 'app-pie-chart',
  standalone: true,
  template: `
    <div class="chart-box">
      @if (total() === 0) {
        <p class="empty">{{ emptyText() }}</p>
      }
      <canvas #canvas></canvas>
    </div>
  `,
  styles: [`
    .chart-box { position: relative; height: 240px; }
    .empty {
      position: absolute; inset: 0; display: flex; align-items: center; justify-content: center;
      margin: 0; color: #8a94a6; font-size: .9rem;
    }
  `],
})
export class PieChartComponent implements AfterViewInit, OnDestroy {
  readonly labels = input<string[]>([]);
  readonly data = input<number[]>([]);
  readonly colors = input<string[]>([]);
  readonly emptyText = input<string>('No data yet');

  private readonly canvasRef = viewChild.required<ElementRef<HTMLCanvasElement>>('canvas');
  private chart?: Chart;

  readonly total = computed(() => this.data().reduce((sum, n) => sum + n, 0));

  constructor() {
    // Keep the chart in sync with the signal inputs after it exists.
    effect(() => {
      const labels = this.labels();
      const data = this.data();
      const colors = this.colors();
      if (!this.chart) return;
      this.chart.data.labels = labels;
      this.chart.data.datasets[0].data = data;
      if (colors.length) {
        this.chart.data.datasets[0].backgroundColor = colors;
      }
      this.chart.update();
    });
  }

  ngAfterViewInit(): void {
    this.chart = new Chart(this.canvasRef().nativeElement, {
      type: 'doughnut',
      data: {
        labels: this.labels(),
        datasets: [{ data: this.data(), backgroundColor: this.colors() }],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { position: 'bottom' } },
      },
    });
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }
}
