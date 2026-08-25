import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { MonitoringTargetService } from '../../../core/services/monitoring-target.service';
import { MonitoringResultService } from '../../../core/services/monitoring-result.service';
import { MonitoringTarget } from '../../../core/models/monitoring-target.model';
import { MonitoringResult } from '../../../core/models/monitoring-result.model';

@Component({
  selector: 'app-target-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './target-list.component.html',
  styleUrl: './target-list.component.css'
})
export class TargetListComponent implements OnInit {
  targets: MonitoringTarget[] = [];
  latestResults: Map<number, MonitoringResult | null> = new Map();

  isLoading = true;
  errorMessage = '';
  deleteConfirmId: number | null = null;

  constructor(
    private targetService: MonitoringTargetService,
    private resultService: MonitoringResultService
  ) {}

  ngOnInit(): void {
    this.loadTargets();
  }

  loadTargets(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.latestResults.clear();

    this.targetService.getAll().subscribe({
      next: (targets) => {
        this.targets = targets;

        if (targets.length === 0) {
          this.isLoading = false;
          return;
        }

        const requests = targets.map((target) =>
          this.resultService
            .getRecentResults(target.id, 1)
            .pipe(
              catchError(() => of([] as MonitoringResult[]))
            )
        );

        forkJoin(requests).subscribe({
          next: (results) => {
            targets.forEach((target, index) => {
              const latestResult = results[index]?.[0] ?? null;
              this.latestResults.set(target.id, latestResult);
            });

            this.isLoading = false;
          },
          error: () => {
            this.errorMessage = 'Failed to load monitoring results.';
            this.isLoading = false;
          }
        });
      },

error: () => {
  this.errorMessage = 'demo-offline';
  this.isLoading = false;
}
    });
  }

  confirmDelete(id: number): void {
    this.deleteConfirmId = id;
  }

  cancelDelete(): void {
    this.deleteConfirmId = null;
  }

  deleteTarget(id: number): void {
    this.targetService.delete(id).subscribe({
      next: () => {
        this.targets = this.targets.filter(
          target => target.id !== id
        );

        this.latestResults.delete(id);
        this.deleteConfirmId = null;
      },

      error: () => {
        this.errorMessage = 'Failed to delete target.';
        this.deleteConfirmId = null;
      }
    });
  }
}