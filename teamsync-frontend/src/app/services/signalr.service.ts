import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject, BehaviorSubject } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

export interface TaskUpdatedEvent {
  taskId: string;
  projectId: string;
  title: string;
  description?: string;
  status: number;
  assigneeId?: string;
  dueDate?: Date;
  lastModifiedBy?: string;
  changedFields: string[];
}

export interface ChatMessageReceivedEvent {
  id: string;
  senderId: string;
  senderName: string;
  receiverId: string;
  content: string;
  timestamp: Date;
  isRead: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class SignalRService {
  private readonly authService = inject(AuthService);
  private hubConnection?: signalR.HubConnection;
  private readonly hubUrl = `${environment.apiBaseUrl}/hubs/notifications`;

  // RxJS Subjects for events
  private taskUpdatedSubject = new Subject<TaskUpdatedEvent>();
  private chatMessageReceivedSubject = new Subject<ChatMessageReceivedEvent>();
  private connectionStateSubject = new BehaviorSubject<signalR.HubConnectionState>(
    signalR.HubConnectionState.Disconnected
  );

  // Public observables
  public taskUpdated$ = this.taskUpdatedSubject.asObservable();
  public chatMessageReceived$ = this.chatMessageReceivedSubject.asObservable();
  public connectionState$ = this.connectionStateSubject.asObservable();

  constructor() {
    console.log('SignalRService initialized');
  }

  /**
   * Start the SignalR connection
   */
  public async startConnection(): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      console.log('SignalR already connected');
      return;
    }

    try {
      // Get the authentication token
      const token = this.authService.getToken();
      if (!token) {
        console.warn('No authentication token found. Cannot connect to SignalR hub.');
        return;
      }

      // Build the hub connection
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(this.hubUrl, {
          accessTokenFactory: () => token,
          skipNegotiation: false,
          transport:
            signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents,
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            // Exponential backoff: 0s, 2s, 10s, 30s, then 60s
            if (retryContext.previousRetryCount === 0) return 0;
            if (retryContext.previousRetryCount === 1) return 2000;
            if (retryContext.previousRetryCount === 2) return 10000;
            if (retryContext.previousRetryCount === 3) return 30000;
            return 60000;
          },
        })
        .configureLogging(signalR.LogLevel.Information)
        .build();

      // Register event handlers
      this.registerEventHandlers();

      // Start the connection
      await this.hubConnection.start();
      console.log('SignalR connection established successfully');
      this.connectionStateSubject.next(this.hubConnection.state);
    } catch (error) {
      console.error('Error starting SignalR connection:', error);
      this.connectionStateSubject.next(signalR.HubConnectionState.Disconnected);
      throw error;
    }
  }

  /**
   * Stop the SignalR connection
   */
  public async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      try {
        await this.hubConnection.stop();
        console.log('SignalR connection stopped');
        this.connectionStateSubject.next(signalR.HubConnectionState.Disconnected);
      } catch (error) {
        console.error('Error stopping SignalR connection:', error);
      }
    }
  }

  /**
   * Register event handlers for SignalR hub methods
   */
  private registerEventHandlers(): void {
    if (!this.hubConnection) return;

    // Handle ReceiveTaskNotification event from backend
    this.hubConnection.on('ReceiveTaskNotification', (event: any) => {
      console.log('ReceiveTaskNotification event received:', event);
      // Transform backend event to TaskUpdatedEvent format
      const taskUpdatedEvent: TaskUpdatedEvent = {
        taskId: event.taskId,
        projectId: event.projectId,
        title: event.title,
        description: event.description,
        status: event.status,
        assigneeId: event.assigneeId,
        dueDate: event.dueDate,
        lastModifiedBy: event.lastModifiedBy,
        changedFields: event.changedFields || [],
      };
      this.taskUpdatedSubject.next(taskUpdatedEvent);
    });

    // Handle NewMessage event (Chat messages) from backend
    this.hubConnection.on('NewMessage', (event: ChatMessageReceivedEvent) => {
      console.log('NewMessage event received:', event);
      this.chatMessageReceivedSubject.next(event);
    });

    // Handle connection lifecycle events
    this.hubConnection.onreconnecting((error) => {
      console.warn('SignalR connection lost. Reconnecting...', error);
      this.connectionStateSubject.next(signalR.HubConnectionState.Reconnecting);
    });

    this.hubConnection.onreconnected((connectionId) => {
      console.log('SignalR reconnected. Connection ID:', connectionId);
      this.connectionStateSubject.next(signalR.HubConnectionState.Connected);
    });

    this.hubConnection.onclose((error) => {
      console.error('SignalR connection closed', error);
      this.connectionStateSubject.next(signalR.HubConnectionState.Disconnected);
    });
  }

  /**
   * Get the current connection state
   */
  public getConnectionState(): signalR.HubConnectionState {
    return this.hubConnection?.state ?? signalR.HubConnectionState.Disconnected;
  }

  /**
   * Check if connected
   */
  public isConnected(): boolean {
    return this.hubConnection?.state === signalR.HubConnectionState.Connected;
  }

  /**
   * Invoke a hub method (for sending data to the server)
   */
  public async invokeHubMethod<T>(methodName: string, ...args: any[]): Promise<T> {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      throw new Error('SignalR connection is not established');
    }

    try {
      return await this.hubConnection.invoke<T>(methodName, ...args);
    } catch (error) {
      console.error(`Error invoking hub method '${methodName}':`, error);
      throw error;
    }
  }
}
