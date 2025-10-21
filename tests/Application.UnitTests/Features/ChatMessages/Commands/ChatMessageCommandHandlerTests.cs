using AutoMapper;
using FluentAssertions;
using MongoDB.Bson;
using Moq;
using Template.Application.Common.Interfaces;
using Template.Application.Common.Models;
using Template.Application.Features.ChatMessages.Commands;
using Xunit;
using ChatMessageEntity = Template.Domain.Entities.ChatMessage;

namespace Application.UnitTests.Features.ChatMessages.Commands
{
    /// <summary>
    /// Critical unit tests for ChatMessage Command Handlers
    /// Tests the NEW real-time chat feature with notification service
    /// </summary>
    public class ChatMessageCommandHandlerTests
    {
        private readonly Mock<IRepository<ChatMessageEntity, ObjectId>> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ICurrentUserService> _mockCurrentUserService;
        private readonly Mock<IChatNotificationService> _mockNotificationService;

        public ChatMessageCommandHandlerTests()
        {
            _mockRepository = new Mock<IRepository<ChatMessageEntity, ObjectId>>();
            _mockMapper = new Mock<IMapper>();
            _mockCacheService = new Mock<ICacheService>();
            _mockCurrentUserService = new Mock<ICurrentUserService>();
            _mockNotificationService = new Mock<IChatNotificationService>();
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateMessage_ValidCommand_ShouldCreateMessageAndSendNotification()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var projectId = ObjectId.GenerateNewId();
            var command = new CreateChatMessageCommand
            {
                ProjectId = projectId.ToString(),
                Message = "Hello team!"
            };

            var chatMessage = new ChatMessageEntity
            {
                Id = ObjectId.GenerateNewId(),
                ProjectId = projectId,
                Message = "Hello team!",
                SenderId = ObjectId.Parse(userId),
                SenderName = "John Doe"
            };

            var response = new ChatMessageResponse
            {
                Id = chatMessage.Id.ToString(),
                ProjectId = projectId.ToString(),
                Message = "Hello team!",
                SenderId = userId,
                SenderName = "John Doe"
            };

            _mockCurrentUserService.Setup(x => x.UserId).Returns(userId);
            _mockCurrentUserService.Setup(x => x.FullName).Returns("John Doe");
            _mockMapper.Setup(x => x.Map<ChatMessageEntity>(command)).Returns(chatMessage);
            _mockRepository.Setup(x => x.AddAsync(It.IsAny<ChatMessageEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chatMessage);
            _mockMapper.Setup(x => x.Map<ChatMessageResponse>(chatMessage)).Returns(response);

            var handler = new CreateChatMessageCommandHandler(
                _mockRepository.Object,
                _mockMapper.Object,
                _mockCacheService.Object,
                _mockCurrentUserService.Object,
                _mockNotificationService.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Message.Should().Be("Hello team!");
            
            // Verify notification was sent
            _mockNotificationService.Verify(x => x.SendMessageToProjectAsync(
                projectId.ToString(),
                It.IsAny<ChatMessageResponse>()), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateMessage_ShouldSetSenderIdFromCurrentUser()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var projectId = ObjectId.GenerateNewId();
            var command = new CreateChatMessageCommand
            {
                ProjectId = projectId.ToString(),
                Message = "Test message"
            };

            ChatMessageEntity capturedMessage = null!;
            _mockCurrentUserService.Setup(x => x.UserId).Returns(userId);
            _mockCurrentUserService.Setup(x => x.UserName).Returns("testuser");
            _mockMapper.Setup(x => x.Map<ChatMessageEntity>(command))
                .Returns(new ChatMessageEntity { ProjectId = projectId, Message = "Test message" });
            _mockRepository.Setup(x => x.AddAsync(It.IsAny<ChatMessageEntity>(), It.IsAny<CancellationToken>()))
                .Callback<ChatMessageEntity, CancellationToken>((msg, ct) => capturedMessage = msg)
                .ReturnsAsync((ChatMessageEntity msg, CancellationToken ct) => msg);
            _mockMapper.Setup(x => x.Map<ChatMessageResponse>(It.IsAny<ChatMessageEntity>()))
                .Returns(new ChatMessageResponse());

            var handler = new CreateChatMessageCommandHandler(
                _mockRepository.Object,
                _mockMapper.Object,
                _mockCacheService.Object,
                _mockCurrentUserService.Object,
                _mockNotificationService.Object);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            capturedMessage.Should().NotBeNull();
            capturedMessage.SenderId.ToString().Should().Be(userId);
            capturedMessage.SenderName.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateMessage_NotOwnMessage_ShouldReturnFailure()
        {
            // Arrange
            var messageId = ObjectId.GenerateNewId();
            var ownerId = ObjectId.GenerateNewId().ToString();
            var currentUserId = ObjectId.GenerateNewId().ToString();

            var command = new UpdateChatMessageCommand
            {
                Id = messageId.ToString(),
                Message = "Updated message"
            };

            var existingMessage = new ChatMessageEntity
            {
                Id = messageId,
                SenderId = ObjectId.Parse(ownerId),
                Message = "Original message"
            };

            _mockCurrentUserService.Setup(x => x.UserId).Returns(currentUserId);
            _mockRepository.Setup(x => x.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingMessage);

            var handler = new UpdateChatMessageCommandHandler(
                _mockRepository.Object,
                _mockMapper.Object,
                _mockCacheService.Object,
                _mockCurrentUserService.Object,
                _mockNotificationService.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().Contain("You can only edit your own messages");
            
            // Verify message was NOT updated
            _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<ChatMessageEntity>(), It.IsAny<CancellationToken>()), Times.Never);
            
            // Verify notification was NOT sent
            _mockNotificationService.Verify(x => x.SendUpdatedMessageToProjectAsync(
                It.IsAny<string>(), It.IsAny<ChatMessageResponse>()), Times.Never);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateMessage_ValidCommand_ShouldUpdateAndSendNotification()
        {
            // Arrange
            var messageId = ObjectId.GenerateNewId();
            var userId = ObjectId.GenerateNewId().ToString();
            var projectId = ObjectId.GenerateNewId();

            var command = new UpdateChatMessageCommand
            {
                Id = messageId.ToString(),
                Message = "Updated message"
            };

            var existingMessage = new ChatMessageEntity
            {
                Id = messageId,
                SenderId = ObjectId.Parse(userId),
                ProjectId = projectId,
                Message = "Original message"
            };

            var response = new ChatMessageResponse
            {
                Id = messageId.ToString(),
                Message = "Updated message"
            };

            _mockCurrentUserService.Setup(x => x.UserId).Returns(userId);
            _mockRepository.Setup(x => x.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingMessage);
            _mockMapper.Setup(x => x.Map<ChatMessageResponse>(It.IsAny<ChatMessageEntity>()))
                .Returns(response);

            var handler = new UpdateChatMessageCommandHandler(
                _mockRepository.Object,
                _mockMapper.Object,
                _mockCacheService.Object,
                _mockCurrentUserService.Object,
                _mockNotificationService.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            _mockRepository.Verify(x => x.UpdateAsync(existingMessage, It.IsAny<CancellationToken>()), Times.Once);
            _mockNotificationService.Verify(x => x.SendUpdatedMessageToProjectAsync(
                projectId.ToString(), It.IsAny<ChatMessageResponse>()), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task DeleteMessage_NotOwnMessage_ShouldReturnFailure()
        {
            // Arrange
            var messageId = ObjectId.GenerateNewId();
            var ownerId = ObjectId.GenerateNewId().ToString();
            var currentUserId = ObjectId.GenerateNewId().ToString();

            var command = new DeleteChatMessageCommand { Id = messageId.ToString() };

            var existingMessage = new ChatMessageEntity
            {
                Id = messageId,
                SenderId = ObjectId.Parse(ownerId),
                ProjectId = ObjectId.GenerateNewId(),
                Message = "Original"
            };

            _mockCurrentUserService.Setup(x => x.UserId).Returns(currentUserId);
            _mockRepository.Setup(x => x.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingMessage);

            var handler = new DeleteChatMessageCommandHandler(
                _mockRepository.Object,
                _mockCurrentUserService.Object,
                _mockCacheService.Object,
                _mockNotificationService.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().Contain("You can only delete your own messages");
            _mockRepository.Verify(x => x.DeleteAsync(It.IsAny<ObjectId>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async System.Threading.Tasks.Task DeleteMessage_ValidCommand_ShouldDeleteAndSendNotification()
        {
            // Arrange
            var messageId = ObjectId.GenerateNewId();
            var userId = ObjectId.GenerateNewId().ToString();
            var projectId = ObjectId.GenerateNewId();

            var command = new DeleteChatMessageCommand { Id = messageId.ToString() };

            var existingMessage = new ChatMessageEntity
            {
                Id = messageId,
                SenderId = ObjectId.Parse(userId),
                ProjectId = projectId,
                Message = "To be deleted"
            };

            _mockCurrentUserService.Setup(x => x.UserId).Returns(userId);
            _mockRepository.Setup(x => x.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingMessage);

            var handler = new DeleteChatMessageCommandHandler(
                _mockRepository.Object,
                _mockCurrentUserService.Object,
                _mockCacheService.Object,
                _mockNotificationService.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            _mockRepository.Verify(x => x.DeleteAsync(messageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockNotificationService.Verify(x => x.SendDeletedMessageToProjectAsync(
                projectId.ToString(), messageId.ToString()), Times.Once);
        }
    }
}
