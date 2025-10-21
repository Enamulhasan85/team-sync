export interface Task {
  id: string;
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  projectId: string;
  projectName?: string;
  assignedToId?: string;
  assignedToName?: string;
  dueDate?: Date | string;
  createdAt?: Date | string;
  updatedAt?: Date | string;
  estimatedHours?: number;
  actualHours?: number;
}

export enum TaskStatus {
  Todo = 'Todo',
  InProgress = 'InProgress',
  InReview = 'InReview',
  Done = 'Done',
  Blocked = 'Blocked'
}

export enum TaskPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Critical = 'Critical'
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  status?: TaskStatus;
  priority?: TaskPriority;
  projectId: string;
  assignedToId?: string;
  dueDate?: Date | string;
  estimatedHours?: number;
}

export interface UpdateTaskRequest {
  title?: string;
  description?: string;
  status?: TaskStatus;
  priority?: TaskPriority;
  assignedToId?: string;
  dueDate?: Date | string;
  estimatedHours?: number;
  actualHours?: number;
}
