import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';
import { Task, CreateTaskRequest, UpdateTaskRequest } from '../models/task.model';

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
}

export interface PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/tasks`;

  getTasks(
    pageNumber: number = 1,
    pageSize: number = 10,
    projectId?: string,
    status?: number,
    assigneeId?: string,
    sortBy?: string,
    sortDescending: boolean = false
  ): Observable<PaginatedResponse<Task>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString())
      .set('sortDescending', sortDescending.toString());

    if (projectId) {
      params = params.set('projectId', projectId);
    }
    if (status !== undefined) {
      params = params.set('status', status.toString());
    }
    if (assigneeId) {
      params = params.set('assigneeId', assigneeId);
    }
    if (sortBy) {
      params = params.set('sortBy', sortBy);
    }

    return this.http.get<ApiResponse<PaginatedResponse<Task>>>(this.apiUrl, { params }).pipe(
      map(response => response.data!)
    );
  }

  getTask(id: string): Observable<Task> {
    return this.http.get<ApiResponse<Task>>(`${this.apiUrl}/${id}`).pipe(
      map(response => response.data!)
    );
  }

  createTask(task: CreateTaskRequest): Observable<Task> {
    return this.http.post<ApiResponse<Task>>(this.apiUrl, task).pipe(
      map(response => response.data!)
    );
  }

  updateTask(id: string, task: UpdateTaskRequest): Observable<Task> {
    return this.http.put<ApiResponse<Task>>(`${this.apiUrl}/${id}`, task).pipe(
      map(response => response.data!)
    );
  }

  deleteTask(id: string): Observable<void> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`).pipe(
      map(() => undefined)
    );
  }
}

