import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';

import { ProjectService } from '../../services/project.service';
import { TaskService } from '../../services/task.service';
import { AuthService } from '../../services/auth.service';
import { Project, ProjectStatus } from '../../models/project.model';
import { Task, TaskStatus, TaskPriority } from '../../models/task.model';

@Component({
  selector: 'app-dashboard',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly taskService = inject(TaskService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly fb = inject(FormBuilder);

  projects: Project[] = [];
  tasks: Task[] = [];
  isLoadingProjects = false;
  isLoadingTasks = false;
  errorMessage = '';

  taskForm!: FormGroup;
  isEditMode = false;
  selectedTaskId: string | null = null;
  showTaskDialog = false;

  taskStatuses = Object.values(TaskStatus);
  taskPriorities = Object.values(TaskPriority);

  ngOnInit(): void {
    this.initTaskForm();
    this.loadProjects();
    this.loadTasks();
  }

  private initTaskForm(): void {
    this.taskForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(3)]],
      description: [''],
      status: [TaskStatus.Todo, Validators.required],
      priority: [TaskPriority.Medium, Validators.required],
      projectId: ['', Validators.required],
      dueDate: [''],
      estimatedHours: [null],
    });
  }

  loadProjects(): void {
    this.isLoadingProjects = true;
    this.errorMessage = '';

    this.projectService.getProjects(1, 100).subscribe({
      next: (response) => {
        this.projects = response.items;
        this.isLoadingProjects = false;
      },
      error: (error) => {
        console.error('Error loading projects:', error);
        this.errorMessage = 'Failed to load projects';
        this.isLoadingProjects = false;
      },
    });
  }

  loadTasks(): void {
    this.isLoadingTasks = true;

    this.taskService.getTasks(1, 100).subscribe({
      next: (response) => {
        this.tasks = response.items;
        this.isLoadingTasks = false;
      },
      error: (error) => {
        console.error('Error loading tasks:', error);
        this.errorMessage = 'Failed to load tasks';
        this.isLoadingTasks = false;
      },
    });
  }

  openTaskDialog(task?: Task): void {
    this.isEditMode = !!task;
    this.selectedTaskId = task?.id || null;

    if (task) {
      this.taskForm.patchValue({
        title: task.title,
        description: task.description,
        status: task.status,
        priority: task.priority,
        projectId: task.projectId,
        dueDate: task.dueDate ? new Date(task.dueDate) : null,
        estimatedHours: task.estimatedHours,
      });
    } else {
      this.taskForm.reset({
        status: TaskStatus.Todo,
        priority: TaskPriority.Medium,
      });
    }

    this.showTaskDialog = true;
  }

  closeTaskDialog(): void {
    this.showTaskDialog = false;
    this.taskForm.reset();
    this.selectedTaskId = null;
  }

  onSubmitTask(): void {
    if (this.taskForm.invalid) {
      this.markFormGroupTouched(this.taskForm);
      return;
    }

    const taskData = this.taskForm.value;

    if (this.isEditMode && this.selectedTaskId) {
      this.updateTask(this.selectedTaskId, taskData);
    } else {
      this.createTask(taskData);
    }
  }

  private createTask(taskData: any): void {
    this.taskService.createTask(taskData).subscribe({
      next: (task) => {
        console.log('Task created:', task);
        this.loadTasks();
        this.closeTaskDialog();
      },
      error: (error) => {
        console.error('Error creating task:', error);
        this.errorMessage = 'Failed to create task';
      },
    });
  }

  private updateTask(id: string, taskData: any): void {
    this.taskService.updateTask(id, taskData).subscribe({
      next: (task) => {
        console.log('Task updated:', task);
        this.loadTasks();
        this.closeTaskDialog();
      },
      error: (error) => {
        console.error('Error updating task:', error);
        this.errorMessage = 'Failed to update task';
      },
    });
  }

  deleteTask(taskId: string): void {
    if (confirm('Are you sure you want to delete this task?')) {
      this.taskService.deleteTask(taskId).subscribe({
        next: () => {
          console.log('Task deleted');
          this.loadTasks();
        },
        error: (error) => {
          console.error('Error deleting task:', error);
          this.errorMessage = 'Failed to delete task';
        },
      });
    }
  }

  updateTaskStatus(taskId: string, status: TaskStatus): void {
    // Use the full updateTask method since there's no separate status endpoint
    this.taskService.updateTask(taskId, { status }).subscribe({
      next: () => {
        console.log('Task status updated');
        this.loadTasks();
      },
      error: (error) => {
        console.error('Error updating task status:', error);
        this.errorMessage = 'Failed to update task status';
      },
    });
  }

  viewProjectDetails(projectId: string): void {
    this.router.navigate(['/project-details', projectId]);
  }

  logout(): void {
    this.authService.logout();
  }

  getStatusColor(status: TaskStatus): string {
    const colors: Record<TaskStatus, string> = {
      [TaskStatus.Todo]: 'default',
      [TaskStatus.InProgress]: 'primary',
      [TaskStatus.InReview]: 'accent',
      [TaskStatus.Done]: 'success',
      [TaskStatus.Blocked]: 'warn',
    };
    return colors[status] || 'default';
  }

  getPriorityColor(priority: TaskPriority): string {
    const colors: Record<TaskPriority, string> = {
      [TaskPriority.Low]: 'default',
      [TaskPriority.Medium]: 'primary',
      [TaskPriority.High]: 'accent',
      [TaskPriority.Critical]: 'warn',
    };
    return colors[priority] || 'default';
  }

  getProjectStatusColor(status: ProjectStatus): string {
    const colors: Record<ProjectStatus, string> = {
      [ProjectStatus.NotStarted]: 'default',
      [ProjectStatus.InProgress]: 'primary',
      [ProjectStatus.Completed]: 'success',
      [ProjectStatus.OnHold]: 'accent',
      [ProjectStatus.Cancelled]: 'warn',
    };
    return colors[status] || 'default';
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach((key) => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  get title() {
    return this.taskForm.get('title');
  }

  get projectId() {
    return this.taskForm.get('projectId');
  }
}
