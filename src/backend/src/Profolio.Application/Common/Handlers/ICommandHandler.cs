namespace Profolio.Application.Common.Handlers;

using System.Threading;
using System.Threading.Tasks;
using Profolio.Application.Common.Results;

public interface ICommandHandler<in TCommand>
{
    Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TCommand, TResponse>
{
    Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
