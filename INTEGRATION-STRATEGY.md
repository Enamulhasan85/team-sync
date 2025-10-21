# Redis, RabbitMQ & SignalR Integration Strategy

**Brief Note: Team Sync Integration Implementation**

This document explains how Redis caching, RabbitMQ messaging, and SignalR real-time communication are integrated in the Team Sync application, including the rationale behind cache key design and invalidation strategies.

### Real-Time Event Flow

```
User Action → API Controller → MediatR Command Handler
                                         ↓
                        ┌────────────────┴────────────────┐
                        ↓                                 ↓
                    MongoDB                         RabbitMQ
                  (Persistence)                   (Event Queue)
                                                        ↓
                                              Background Consumer
                                                        ↓
                                                  SignalR Hub
                                                        ↓
                                              Connected Clients
```

## 1. Redis Caching Implementation

### Purpose

Redis is used as a distributed cache to reduce MongoDB database load and improve API response times by caching frequently-read data.

### Cache Key Design & Implementation

**Version-Based Invalidation Pattern:**

- **Cache Keys**: `chatmessages:project:{projectId}:{version}:latest20` and `tasks:project:{projectId}:{version}`
- **Version Keys**: `chatmessages:v:project:{projectId}`, `tasks:v:project:{projectId}`, `tasks:v:status:{status}`, `tasks:v:assignee:{assigneeId}`, `tasks:v:global`

### How It Works

**Read Operation (Query Handlers):**

```csharp
// 1. Get current version number
var version = await _cacheService.GetAsync<int>($"chatmessages:v:project:{projectId}");

// 2. Check cache with version
var cacheKey = $"chatmessages:project:{projectId}:{version}:latest20";
var cachedData = await _cacheService.GetAsync<PaginatedResult<ChatMessageDto>>(cacheKey);

// 3. If cache hit, return immediately
if (cachedData != null) return cachedData;

// 4. If cache miss, query MongoDB and cache result
var data = await _repository.GetPaginatedAsync(...);
await _cacheService.SetAsync(cacheKey, data);
```

**Write Operation (Command Handlers):**

```csharp
// 1. Update MongoDB
var createdTask = await _repository.AddAsync(task, cancellationToken);

// 2. Increment version counters (invalidates cache)
await _cacheService.IncrementAsync($"tasks:v:project:{projectId}", 1, TimeSpan.FromDays(30));
await _cacheService.IncrementAsync($"tasks:v:status:{status}", 1, TimeSpan.FromDays(30));
await _cacheService.IncrementAsync("tasks:v:global", 1, TimeSpan.FromDays(30));

// 3. Next read will fetch fresh data (version mismatch)
```

### Why Version-Based Invalidation?

**Advantages:**

- **Atomic**: Single counter increment invalidates all related caches
- **Efficient**: No need to delete or scan multiple cache keys
- **Scalable**: Works across multiple application instances
- **Simple**: Version mismatch automatically triggers fresh fetch

**Cache Targets:**

- **Chat Messages**: Latest 20 messages per project (first page optimization)
- **Task Lists**: Paginated task lists filtered by project, status, assignee
- **Version Counters**: Long TTL (30 days) as they're rarely invalidated manually

---

## 2. RabbitMQ Event-Driven Architecture

### Purpose

RabbitMQ decouples the API layer from the notification layer, enabling asynchronous event processing without blocking HTTP responses.

### Exchange Configuration

- **Exchange Type**: Fanout (`tasks_exchange`)
- **Queue**: `task_notifications_queue` (durable, non-exclusive, non-auto-delete)
- **Routing Keys**: `task.created`, `task.updated`, `task.deleted`

### Implementation Flow

**1. Publishing Events (Application Layer):**

```csharp
// After creating/updating/deleting a task in MongoDB
var taskCreatedEvent = new TaskCreatedEvent {
    TaskId = createdTask.Id.ToString(),
    ProjectId = createdTask.ProjectId.ToString(),
    Title = createdTask.Title,
    Status = createdTask.Status,
    AssigneeId = createdTask.AssigneeId?.ToString()
};

await _eventPublisher.PublishAsync("task.created", taskCreatedEvent);
// API returns immediately after this - no waiting for notifications
```

**2. Consuming Events (Background Service):**

```csharp
// RabbitMqConsumer runs as a BackgroundService (hosted service)
consumer.ReceivedAsync += async (model, ea) => {
    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
    var routingKey = ea.RoutingKey;

    // Deserialize based on routing key
    switch (routingKey) {
        case "task.created":
            var event = JsonSerializer.Deserialize<TaskCreatedEvent>(message);
            await ProcessTaskEventAsync(event, "Task Created", ...);
            break;
        // Similar for task.updated and task.deleted
    }

    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
};
```

**3. Event Processing:**

- Fetch project members from MongoDB
- Broadcast to SignalR project group
- Create notification entity in MongoDB for each member
- Log email notification (simulated)

### Why Fanout Exchange?

**Current Design Choice:**

