using System.Threading;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Template.Application.Common.Behaviors;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;

namespace Application.UnitTests.Common.Behaviors
{
    /// <summary>
    /// CRITICAL TESTS - LoggingBehavior affects ALL requests in the pipeline
    /// </summary>
    public class LoggingBehaviorTests
    {
        private readonly Mock<ILogger<LoggingBehavior<TestRequest, TestResponse>>> _mockLogger;
        private readonly LoggingBehavior<TestRequest, TestResponse> _behavior;

        public LoggingBehaviorTests()
        {
            _mockLogger = new Mock<ILogger<LoggingBehavior<TestRequest, TestResponse>>>();
            _behavior = new LoggingBehavior<TestRequest, TestResponse>(_mockLogger.Object);
        }

        [Fact]
        public async TaskAlias Handle_ShouldLogRequestStart()
        {
            // Arrange
            var request = new TestRequest { Value = "test" };
            var expectedResponse = new TestResponse { Result = "success" };
            RequestHandlerDelegate<TestResponse> next = (ct) => TaskAlias.FromResult(expectedResponse);

            // Act
            await _behavior.Handle(request, next, CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling TestRequest")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once,
                "Should log when request handling starts");
        }

        [Fact]
        public async TaskAlias Handle_ShouldLogRequestComplete()
        {
            // Arrange
            var request = new TestRequest { Value = "test" };
            var expectedResponse = new TestResponse { Result = "success" };
            RequestHandlerDelegate<TestResponse> next = (ct) => TaskAlias.FromResult(expectedResponse);

            // Act
            await _behavior.Handle(request, next, CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handled TestRequest successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once,
                "Should log when request handling completes successfully");
        }

        [Fact]
        public async TaskAlias Handle_ShouldLogExecutionTime()
        {
            // Arrange
            var request = new TestRequest { Value = "test" };
            var expectedResponse = new TestResponse { Result = "success" };
            RequestHandlerDelegate<TestResponse> next = async (ct) =>
            {
                await TaskAlias.Delay(100, ct); // Simulate some work
                return expectedResponse;
            };

            // Act
            await _behavior.Handle(request, next, CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ms")), // Contains milliseconds
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce,
                "Should log execution time in milliseconds");
        }

        [Fact]
        public async TaskAlias Handle_ShouldCallNext()
        {
            // Arrange
            var request = new TestRequest { Value = "test" };
            var expectedResponse = new TestResponse { Result = "success" };
            var nextCalled = false;
            RequestHandlerDelegate<TestResponse> next = (ct) =>
            {
                nextCalled = true;
                return TaskAlias.FromResult(expectedResponse);
            };

            // Act
            var result = await _behavior.Handle(request, next, CancellationToken.None);

            // Assert
            nextCalled.Should().BeTrue("Next delegate must be called to continue pipeline");
            result.Should().Be(expectedResponse, "Should return the response from next delegate");
        }

        [Fact]
        public async TaskAlias Handle_WhenExceptionThrown_ShouldLogError()
        {
            // Arrange
            var request = new TestRequest { Value = "test" };
            var expectedException = new InvalidOperationException("Test error");
            RequestHandlerDelegate<TestResponse> next = (ct) => throw expectedException;

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _behavior.Handle(request, next, CancellationToken.None));

            // Verify error was logged
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error handling TestRequest")),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once,
                "Should log error with exception details");
        }

        [Fact]
        public async TaskAlias Handle_WhenExceptionThrown_ShouldLogExecutionTimeBeforeError()
        {
            // Arrange
            var request = new TestRequest { Value = "test" };
            var expectedException = new InvalidOperationException("Test error");
            RequestHandlerDelegate<TestResponse> next = async (ct) =>
            {
                await TaskAlias.Delay(10, ct); // Simulate some work before error
                throw expectedException;
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _behavior.Handle(request, next, CancellationToken.None));

            // Verify error log contains execution time
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ms")), // Contains milliseconds
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once,
                "Should log execution time even when error occurs");
        }

        [Fact]
        public async TaskAlias Handle_ShouldLogRequestName()
        {
            // Arrange
            var request = new TestRequest { Value = "test" };
            var expectedResponse = new TestResponse { Result = "success" };
            RequestHandlerDelegate<TestResponse> next = (ct) => TaskAlias.FromResult(expectedResponse);

            // Act
            await _behavior.Handle(request, next, CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TestRequest")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeast(2), // Once at start, once at completion
                "Should log the request type name");
        }

        // Test request/response classes
        public class TestRequest : IRequest<TestResponse>
        {
            public string Value { get; set; } = string.Empty;
        }

        public class TestResponse
        {
            public string Result { get; set; } = string.Empty;
        }
    }
}
