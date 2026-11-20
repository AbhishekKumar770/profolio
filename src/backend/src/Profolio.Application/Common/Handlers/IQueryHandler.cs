namespace Profolio.Application.Common.Handlers;

using System.Threading;
using System.Threading.Tasks;
using Profolio.Application.Common.Results;

public interface IQueryHandler<in TQuery, TResponse>
{
    Task<Result<TResponse>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
