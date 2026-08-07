import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { MonitoringTargetService } from '../../../core/services/monitoring-target.service';
import { MonitoringTarget } from '../../../core/models/monitoring-target.model';

@Component({
  selector: 'app-target-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './target-list.component.html',
  styleUrl: './target-list.component.css'
})
export class TargetListComponent implements OnInit {
  targets: MonitoringTarget[] = [];
  isLoading = true;
  errorMessage = '';
  deleteConfirmId: number | null = null;

  constructor(private targetService: MonitoringTargetService) {}

  ngOnInit(): void {
    this.loadTargets();
  }

  loadTargets(): void {
    this.isLoading = true;
    this.targetService.getAll().subscribe({
      next: (targets) => {
        this.targets = targets;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load targets.';
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
        this.targets = this.targets.filter(t => t.id !== id);
        this.deleteConfirmId = null;
      },
      error: () => {
        this.errorMessage = 'Failed to delete target.';
        this.deleteConfirmId = null;
      }
    });
  }
}
