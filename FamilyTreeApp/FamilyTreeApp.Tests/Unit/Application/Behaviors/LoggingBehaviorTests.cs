using FamilyTreeApp.Application.Common.Behaviors;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FamilyTreeApp.Tests.Unit.Application.Behaviors;

public class LoggingBehaviorTests
{
    public record TestCommand(string SecretPayload) : IRequest<string>;

    [Fact]
    public async Task HandleAsync_OnSuccess_LogsAtInformationLevel()
    {
        // Arrange
        ICommandHandler<TestCommand, string> innerHandler = Substitute.For<ICommandHandler<TestCommand, string>>();
        ILogger<LoggingBehavior<TestCommand, string>> logger = Substitute.For<ILogger<LoggingBehavior<TestCommand, string>>>();

        var command = new TestCommand("SecretData");
        innerHandler.HandleAsync(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success("SuccessValue")));

        var behavior = new LoggingBehavior<TestCommand, string>(innerHandler, logger);

        // Act
        Result<string> result = await behavior.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v != null && v.ToString()!.Contains(nameof(TestCommand))),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task HandleAsync_OnFailure_LogsAtWarningLevel()
    {
        // Arrange
        ICommandHandler<TestCommand, string> innerHandler = Substitute.For<ICommandHandler<TestCommand, string>>();
        ILogger<LoggingBehavior<TestCommand, string>> logger = Substitute.For<ILogger<LoggingBehavior<TestCommand, string>>>();

        var command = new TestCommand("SecretData");
        var error = new Error("Test.Error", "Something went wrong", ErrorType.Validation);
        innerHandler.HandleAsync(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure<string>(error)));

        var behavior = new LoggingBehavior<TestCommand, string>(innerHandler, logger);

        // Act
        Result<string> result = await behavior.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v != null && v.ToString()!.Contains("Test.Error") && v.ToString()!.Contains("Something went wrong")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task HandleAsync_NeverLogsRequestPayload()
    {
        // Arrange
        ICommandHandler<TestCommand, string> innerHandler = Substitute.For<ICommandHandler<TestCommand, string>>();
        ILogger<LoggingBehavior<TestCommand, string>> logger = Substitute.For<ILogger<LoggingBehavior<TestCommand, string>>>();

        var command = new TestCommand("SuperSecret123");
        innerHandler.HandleAsync(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success("SuccessValue")));

        var behavior = new LoggingBehavior<TestCommand, string>(innerHandler, logger);

        // Act
        await behavior.HandleAsync(command, CancellationToken.None);

        // Assert
        logger.DidNotReceive().Log(
            Arg.Any<LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v != null && v.ToString()!.Contains("SuperSecret123")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
