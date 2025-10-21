using System.Threading;
using AutoMapper;
using Bogus;
using FluentAssertions;
using MongoDB.Bson;
using Moq;
using Template.Application.Common.Interfaces;
using Template.Application.Common.Models;
using Template.Application.Features.Projects.Commands;
using Template.Domain.Entities;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;

namespace Application.UnitTests.Features.Projects.Commands
{
    /// <summary>
    /// Tests for UpdateProjectCommandHandler - Core business logic for project updates
    /// </summary>
    public class UpdateProjectCommandHandlerTests
    {
        private readonly Mock<IRepository<Project, ObjectId>> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly UpdateProjectCommandHandler _handler;
        private readonly Faker _faker;

        public UpdateProjectCommandHandlerTests()
        {
            _mockRepository = new Mock<IRepository<Project, ObjectId>>();
            _mockMapper = new Mock<IMapper>();
            _handler = new UpdateProjectCommandHandler(_mockRepository.Object, _mockMapper.Object);
            _faker = new Faker();
        }

        [Fact]
        public async TaskAlias Handle_ValidCommand_ShouldUpdateProject()
        {
            // Arrange
            var projectId = ObjectId.GenerateNewId();
            var command = new UpdateProjectCommand
            {
                Id = projectId.ToString(),
                Name = _faker.Company.CompanyName(),
                Description = _faker.Lorem.Sentence(),
                Status = ProjectStatus.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(3)
            };

            var existingProject = new Project
            {
                Id = projectId,
                Name = "Old Name",
                Description = "Old Description",
                Status = ProjectStatus.Planned,
                MemberIds = new List<ObjectId>()
            };

            var response = new ProjectResponse
            {
                Id = projectId.ToString(),
                Name = command.Name,
                Description = command.Description,
                Status = command.Status
            };

            _mockRepository
                .Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProject);

            _mockMapper
                .Setup(x => x.Map(command, existingProject))
                .Callback<UpdateProjectCommand, Project>((cmd, proj) =>
                {
                    proj.Name = cmd.Name;
                    proj.Description = cmd.Description;
                    proj.Status = cmd.Status;
                });

            _mockRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
                .Returns(TaskAlias.CompletedTask);

            _mockMapper
                .Setup(x => x.Map<ProjectResponse>(It.IsAny<Project>()))
                .Returns(response);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Name.Should().Be(command.Name);
            result.Value.Description.Should().Be(command.Description);

            _mockRepository.Verify(
                x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()),
                Times.Once);
            _mockRepository.Verify(
                x => x.UpdateAsync(It.Is<Project>(p => p.Id == projectId), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async TaskAlias Handle_InvalidProjectId_ShouldReturnFailure()
        {
            // Arrange
            var command = new UpdateProjectCommand
            {
                Id = "invalid-id-format",
                Name = _faker.Company.CompanyName(),
                Description = _faker.Lorem.Sentence()
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
        }

        [Fact]
        public async TaskAlias Handle_NonExistentProject_ShouldReturnFailure()
        {
            // Arrange
            var projectId = ObjectId.GenerateNewId();
            var command = new UpdateProjectCommand
            {
                Id = projectId.ToString(),
                Name = _faker.Company.CompanyName(),
                Description = _faker.Lorem.Sentence()
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
                x => x.UpdateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async TaskAlias Handle_ValidCommand_ShouldMapCorrectly()
        {
            // Arrange
            var projectId = ObjectId.GenerateNewId();
            var command = new UpdateProjectCommand
            {
                Id = projectId.ToString(),
                Name = _faker.Company.CompanyName(),
                Description = _faker.Lorem.Sentence(),
                Status = ProjectStatus.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(3)
            };

            var existingProject = new Project
            {
                Id = projectId,
                Name = "Old Name",
                MemberIds = new List<ObjectId>()
            };

            var response = new ProjectResponse
            {
                Id = projectId.ToString(),
                Name = command.Name
            };

            _mockRepository
                .Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProject);

            _mockRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
                .Returns(TaskAlias.CompletedTask);

            _mockMapper
                .Setup(x => x.Map<ProjectResponse>(It.IsAny<Project>()))
                .Returns(response);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();

            // Verify mapper was called with the command and existing project
            _mockMapper.Verify(
                x => x.Map(command, existingProject),
                Times.Once,
                "Mapper should map command properties to existing project entity");

            // Verify mapper was called to create response
            _mockMapper.Verify(
                x => x.Map<ProjectResponse>(existingProject),
                Times.Once,
                "Mapper should convert updated project to response DTO");
        }

        [Fact]
        public async TaskAlias Handle_StatusChange_ShouldUpdateStatus()
        {
            // Arrange
            var projectId = ObjectId.GenerateNewId();
            var command = new UpdateProjectCommand
            {
                Id = projectId.ToString(),
                Name = _faker.Company.CompanyName(),
                Status = ProjectStatus.Completed // Status change
            };

            var existingProject = new Project
            {
                Id = projectId,
                Name = "Test Project",
                Status = ProjectStatus.Active, // Original status
                MemberIds = new List<ObjectId>()
            };

            var response = new ProjectResponse
            {
                Id = projectId.ToString(),
                Name = command.Name,
                Status = ProjectStatus.Completed
            };

            _mockRepository
                .Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProject);

            _mockMapper
                .Setup(x => x.Map(command, existingProject))
                .Callback<UpdateProjectCommand, Project>((cmd, proj) => proj.Status = cmd.Status);

            _mockRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
                .Returns(TaskAlias.CompletedTask);

            _mockMapper
                .Setup(x => x.Map<ProjectResponse>(It.IsAny<Project>()))
                .Returns(response);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value!.Status.Should().Be(ProjectStatus.Completed);

            _mockRepository.Verify(
                x => x.UpdateAsync(It.Is<Project>(p => p.Status == ProjectStatus.Completed), It.IsAny<CancellationToken>()),
                Times.Once,
                "Project status should be updated in the database");
        }
    }
}
