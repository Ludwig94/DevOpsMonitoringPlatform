import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { MonitoringTargetService } from '../../../core/services/monitoring-target.service';
import { MonitoringResultService } from '../../../core/services/monitoring-result.service';
import { MonitoringTarget } from '../../../core/models/monitoring-target.model';
import { MonitoringResult, UptimeStatistics, AverageResponseTime } from '../../../core/models/monitoring-result.model';

@Component({
  selector: 'app-target-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './target-detail.component.html',
  styleUrl: './target-detail.component.css'
})
export class TargetDetailComponent implements OnInit {
  target: MonitoringTarget | null = null;
  recentResults: MonitoringResult[] = [];
  stats: UptimeStatistics | null = null;
  avgResponse: AverageResponseTime | null = null;
  isLoading = true;
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private targetService: MonitoringTargetService,
    private resultService: MonitoringResultService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.errorMessage = 'Invalid target ID.';
      this.isLoading = false;
      return;
    }
    this.loadData(id);
  }

  private loadData(id: number): void {
    forkJoin({
      target: this.targetService.getById(id),
      results: this.resultService.getRecentResults(id, 20),
      stats: this.resultService.getUptimeStats(id),
      avgResponse: this.resultService.getAverageResponseTime(id)
    }).subscribe({
      next: (data) => {
        this.target = data.target;
        this.recentResults = data.results;
        this.stats = data.stats;
        this.avgResponse = data.avgResponse;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load target details.';
        this.isLoading = false;
      }
    });
  }

  get currentStatus(): 'up' | 'down' | 'unknown' {
    if (!this.recentResults.length) return 'unknown';
    return this.recentResults[0].isHealthy ? 'up' : 'down';
  }

  uptimeClass(value: number): string {
    if (value >= 99) return 'metric-good';
    if (value >= 90) return 'metric-warn';
    return 'metric-bad';
  }
}
