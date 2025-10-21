using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Template.Application.Common.Events;
using Template.Application.Common.Interfaces;
using Template.Application.Common.Settings;
using Template.Domain.Entities;
using Template.Domain.Identity;
using Template.Infrastructure.Hubs;
using TaskEntity = Template.Domain.Entities.Task;

namespace Template.Infrastructure.Services;

/// <summary>
/// Background service that consumes messages from RabbitMQ
/// and processes task events for real-time notifications
/// </summary>
public class RabbitMqConsumer : BackgroundService
{
    private readonly ILogger<RabbitMqConsumer> _logger;
    private readonly RabbitMqSettings _settings;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly IServiceProvider _serviceProvider;

    public RabbitMqConsumer(
        IOptions<RabbitMqSettings> options,
        ILogger<RabbitMqConsumer> logger,
        IServiceProvider serviceProvider)
    {
        _settings = options.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await ConnectToRabbitMqAsync();

            if (_channel == null)
            {
                _logger.LogError("Failed to initialize RabbitMQ channel");
                return;
            }

            // Declare a queue for this consumer
            var queueDeclareResult = await _channel.QueueDeclareAsync(
                queue: "task_notifications_queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken
            );

            // Bind queue to exchange (fanout = receive all messages)
            await _channel.QueueBindAsync(
                queue: queueDeclareResult.QueueName,
                exchange: _settings.ExchangeName,
                routingKey: string.Empty,
                cancellationToken: stoppingToken
            );

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;

                    _logger.LogInformation("===== RabbitMQ Message Received =====");
                    _logger.LogInformation("Routing Key: {RoutingKey}", routingKey);
                    _logger.LogInformation("Message Content: {Message}", message);
                    _logger.LogInformation("=====================================");

                    // Process the message based on routing key
                    await ProcessMessageAsync(routingKey, message, stoppingToken);

                    // Acknowledge the message
                    await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    // Negative acknowledge - requeue the message
                    await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: queueDeclareResult.QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken
            );

            _logger.LogInformation("RabbitMQ consumer started and listening for messages");

            // Keep the service running
            await System.Threading.Tasks.Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in RabbitMQ consumer");
        }
    }

    private async System.Threading.Tasks.Task ConnectToRabbitMqAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            Port = _settings.Port,
            UserName = _settings.Username,
            Password = _settings.Password
        };

        try
        {
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Declare exchange
            await _channel.ExchangeDeclareAsync(
                exchange: _settings.ExchangeName,
                type: ExchangeType.Fanout,
                durable: true
            );

            _logger.LogInformation("Successfully connected to RabbitMQ");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ");
            throw;
        }
    }

    private async System.Threading.Tasks.Task ProcessMessageAsync(string routingKey, string messageJson, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing message - Routing Key: {RoutingKey}", routingKey);

            using var scope = _serviceProvider.CreateScope();
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();
            var projectRepository = scope.ServiceProvider.GetRequiredService<IRepository<Project, ObjectId>>();
            var notificationRepository = scope.ServiceProvider.GetRequiredService<IRepository<Notification, ObjectId>>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            switch (routingKey)
            {
                case "task.created":
                    var createdEvent = JsonSerializer.Deserialize<TaskCreatedEvent>(messageJson);
                    if (createdEvent != null)
                    {
                        var notificationTitle = "Task Created";
                        var notificationMessage = $"A new task '{createdEvent.Title}' has been created.";
                        await ProcessTaskEventAsync(createdEvent, notificationTitle, notificationMessage,
                            hubContext, projectRepository, notificationRepository,
                            notificationService, userManager, cancellationToken);
                    }
                    break;

                case "task.updated":
                    var updatedEvent = JsonSerializer.Deserialize<TaskUpdatedEvent>(messageJson);
                    if (updatedEvent != null)
                    {
                        var changedFieldsStr = updatedEvent.ChangedFields.Any()
                            ? $" ({string.Join(", ", updatedEvent.ChangedFields)})"
                            : "";

                        await ProcessTaskEventAsync(updatedEvent, "Task Updated",
                            $"Task '{updatedEvent.Title}' has been updated{changedFieldsStr}.",
                            hubContext, projectRepository, notificationRepository,
                            notificationService, userManager, cancellationToken);
                    }
                    break;

                case "task.deleted":
                    var deletedEvent = JsonSerializer.Deserialize<TaskDeletedEvent>(messageJson);
                    if (deletedEvent != null)
                    {
                        await ProcessTaskEventAsync(deletedEvent, "Task Deleted",
                            $"Task '{deletedEvent.Title}' has been deleted.",
                            hubContext, projectRepository, notificationRepository,
                            notificationService, userManager, cancellationToken);
                    }
                    break;

                default:
                    _logger.LogWarning("Unknown routing key: {RoutingKey}", routingKey);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message: {Message}", messageJson);
            throw;
        }
    }

    private async System.Threading.Tasks.Task ProcessTaskEventAsync(
        TaskEventBase taskEvent,
        string notificationTitle,
        string notificationMessage,
        IHubContext<NotificationHub> hubContext,
        IRepository<Project, ObjectId> projectRepository,
        IRepository<Notification, ObjectId> notificationRepository,
        INotificationService notificationService,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ObjectId.TryParse(taskEvent.ProjectId, out var projectObjectId))
            {
                _logger.LogWarning("Invalid ProjectId format: {ProjectId}", taskEvent.ProjectId);
                return;
            }

            var project = await projectRepository.GetByIdAsync(projectObjectId, cancellationToken);
            if (project == null)
            {
                _logger.LogWarning("Project not found: {ProjectId}", taskEvent.ProjectId);
                return;
            }

            _logger.LogInformation("Processing task event for project {ProjectId} with {MemberCount} members",
                taskEvent.ProjectId, project.MemberIds.Count);

            // send real-time notification via SignalR
            await hubContext.Clients.Group($"project_{taskEvent.ProjectId}")
                .SendAsync(
                    "ReceiveTaskNotification",
                    new
                    {
                        taskEvent.TaskId,
                        taskEvent.ProjectId,
                        taskEvent.Title,
                        notificationTitle,
                        notificationMessage
                    }
                );

            foreach (var memberId in project.MemberIds)
            {
                try
                {
                    var user = await userManager.FindByIdAsync(memberId.ToString());
                    if (user == null || user.IsActive == false)
                    {
                        _logger.LogWarning("User not found for memberId {UserId}", memberId);
                        continue;
                    }

                    // Create notification entity
                    var notification = new Notification(
                        recipientId: memberId,
                        title: notificationTitle,
                        message: notificationMessage
                    );

                    // Save notification to MongoDB
                    await notificationRepository.AddAsync(notification, cancellationToken);
                    _logger.LogInformation("Created notification for user {UserId}", memberId);

                    await notificationService.SendEmailAsync(user.Email!, notificationTitle, notificationMessage, cancellationToken);
                    _logger.LogInformation("Created email log for user {UserId}", memberId);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing notification for user {UserId}", memberId);
                }
            }

            _logger.LogInformation("Completed processing task event for project {ProjectId}", taskEvent.ProjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessTaskEventAsync");
            throw;
        }
    }

    public override async void Dispose()
    {
        if (_channel != null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }
        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }
        base.Dispose();
    }
}
