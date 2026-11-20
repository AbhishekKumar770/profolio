namespace Profolio.UnitTests.Application;

using System;
using System.Threading;
using System.Threading.Tasks;
using Profolio.Application.Common.Handlers;
using Profolio.Application.Common.Results;
using Xunit;

public class HandlerContractTests
{
    private sealed record TestVoidCommand(string Name);
    private sealed record TestResponseCommand(string Title);
    private sealed record TestQuery(Guid EntityId);
    private sealed record TestResponseDto(string Data);

    private sealed class TestVoidCommandHandler : ICommandHandler<TestVoidCommand>
    {
        public Task<Result> HandleAsync(TestVoidCommand command, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<Result>(cancellationToken);
            }

            if (string.IsNullOrWhiteSpace(command.Name))
            {
                return Task.FromResult(Result.Failure(Error.Validation("Name.Empty", "Name cannot be empty.")));
            }

            return Task.FromResult(Result.Success());
        }
    }

    private sealed class TestResponseCommandHandler : ICommandHandler<TestResponseCommand, Guid>
    {
        public Task<Result<Guid>> HandleAsync(TestResponseCommand command, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<Result<Guid>>(cancellationToken);
            }

            if (string.IsNullOrWhiteSpace(command.Title))
            {
                return Task.FromResult(Result.Failure<Guid>(Error.Validation("Title.Empty", "Title cannot be empty.")));
            }

            return Task.FromResult(Result.Success(Guid.NewGuid()));
        }
    }

    private sealed class TestQueryHandler : IQueryHandler<TestQuery, TestResponseDto>
    {
        public Task<Result<TestResponseDto>> HandleAsync(TestQuery query, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<Result<TestResponseDto>>(cancellationToken);
            }

            if (query.EntityId == Guid.Empty)
            {
                return Task.FromResult(Result.Failure<TestResponseDto>(Error.NotFound("Entity.NotFound", "Entity not found.")));
            }

            return Task.FromResult(Result.Success(new TestResponseDto("Sample Data")));
        }
    }

    [Fact]
    public async Task CommandHandler_VoidCommand_ExecutesSuccessfullyAndReturnsResult()
    {
        // Arrange
        ICommandHandler<TestVoidCommand> handler = new TestVoidCommandHandler();
        var command = new TestVoidCommand("Valid Name");

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CommandHandler_VoidCommand_ReturnsFailureOnValidationError()
    {
        // Arrange
        ICommandHandler<TestVoidCommand> handler = new TestVoidCommandHandler();
        var command = new TestVoidCommand("");

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Name.Empty", result.Error.Code);
    }

    [Fact]
    public async Task CommandHandler_VoidCommand_ThrowsWhenCancellationRequested()
    {
        // Arrange
        ICommandHandler<TestVoidCommand> handler = new TestVoidCommandHandler();
        var command = new TestVoidCommand("Valid");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => handler.HandleAsync(command, cts.Token));
    }

    [Fact]
    public async Task CommandHandler_WithResponse_ExecutesSuccessfullyAndReturnsValue()
    {
        // Arrange
        ICommandHandler<TestResponseCommand, Guid> handler = new TestResponseCommandHandler();
        var command = new TestResponseCommand("Project Profolio");

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task CommandHandler_WithResponse_ThrowsWhenCancellationRequested()
    {
        // Arrange
        ICommandHandler<TestResponseCommand, Guid> handler = new TestResponseCommandHandler();
        var command = new TestResponseCommand("Project Profolio");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => handler.HandleAsync(command, cts.Token));
    }

    [Fact]
    public async Task QueryHandler_ExecutesSuccessfullyAndReturnsResponseDto()
    {
        // Arrange
        IQueryHandler<TestQuery, TestResponseDto> handler = new TestQueryHandler();
        var query = new TestQuery(Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Sample Data", result.Value.Data);
    }

    [Fact]
    public async Task QueryHandler_ThrowsWhenCancellationRequested()
    {
        // Arrange
        IQueryHandler<TestQuery, TestResponseDto> handler = new TestQueryHandler();
        var query = new TestQuery(Guid.NewGuid());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => handler.HandleAsync(query, cts.Token));
    }
}
