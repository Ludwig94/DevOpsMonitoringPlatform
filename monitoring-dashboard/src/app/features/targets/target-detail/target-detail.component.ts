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

  @ViewChild('responseChart')
  chartCanvas?: ElementRef<HTMLCanvasElement>;

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
         * Wait until Angular has rendered the canvas
         * after recentResults has been populated.
         */
        setTimeout(() => {
          this.buildChart();
        });
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

    if (!this.chartCanvas) {
      console.error(
        'Response chart canvas was not found.'
      );

      return;
    }

    if (!this.recentResults.length) {
      console.warn(
        'No monitoring results available for chart.'
      );

      return;
    }

    this.destroyChart();

    /*
     * API returns newest first.
     * Reverse so the oldest check appears first.
     */
    const ordered = [
      ...this.recentResults
    ].reverse();

    /*
     * Simple labels: 1, 2, 3 ... 20.
     *
     * This removes date/time formatting from the
     * equation while we troubleshoot the chart.
     */
    const labels = ordered.map(
      (_result, index) => `${index + 1}`
    );

    /*
     * Use every response time directly.
     *
     * We deliberately don't convert unhealthy
     * results to null here.
     */
    const data = ordered.map(
      result => result.responseTime
    );

    console.log(
      'CHART DATA:',
      data
    );

    console.log(
      'CHART LABELS:',
      labels
    );

    console.log(
      'CANVAS:',
      this.chartCanvas
    );

    console.log(
      'RECENT RESULTS:',
      this.recentResults
    );

    const canvas =
      this.chartCanvas.nativeElement;

    this.chart = new Chart(
      canvas,
      {
        type: 'line',

        data: {
          labels: labels,

          datasets: [
            {
              label: 'Response Time (ms)',

              data: data,

              borderColor: '#4ade80',

              backgroundColor:
                'rgba(74, 222, 128, 0.08)',

              borderWidth: 2,

              pointRadius: 4,

              pointHoverRadius: 6,

              pointBackgroundColor:
                '#4ade80',

              pointBorderColor:
                '#4ade80',

              tension: 0.3,

              fill: true
            }
          ]
        },

        options: {

          responsive: true,

          maintainAspectRatio: false,

          animation: false,

          plugins: {

            legend: {
              display: false
            },

            tooltip: {
              enabled: true,

              callbacks: {

                label: (context) =>
                  ` ${context.parsed.y} ms`
              }
            }
          },

          scales: {

            x: {

              display: true,

              ticks: {
                color: '#94a3b8',

                font: {
                  size: 11
                }
              },

              grid: {
                color: '#1e293b'
              }
            },

            y: {

              display: true,

              beginAtZero: true,

              ticks: {

                color: '#94a3b8',

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

    console.log(
      'CHART CREATED:',
      this.chart
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