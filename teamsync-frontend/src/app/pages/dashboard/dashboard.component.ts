import { Component, OnInit, OnDestroy, inject } from '@angular/core';
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
import { ToastrService } from 'ngx-toastr';
import { Subject, takeUntil } from 'rxjs';

import { ProjectService } from '../../services/project.service';
import { TaskService } from '../../services/task.service';
import { AuthService } from '../../services/auth.service';
import { SignalRService, TaskUpdatedEvent, ChatMessageReceivedEvent } from '../../services/signalr.service';
import { Project, ProjectStatus } from '../../models/project.model';
import { Task, TaskStatus } from '../../models/task.model';

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
export class DashboardComponent implements OnInit, OnDestroy {
  private readonly projectService = inject(ProjectService);
  private readonly taskService = inject(TaskService);
  private readonly authService = inject(AuthService);
  private readonly signalRService = inject(SignalRService);
  private readonly toastr = inject(ToastrService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly fb = inject(FormBuilder);
  private readonly destroy$ = new Subject<void>();

  projects: Project[] = [];
  tasks: Task[] = [];
  isLoadingProjects = false;
  isLoadingTasks = false;
  errorMessage = '';

  taskForm!: FormGroup;
  projectForm!: FormGroup;
  isEditMode = false;
  selectedTaskId: string | null = null;
  showTaskDialog = false;
  showProjectDialog = false;

  taskStatuses = Object.values(TaskStatus).filter((v) => typeof v === 'number') as TaskStatus[];
  projectStatuses = Object.values(ProjectStatus).filter(
    (v) => typeof v === 'number'
  ) as ProjectStatus[];

  ngOnInit(): void {
    this.initTaskForm();
    this.initProjectForm();
    this.loadProjects();
    this.loadTasks();
    this.initializeSignalR();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.signalRService.stopConnection().catch(err => 
      console.error('Error stopping SignalR connection:', err)
    );
  }

  /**
   * Initialize SignalR connection and subscribe to events
   */
  private async initializeSignalR(): Promise<void> {
    try {
      // Start SignalR connection
      await this.signalRService.startConnection();
      this.toastr.success('Real-time updates connected', 'Connected');

      // Subscribe to task updates
      this.signalRService.taskUpdated$
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (event: TaskUpdatedEvent) => {
            this.handleTaskUpdated(event);
          },
          error: (error) => {
            console.error('Error in taskUpdated subscription:', error);
          }
        });

      // Subscribe to chat messages
      this.signalRService.chatMessageReceived$
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (event: ChatMessageReceivedEvent) => {
            this.handleChatMessageReceived(event);
          },
          error: (error) => {
            console.error('Error in chatMessageReceived subscription:', error);
          }
        });

      // Subscribe to connection state changes
      this.signalRService.connectionState$
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (state) => {
            console.log('SignalR connection state:', state);
          }
        });
    } catch (error) {
      console.error('Failed to initialize SignalR:', error);
      this.toastr.warning('Real-time updates unavailable', 'Connection Issue');
    }
  }

  /**
   * Handle task updated event from SignalR
   */
  private handleTaskUpdated(event: TaskUpdatedEvent): void {
    console.log('Handling task update:', event);
    
    // Find and update the task in the local array
    const taskIndex = this.tasks.findIndex(t => t.id === event.taskId);
    if (taskIndex !== -1) {
      // Update existing task
      const updatedTask = {
        ...this.tasks[taskIndex],
        title: event.title,
        description: event.description,
        status: event.status,
        assignedToId: event.assigneeId,
        dueDate: event.dueDate,
      };
      this.tasks[taskIndex] = updatedTask;
      
      this.toastr.info(
        `Task "${event.title}" was updated`,
        'Task Updated',
        { timeOut: 5000 }
      );
    } else {
      // Task not in current list, reload to get it
      this.loadTasks();
      this.toastr.info(
        `New task detected`,
        'Task Created',
        { timeOut: 5000 }
      );
    }
  }

  /**
   * Handle chat message received event from SignalR
   */
  private handleChatMessageReceived(event: ChatMessageReceivedEvent): void {
    console.log('Handling chat message:', event);
    
    // Show notification
    this.toastr.info(
      event.content,
      `Message from ${event.senderName}`,
      { 
        timeOut: 5000,
        enableHtml: true,
        closeButton: true
      }
    );

    // TODO: Update chat messages array when chat feature is implemented
    // For now, just log it
    console.log('Chat message received:', {
      sender: event.senderName,
      content: event.content,
      timestamp: event.timestamp
    });
  }

  private initTaskForm(): void {
    this.taskForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(3)]],
      description: [''],
      status: [TaskStatus.Todo, Validators.required],
      projectId: ['', Validators.required],
      assignedToId: [''],
      dueDate: [''],
    });
  }

  private initProjectForm(): void {
    this.projectForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      description: [''],
      status: [ProjectStatus.Planned, Validators.required],
      startDate: [''],
      endDate: [''],
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
        projectId: task.projectId,
        assignedToId: task.assignedToId,
        dueDate: task.dueDate ? new Date(task.dueDate) : null,
      });
    } else {
      this.taskForm.reset({
        status: TaskStatus.Todo,
      });
    }

    this.showTaskDialog = true;
  }

  openProjectDialog(): void {
    this.projectForm.reset({
      status: ProjectStatus.Planned,
    });
    this.showProjectDialog = true;
  }

  closeProjectDialog(): void {
    this.showProjectDialog = false;
    this.projectForm.reset();
  }

  onSubmitProject(): void {
    if (this.projectForm.invalid) {
      this.markFormGroupTouched(this.projectForm);
      return;
    }

    const projectData = this.projectForm.value;
    this.createProject(projectData);
  }

  private createProject(projectData: any): void {
    this.projectService.createProject(projectData).subscribe({
      next: (project) => {
        console.log('Project created:', project);
        this.loadProjects();
        this.closeProjectDialog();
      },
      error: (error) => {
        console.error('Error creating project:', error);
        this.errorMessage = 'Failed to create project';
      },
    });
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
      [TaskStatus.Done]: 'success',
      [TaskStatus.Blocked]: 'warn',
    };
    return colors[status] || 'default';
  }

  getProjectStatusColor(status: ProjectStatus): string {
    const colors: Record<ProjectStatus, string> = {
      [ProjectStatus.Planned]: 'default',
      [ProjectStatus.Active]: 'primary',
      [ProjectStatus.Completed]: 'success',
      [ProjectStatus.Cancelled]: 'warn',
    };
    return colors[status] || 'default';
  }

  getTaskStatusName(status: TaskStatus): string {
    const names: Record<TaskStatus, string> = {
      [TaskStatus.Todo]: 'To Do',
      [TaskStatus.InProgress]: 'In Progress',
      [TaskStatus.Done]: 'Done',
      [TaskStatus.Blocked]: 'Blocked',
    };
    return names[status] || 'Unknown';
  }

  getProjectStatusName(status: ProjectStatus): string {
    const names: Record<ProjectStatus, string> = {
      [ProjectStatus.Planned]: 'Planned',
      [ProjectStatus.Active]: 'Active',
      [ProjectStatus.Completed]: 'Completed',
      [ProjectStatus.Cancelled]: 'Cancelled',
    };
    return names[status] || 'Unknown';
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

  get projectName() {
    return this.projectForm.get('name');
  }
}
