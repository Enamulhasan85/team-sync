export interface Project {
  id: string;
  name: string;
  description?: string;
  startDate?: Date | string;
  endDate?: Date | string;
  status: ProjectStatus;
  ownerId?: string;
  createdAt?: Date | string;
  updatedAt?: Date | string;
  taskCount?: number;
  completedTaskCount?: number;
}

export enum ProjectStatus {
  NotStarted = 'NotStarted',
  InProgress = 'InProgress',
  Completed = 'Completed',
  OnHold = 'OnHold',
  Cancelled = 'Cancelled'
}

export interface CreateProjectRequest {
  name: string;
  description?: string;
  startDate?: Date | string;
  endDate?: Date | string;
  status?: ProjectStatus;
}

export interface UpdateProjectRequest {
  name?: string;
  description?: string;
  startDate?: Date | string;
  endDate?: Date | string;
  status?: ProjectStatus;
}
