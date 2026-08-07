import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  MonitoringTarget,
  CreateMonitoringTargetRequest,
  UpdateMonitoringTargetRequest
} from '../models/monitoring-target.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class MonitoringTargetService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/monitoringtargets`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<MonitoringTarget[]> {
    return this.http.get<MonitoringTarget[]>(this.apiUrl);
  }

  getById(id: number): Observable<MonitoringTarget> {
    return this.http.get<MonitoringTarget>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateMonitoringTargetRequest): Observable<MonitoringTarget> {
    return this.http.post<MonitoringTarget>(this.apiUrl, request);
  }

  update(id: number, request: UpdateMonitoringTargetRequest): Observable<MonitoringTarget> {
    return this.http.put<MonitoringTarget>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
