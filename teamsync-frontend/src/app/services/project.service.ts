import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';
import { Project, CreateProjectRequest, UpdateProjectRequest } from '../models/project.model';

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
export class ProjectService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/projects`;

  getProjects(
    pageNumber: number = 1,
    pageSize: number = 10,
    status?: number,
    sortBy?: string,
    sortDescending: boolean = false
  ): Observable<PaginatedResponse<Project>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString())
      .set('sortDescending', sortDescending.toString());

    if (status !== undefined) {
      params = params.set('status', status.toString());
    }
    if (sortBy) {
      params = params.set('sortBy', sortBy);
    }

    return this.http.get<ApiResponse<PaginatedResponse<Project>>>(this.apiUrl, { params }).pipe(
      map(response => response.data!)
    );
  }

  getProject(id: string): Observable<Project> {
    return this.http.get<ApiResponse<Project>>(`${this.apiUrl}/${id}`).pipe(
      map(response => response.data!)
    );
  }

  createProject(project: CreateProjectRequest): Observable<Project> {
    return this.http.post<ApiResponse<Project>>(this.apiUrl, project).pipe(
      map(response => response.data!)
    );
  }

  updateProject(id: string, project: UpdateProjectRequest): Observable<Project> {
    return this.http.put<ApiResponse<Project>>(`${this.apiUrl}/${id}`, project).pipe(
      map(response => response.data!)
    );
  }

  deleteProject(id: string): Observable<void> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`).pipe(
      map(() => undefined)
    );
  }
}

