using Bogus;
using FluentAssertions;
using MongoDB.Bson;
using Moq;
using System.Threading;
using Template.Application.Common.Interfaces;
using Template.Application.Common.Models;
using Template.Application.Features.Projects.Commands;
using Template.Domain.Entities;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;

namespace Application.UnitTests.Features.Projects.Commands
{
    /// <summary>
    /// Tests for DeleteProjectCommandHandler - Core business logic for project deletion
    /// </summary>
    public class DeleteProjectCommandHandlerTests
    {
        private readonly Mock<IRepository<Project, ObjectId>> _mockRepository;
        private readonly DeleteProjectCommandHandler _handler;
        private readonly Faker _faker;

        public DeleteProjectCommandHandlerTests()
        {
            _mockRepository = new Mock<IRepository<Project, ObjectId>>();
            _handler = new DeleteProjectCommandHandler(_mockRepository.Object);
            _faker = new Faker();
        }

        [Fact]
        public async TaskAlias Handle_ValidId_ShouldDeleteProject()
        {
            // Arrange
            var projectId = ObjectId.GenerateNewId();
            var command = new DeleteProjectCommand
            {
                Id = projectId.ToString()
            };

            var existingProject = new Project
            {
                Id = projectId,
                Name = _faker.Company.CompanyName(),
                Status = ProjectStatus.Active,
                MemberIds = new List<ObjectId>()
            };

            _mockRepository
                .Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProject);

            _mockRepository
                .Setup(x => x.DeleteAsync(projectId, It.IsAny<CancellationToken>()))
                .Returns(TaskAlias.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeTrue();

            _mockRepository.Verify(
                x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()),
                Times.Once);
            _mockRepository.Verify(
                x => x.DeleteAsync(projectId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async TaskAlias Handle_InvalidProjectId_ShouldReturnFailure()
        {
            // Arrange
            var command = new DeleteProjectCommand
            {
                Id = "invalid-id-format"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().Contain("Invalid project ID format");

            _mockRepository.Verify(
                x => x.GetByIdAsync(It.IsAny<ObjectId>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _mockRepository.Verify(
                x => x.DeleteAsync(It.IsAny<ObjectId>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async TaskAlias Handle_NonExistentProject_ShouldReturnFailure()
        {
            // Arrange
            var projectId = ObjectId.GenerateNewId();
            var command = new DeleteProjectCommand
            {
                Id = projectId.ToString()
            };

            _mockRepository
                .Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Project?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().Contain("Project not found");

            _mockRepository.Verify(
                x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()),
                Times.Once);
            _mockRepository.Verify(
                x => x.DeleteAsync(It.IsAny<ObjectId>(), It.IsAny<CancellationToken>()),
                Times.Never,
                "Delete should not be called when project doesn't exist");
        }

        [Fact]
        public async TaskAlias Handle_ExistingProjectWithMembers_ShouldStillDelete()
        {
            // Arrange
            var projectId = ObjectId.GenerateNewId();
            var command = new DeleteProjectCommand
            {
                Id = projectId.ToString()
            };

            var existingProject = new Project
            {
                Id = projectId,
                Name = _faker.Company.CompanyName(),
                Status = ProjectStatus.Active,
                MemberIds = new List<ObjectId>
                {
                    ObjectId.GenerateNewId(),
                    ObjectId.GenerateNewId(),
                    ObjectId.GenerateNewId()
                }
            };

            _mockRepository
                .Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProject);

            _mockRepository
                .Setup(x => x.DeleteAsync(projectId, It.IsAny<CancellationToken>()))
                .Returns(TaskAlias.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeTrue();

            _mockRepository.Verify(
                x => x.DeleteAsync(projectId, It.IsAny<CancellationToken>()),
                Times.Once,
                "Project should be deleted even if it has members");
        }

        [Fact]
        public async TaskAlias Handle_CompletedProject_ShouldStillDelete()
        {
            // Arrange
            var projectId = ObjectId.GenerateNewId();
            var command = new DeleteProjectCommand
            {
                Id = projectId.ToString()
            };

            var existingProject = new Project
            {
                Id = projectId,
                Name = _faker.Company.CompanyName(),
                Status = ProjectStatus.Completed, // Completed project
                MemberIds = new List<ObjectId>()
            };

            _mockRepository
                .Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProject);

            _mockRepository
                .Setup(x => x.DeleteAsync(projectId, It.IsAny<CancellationToken>()))
                .Returns(TaskAlias.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeTrue();

            _mockRepository.Verify(
                x => x.DeleteAsync(projectId, It.IsAny<CancellationToken>()),
                Times.Once,
                "Completed projects should be deletable");
        }
    }
}
