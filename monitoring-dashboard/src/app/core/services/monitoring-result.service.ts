import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  MonitoringResult,
  UptimeStatistics,
  AverageResponseTime
} from '../models/monitoring-result.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class MonitoringResultService {
  private readonly baseUrl = `${environment.apiBaseUrl}/api/targets`;

  constructor(private http: HttpClient) {}

  getRecentResults(targetId: number, limit: number = 50): Observable<MonitoringResult[]> {
    const params = new HttpParams().set('limit', limit.toString());
    return this.http.get<MonitoringResult[]>(`${this.baseUrl}/${targetId}/results`, { params });
  }

  getUptimeStats(targetId: number): Observable<UptimeStatistics> {
    return this.http.get<UptimeStatistics>(`${this.baseUrl}/${targetId}/results/stats`);
  }

  getAverageResponseTime(targetId: number, hoursBack: number = 24): Observable<AverageResponseTime> {
    const params = new HttpParams().set('hoursBack', hoursBack.toString());
    return this.http.get<AverageResponseTime>(`${this.baseUrl}/${targetId}/results/average-response-time`, { params });
  }
}
