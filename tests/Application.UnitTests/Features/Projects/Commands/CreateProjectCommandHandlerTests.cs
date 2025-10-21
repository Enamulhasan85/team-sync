using AutoMapper;
using FluentAssertions;
using MongoDB.Bson;
using Moq;
using Template.Application.Common.Interfaces;
using Template.Application.Common.Models;
using Template.Application.Features.Projects.Commands;
using Template.Domain.Entities;

namespace Application.UnitTests.Features.Projects.Commands
{
    public class CreateProjectCommandHandlerTests
    {
        private readonly Mock<IRepository<Project, ObjectId>> mockRepository;
        private readonly Mock<IMapper> mockMapper;
        private readonly CreateProjectCommandHandler handler;

        public CreateProjectCommandHandlerTests()
        {
            mockRepository = new Mock<IRepository<Project, ObjectId>>();
            mockMapper = new Mock<IMapper>();
            handler = new CreateProjectCommandHandler(mockRepository.Object, mockMapper.Object);
        }

        [Xunit.Fact]
        public async System.Threading.Tasks.Task Handle_ValidCommand_ShouldCreateProject()
        {
            var command = new CreateProjectCommand { Name = "Test", Description = "Test" };
            var project = new Project { Name = "Test", Description = "Test", MemberIds = new List<ObjectId>() };
            var response = new ProjectResponse { Id = ObjectId.GenerateNewId().ToString(), Name = "Test" };
            
            mockMapper.Setup(x => x.Map<Project>(command)).Returns(project);
            mockRepository.Setup(x => x.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>())).ReturnsAsync(project);
            mockMapper.Setup(x => x.Map<ProjectResponse>(project)).Returns(response);

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            mockRepository.Verify(x => x.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Moq.Times.Once);
        }
    }
}
