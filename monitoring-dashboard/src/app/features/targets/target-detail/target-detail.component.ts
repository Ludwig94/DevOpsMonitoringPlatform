import {
  Component,
  OnInit,
  OnDestroy,
  ViewChild,
  ElementRef
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import {
  Chart,
  LineController,
  LineElement,
  PointElement,
  LinearScale,
  CategoryScale,
  Tooltip,
  Filler
} from 'chart.js';

import { MonitoringTargetService } from '../../../core/services/monitoring-target.service';
import { MonitoringResultService } from '../../../core/services/monitoring-result.service';

import { MonitoringTarget } from '../../../core/models/monitoring-target.model';

import {
  MonitoringResult,
  UptimeStatistics,
  AverageResponseTime
} from '../../../core/models/monitoring-result.model';

Chart.register(
  LineController,
  LineElement,
  PointElement,
  LinearScale,
  CategoryScale,
  Tooltip,
  Filler
);

@Component({
  selector: 'app-target-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './target-detail.component.html',
  styleUrl: './target-detail.component.css'
})
export class TargetDetailComponent implements OnInit, OnDestroy {

  private chartCanvas?: ElementRef<HTMLCanvasElement>;

  @ViewChild('responseChart')
  set responseChart(element: ElementRef<HTMLCanvasElement> | undefined) {
    this.chartCanvas = element;

    if (element && this.recentResults.length > 0) {
      setTimeout(() => this.buildChart());
    }
  }

  target: MonitoringTarget | null = null;

  recentResults: MonitoringResult[] = [];

  stats: UptimeStatistics | null = null;

  avgResponse: AverageResponseTime | null = null;

  isLoading = true;

  errorMessage = '';

  private chart: Chart | null = null;

  constructor(
    private route: ActivatedRoute,
    private targetService: MonitoringTargetService,
    private resultService: MonitoringResultService
  ) {}

  ngOnInit(): void {

    const id = Number(
      this.route.snapshot.paramMap.get('id')
    );

    if (!id) {
      this.errorMessage = 'Invalid target ID.';
      this.isLoading = false;
      return;
    }

    this.loadData(id);
  }

  ngOnDestroy(): void {
    this.destroyChart();
  }

  private loadData(id: number): void {

    forkJoin({
      target: this.targetService.getById(id),

      results: this.resultService.getRecentResults(
        id,
        20
      ),

      stats: this.resultService.getUptimeStats(id),

      avgResponse:
        this.resultService.getAverageResponseTime(id)

    }).subscribe({

      next: (data) => {

        this.target = data.target;

        this.recentResults = data.results;

        this.stats = data.stats;

        this.avgResponse = data.avgResponse;

        this.isLoading = false;

        /*
         * The canvas is created by *ngIf after Angular
         * updates the view. The @ViewChild setter above
         * will therefore call buildChart() when it exists.
         */
      },

      error: (error) => {

        console.error(
          'Failed to load target details:',
          error
        );

        this.errorMessage =
          'Failed to load target details.';

        this.isLoading = false;
      }
    });
  }

  private buildChart(): void {

    if (
      !this.chartCanvas ||
      !this.recentResults.length
    ) {
      return;
    }

    this.destroyChart();

    /*
     * API returns newest first.
     * Reverse so the chart goes oldest -> newest.
     */
    const ordered = [
      ...this.recentResults
    ].reverse();

    const labels = ordered.map(result => {

      const date = new Date(
        result.checkedAt
      );

      return `${date
        .getHours()
        .toString()
        .padStart(2, '0')}:${date
        .getMinutes()
        .toString()
        .padStart(2, '0')}`;
    });

    const data = ordered.map(result =>
      result.isHealthy
        ? result.responseTime
        : null
    );

    this.chart = new Chart(
      this.chartCanvas.nativeElement,
      {
        type: 'line',

        data: {

          labels,

          datasets: [
            {
              label: 'Response Time (ms)',

              data,

              borderColor: '#4ade80',

              backgroundColor:
                'rgba(74, 222, 128, 0.08)',

              borderWidth: 2,

              pointRadius: 3,

              pointBackgroundColor:
                '#4ade80',

              tension: 0.3,

              fill: true,

              spanGaps: false
            }
          ]
        },

        options: {

          responsive: true,

          maintainAspectRatio: false,

          plugins: {

            legend: {
              display: false
            },

            tooltip: {

              callbacks: {

                label: (context) =>
                  `${context.parsed.y} ms`
              }
            }
          },

          scales: {

            x: {

              ticks: {
                color: '#475569',

                font: {
                  size: 11
                }
              },

              grid: {
                color: '#1e293b'
              }
            },

            y: {

              beginAtZero: true,

              ticks: {

                color: '#475569',

                font: {
                  size: 11
                },

                callback: (value) =>
                  `${value}ms`
              },

              grid: {
                color: '#1e293b'
              }
            }
          }
        }
      }
    );
  }

  private destroyChart(): void {

    if (this.chart) {

      this.chart.destroy();

      this.chart = null;
    }
  }

  get currentStatus():
    'up' | 'down' | 'unknown' {

    if (!this.recentResults.length) {
      return 'unknown';
    }

    return this.recentResults[0].isHealthy
      ? 'up'
      : 'down';
  }

  uptimeClass(value: number): string {

    if (value >= 99) {
      return 'metric-good';
    }

    if (value >= 90) {
      return 'metric-warn';
    }

    return 'metric-bad';
  }
}