export interface Task {
  id: string;
  title: string;
  description?: string;
  status: TaskStatus;
  projectId: string;
  projectName?: string;
  assignedToId?: string;
  assignedToName?: string;
  dueDate?: Date | string;
  createdAt?: Date | string;
  updatedAt?: Date | string;
}

export enum TaskStatus {
  Todo = 1,
  InProgress = 2,
  Done = 3,
  Blocked = 4
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  status?: TaskStatus;
  projectId: string;
  assignedToId?: string;
  dueDate?: Date | string;
}

export interface UpdateTaskRequest {
  title?: string;
  description?: string;
  status?: TaskStatus;
  assignedToId?: string;
  dueDate?: Date | string;
}
