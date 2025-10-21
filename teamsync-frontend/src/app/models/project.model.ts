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
  Planned = 1,
  Active = 2,
  Completed = 3,
  Cancelled = 4
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
