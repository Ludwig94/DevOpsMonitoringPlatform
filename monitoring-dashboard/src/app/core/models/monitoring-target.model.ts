export interface MonitoringTarget {
  id: number;
  name: string;
  url: string;
  monitoringInterval: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateMonitoringTargetRequest {
  name: string;
  url: string;
  monitoringInterval: number;
}

export interface UpdateMonitoringTargetRequest {
  name?: string;
  url?: string;
  monitoringInterval?: number;
  isActive?: boolean;
}
