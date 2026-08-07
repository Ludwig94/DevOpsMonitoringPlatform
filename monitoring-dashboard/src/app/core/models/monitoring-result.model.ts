export interface MonitoringResult {
  id: number;
  monitoringTargetId: number;
  responseTime: number;
  statusCode: number;
  isHealthy: boolean;
  checkedAt: string;
  errorMessage: string | null;
}

export interface UptimeStatistics {
  targetId: number;
  last24Hours: number;
  last7Days: number;
  last30Days: number;
  allTime: number;
  calculatedAt: string;
}

export interface AverageResponseTime {
  targetId: number;
  averageResponseTimeMs: number;
  sampleCount: number;
  periodHours: number;
  calculatedAt: string;
}
