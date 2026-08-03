using FamilyTreeApp.Application.Common.Behaviors;
using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FamilyTreeApp.Tests.Unit.Application.Behaviors;

public class TransactionBehaviorTests
{
    public record NonTransactionalCommand(string Name) : IRequest<string>;

    public record TransactionalCommand(string Name) : IRequest<string>, ITransactionalCommand;

    [Fact]
    public async Task HandleAsync_NonTransactionalCommand_NoTransactionOpened()
    {
        // Arrange
        ICommandHandler<NonTransactionalCommand, string> innerHandler = Substitute.For<ICommandHandler<NonTransactionalCommand, string>>();
        IApplicationDbContext dbContext = Substitute.For<IApplicationDbContext>();

        var command = new NonTransactionalCommand("Test");
        innerHandler.HandleAsync(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success("Result")));

        var behavior = new TransactionBehavior<NonTransactionalCommand, string>(innerHandler, dbContext);

        // Act
        Result<string> result = await behavior.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _ = dbContext.DidNotReceive().Database;
    }

    [Fact]
    public async Task HandleAsync_TransactionalCommand_Success_CommitsTransaction()
    {
        // Arrange
        ICommandHandler<TransactionalCommand, string> innerHandler = Substitute.For<ICommandHandler<TransactionalCommand, string>>();
        IApplicationDbContext dbContext = Substitute.For<IApplicationDbContext>();
        DatabaseFacade databaseFacade = Substitute.For<DatabaseFacade>((Microsoft.EntityFrameworkCore.DbContext)null!);
        IDbContextTransaction transaction = Substitute.For<IDbContextTransaction>();

        dbContext.Database.Returns(databaseFacade);
        databaseFacade.BeginTransactionAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(transaction));

        var command = new TransactionalCommand("Test");
        innerHandler.HandleAsync(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success("Result")));

        var behavior = new TransactionBehavior<TransactionalCommand, string>(innerHandler, dbContext);

        // Act
        Result<string> result = await behavior.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        await transaction.DidNotReceive().RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_TransactionalCommand_FailureResult_RollsBack()
    {
        // Arrange
        ICommandHandler<TransactionalCommand, string> innerHandler = Substitute.For<ICommandHandler<TransactionalCommand, string>>();
        IApplicationDbContext dbContext = Substitute.For<IApplicationDbContext>();
        DatabaseFacade databaseFacade = Substitute.For<DatabaseFacade>((Microsoft.EntityFrameworkCore.DbContext)null!);
        IDbContextTransaction transaction = Substitute.For<IDbContextTransaction>();

        dbContext.Database.Returns(databaseFacade);
        databaseFacade.BeginTransactionAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(transaction));

        var command = new TransactionalCommand("Test");
        var error = new Error("Domain.Error", "Failed", ErrorType.Validation);
        innerHandler.HandleAsync(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure<string>(error)));

        var behavior = new TransactionBehavior<TransactionalCommand, string>(innerHandler, dbContext);

        // Act
        Result<string> result = await behavior.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
        await transaction.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_TransactionalCommand_ThrowsException_RollsBackAndRethrows()
    {
        // Arrange
        ICommandHandler<TransactionalCommand, string> innerHandler = Substitute.For<ICommandHandler<TransactionalCommand, string>>();
        IApplicationDbContext dbContext = Substitute.For<IApplicationDbContext>();
        DatabaseFacade databaseFacade = Substitute.For<DatabaseFacade>((Microsoft.EntityFrameworkCore.DbContext)null!);
        IDbContextTransaction transaction = Substitute.For<IDbContextTransaction>();

        dbContext.Database.Returns(databaseFacade);
        databaseFacade.BeginTransactionAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(transaction));

        var command = new TransactionalCommand("Test");
        innerHandler.HandleAsync(command, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Db failure"));

        var behavior = new TransactionBehavior<TransactionalCommand, string>(innerHandler, dbContext);

        // Act
        Func<Task> act = async () => await behavior.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Db failure");
        await transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
        await transaction.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }
}
