import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';

import { MonitoringTargetService } from '../../../core/services/monitoring-target.service';

@Component({
  selector: 'app-target-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './target-form.component.html',
  styleUrl: './target-form.component.css'
})
export class TargetFormComponent implements OnInit {
  isEditMode = false;
  targetId: number | null = null;
  isLoading = false;
  isSaving = false;
  errorMessage = '';

  form = {
    name: '',
    url: '',
    monitoringInterval: 60,
    isActive: true
  };

  constructor(
    private targetService: MonitoringTargetService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.targetId = +id;
      this.loadTarget(this.targetId);
    }
  }

  private loadTarget(id: number): void {
    this.isLoading = true;
    this.targetService.getById(id).subscribe({
      next: (target) => {
        this.form = {
          name: target.name,
          url: target.url,
          monitoringInterval: target.monitoringInterval,
          isActive: target.isActive
        };
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load target.';
        this.isLoading = false;
      }
    });
  }

  get pageTitle(): string {
    return this.isEditMode ? 'Edit Target' : 'Add Target';
  }

  onSubmit(): void {
    if (!this.isFormValid()) return;

    this.isSaving = true;
    this.errorMessage = '';

    if (this.isEditMode && this.targetId) {
      this.targetService.update(this.targetId, this.form).subscribe({
        next: () => this.router.navigate(['/targets', this.targetId]),
        error: () => {
          this.errorMessage = 'Failed to update target.';
          this.isSaving = false;
        }
      });
    } else {
      this.targetService.create({
        name: this.form.name,
        url: this.form.url,
        monitoringInterval: this.form.monitoringInterval
      }).subscribe({
        next: (created) => this.router.navigate(['/targets', created.id]),
        error: () => {
          this.errorMessage = 'Failed to create target. Please check your input.';
          this.isSaving = false;
        }
      });
    }
  }

  isFormValid(): boolean {
    return (
      this.form.name.trim().length > 0 &&
      this.form.url.trim().length > 0 &&
      this.form.monitoringInterval >= 10 &&
      this.form.monitoringInterval <= 3600
    );
  }
}