- Simple broadcast model - all queues receive all task events
- Single consumer processes all event types
- Suitable for current notification requirements

**Benefits:**

- Decouples API response from notification delivery (faster API)
- Guarantees event delivery (durable queue persists messages)
- Enables horizontal scaling (multiple consumer instances)
- Provides retry mechanism on failure (nack + requeue)

---

## 3. SignalR Real-Time Communication

### Purpose

SignalR provides WebSocket-based real-time updates to connected clients, enabling instant UI updates without polling.

### Hub Implementation

**Endpoint**: `/hubs/notifications` (JWT authenticated)

**Automatic Group Subscription:**

```csharp
public override async Task OnConnectedAsync() {
    var userId = _currentUserService.UserId;

    // Find all projects where user is a member
    var projects = await _projectRepository.FindAsync(
        p => p.MemberIds.Contains(userObjectId));

    // Add connection to all project groups automatically
    foreach (var project in projects) {
        var groupName = $"project_{project.Id}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    await base.OnConnectedAsync();
}
```

### Broadcasting from RabbitMQ Consumer

**Group-Based Broadcasting:**

```csharp
// RabbitMqConsumer processes event and broadcasts to project members
var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();

await hubContext.Clients.Group($"project_{taskEvent.ProjectId}")
    .SendAsync("ReceiveTaskNotification", new {
        taskEvent.TaskId,
        taskEvent.ProjectId,
        taskEvent.Title,
        notificationTitle = "Task Created",
        notificationMessage = $"A new task '{taskEvent.Title}' has been created."
    });
```

### Why Project-Based Groups?

**Design Rationale:**

- **Automatic Authorization**: Group membership = project membership
- **Targeted Broadcasting**: Only relevant users receive updates
- **Efficient**: No iteration over all connections
- **Scalable**: SignalR manages group membership efficiently

**Chat Messages:**

```csharp
// Direct SignalR notification (not via RabbitMQ)
await _chatNotificationService.SendMessageToProjectAsync(projectId, response);
// Broadcasts immediately to project_{projectId} group
```

---

## 4. Complete Integration Flow

### Example: User Creates a Task

**Step 1 - API Request Processing:**

```
POST /api/v1/tasks → CreateTaskCommand → Command Handler
```

**Step 2 - Data Persistence & Cache Invalidation:**

```csharp
// Save to MongoDB
var createdTask = await _repository.AddAsync(task);

// Increment version counters (invalidates cache)
await _cacheService.IncrementAsync($"tasks:v:project:{projectId}", 1);
await _cacheService.IncrementAsync($"tasks:v:status:{status}", 1);
await _cacheService.IncrementAsync("tasks:v:global", 1);
```

**Step 3 - Event Publishing:**

```csharp
// Publish to RabbitMQ (non-blocking)
await _eventPublisher.PublishAsync("task.created", taskCreatedEvent);

// API returns immediately
return Result<TaskResponse>.Success(response);
```

**Step 4 - Background Processing:**

```
RabbitMQ Consumer receives event → Deserializes → Processes
```

**Step 5 - Real-Time Notification:**

```csharp
// Broadcast to project members via SignalR
await hubContext.Clients.Group($"project_{projectId}")
    .SendAsync("ReceiveTaskNotification", eventData);

// Create notification entity in MongoDB
await notificationRepository.AddAsync(notification);
```

**Step 6 - Frontend Update:**

```
SignalR listener receives event → Updates UI → Shows toast notification
```

---

## 5. Why This Cache Invalidation Strategy?

### Version-Based Invalidation Benefits

**1. Atomic Operations:**

- Single increment operation invalidates all related cache entries
- No need to track or delete individual cache keys
- Prevents race conditions

**2. Cross-Instance Consistency:**

- Version counter stored in shared Redis
- All API instances see the same version
- New cache entries automatically use new version

**3. Simplicity:**

- No complex cache key scanning or deletion logic
- Version mismatch automatically triggers database fetch
- Self-healing (stale cache entries expire naturally)

**4. Granular Control:**

- Project-level invalidation: `tasks:v:project:{projectId}`
- Status-level invalidation: `tasks:v:status:{status}`
- Assignee-level invalidation: `tasks:v:assignee:{assigneeId}`
- Global invalidation: `tasks:v:global`

### Alternative Considered: Direct Key Deletion

**Why Rejected:**

- Requires tracking all cache keys to delete
- Race conditions between delete and re-cache
- Complex pattern matching for wildcards
- Doesn't work well across distributed instances

---

## Summary

This integration demonstrates a production-ready approach to building scalable, real-time collaborative systems:

- **Redis** provides version-based cache invalidation for performance
- **RabbitMQ** decouples API from notifications for responsiveness
- **SignalR** delivers instant updates via project-based groups

The architecture is **simple**, **maintainable**, and **scalable**, following enterprise best practices while avoiding over-engineering.

---

**Author:** Enamul Hasan  
**Date:** October 21, 2025
