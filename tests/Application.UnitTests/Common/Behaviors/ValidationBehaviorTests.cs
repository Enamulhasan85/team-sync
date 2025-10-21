using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using Template.Application.Common.Behaviors;
using Xunit;

namespace Application.UnitTests.Common.Behaviors
{
    /// <summary>
    /// Unit tests for ValidationBehavior
    /// This is CRITICAL as it affects ALL requests in the application
    /// </summary>
    public class ValidationBehaviorTests
    {
        [Fact]
        public async Task Handle_NoValidators_ShouldCallNext()
        {
            // Arrange
            var validators = new List<IValidator<TestRequest>>();
            var behavior = new ValidationBehavior<TestRequest, string>(validators);
            var request = new TestRequest { Value = "test" };
            var nextCalled = false;

            RequestHandlerDelegate<string> next = (ct) =>
            {
                nextCalled = true;
                return Task.FromResult("Success");
            };

            // Act
            var result = await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            nextCalled.Should().BeTrue();
            result.Should().Be("Success");
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldCallNext()
        {
            // Arrange
            var mockValidator = new Mock<IValidator<TestRequest>>();
            mockValidator
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var validators = new List<IValidator<TestRequest>> { mockValidator.Object };
            var behavior = new ValidationBehavior<TestRequest, string>(validators);
            var request = new TestRequest { Value = "test" };
            var nextCalled = false;

            RequestHandlerDelegate<string> next = (ct) =>
            {
                nextCalled = true;
                return Task.FromResult("Success");
            };

            // Act
            var result = await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            nextCalled.Should().BeTrue();
            result.Should().Be("Success");
            mockValidator.Verify(v => v.ValidateAsync(
                It.IsAny<ValidationContext<TestRequest>>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidRequest_ShouldThrowValidationException()
        {
            // Arrange
            var validationFailure = new ValidationFailure("Value", "Value is required");
            var mockValidator = new Mock<IValidator<TestRequest>>();
            mockValidator
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[] { validationFailure }));

            var validators = new List<IValidator<TestRequest>> { mockValidator.Object };
            var behavior = new ValidationBehavior<TestRequest, string>(validators);
            var request = new TestRequest { Value = "" };

            RequestHandlerDelegate<string> next = (ct) => Task.FromResult("Success");

            // Act
            Func<Task> act = async () => await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Value is required*");
        }

        [Fact]
        public async Task Handle_MultipleValidators_ShouldAggregateErrors()
        {
            // Arrange
            var mockValidator1 = new Mock<IValidator<TestRequest>>();
            mockValidator1
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[]
                {
                    new ValidationFailure("Value", "Error 1")
                }));

            var mockValidator2 = new Mock<IValidator<TestRequest>>();
            mockValidator2
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[]
                {
                    new ValidationFailure("Value", "Error 2")
                }));

            var validators = new List<IValidator<TestRequest>> { mockValidator1.Object, mockValidator2.Object };
            var behavior = new ValidationBehavior<TestRequest, string>(validators);
            var request = new TestRequest { Value = "" };

            RequestHandlerDelegate<string> next = (ct) => Task.FromResult("Success");

            // Act
            Func<Task> act = async () => await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            var exception = await act.Should().ThrowAsync<ValidationException>();
            exception.Which.Errors.Should().HaveCount(2);
            exception.Which.Errors.Should().Contain(e => e.ErrorMessage == "Error 1");
            exception.Which.Errors.Should().Contain(e => e.ErrorMessage == "Error 2");
        }

        [Fact]
        public async Task Handle_MultipleErrors_ShouldIncludeAll()
        {
            // Arrange
            var mockValidator = new Mock<IValidator<TestRequest>>();
            mockValidator
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[]
                {
                    new ValidationFailure("Value", "Error 1"),
                    new ValidationFailure("Name", "Error 2"),
                    new ValidationFailure("Email", "Error 3")
                }));

            var validators = new List<IValidator<TestRequest>> { mockValidator.Object };
            var behavior = new ValidationBehavior<TestRequest, string>(validators);
            var request = new TestRequest { Value = "" };

            RequestHandlerDelegate<string> next = (ct) => Task.FromResult("Success");

            // Act
            Func<Task> act = async () => await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            var exception = await act.Should().ThrowAsync<ValidationException>();
            exception.Which.Errors.Should().HaveCount(3);
        }
    }

    // Test request class
    public class TestRequest : IRequest<string>
    {
        public string Value { get; set; } = string.Empty;
    }
}
