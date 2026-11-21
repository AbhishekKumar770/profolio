namespace Profolio.UnitTests.Application.TestDoubles;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.EntityFrameworkCore;

public sealed class TestDbSet<TEntity> : DbSet<TEntity>, IQueryable<TEntity>, IAsyncEnumerable<TEntity>
    where TEntity : class
{
    private readonly List<TEntity> _data;
    private readonly IQueryable<TEntity> _queryable;

    public TestDbSet(IEnumerable<TEntity>? initialData = null)
    {
        _data = initialData?.ToList() ?? new List<TEntity>();
        _queryable = new TestAsyncEnumerable<TEntity>(_data);
    }

    public List<TEntity> Data => _data;

    public override Microsoft.EntityFrameworkCore.Metadata.IEntityType EntityType => null!;

    Type IQueryable.ElementType => _queryable.ElementType;
    Expression IQueryable.Expression => _queryable.Expression;
    IQueryProvider IQueryable.Provider => _queryable.Provider;

    IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();
    IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() => _data.GetEnumerator();

    IAsyncEnumerator<TEntity> IAsyncEnumerable<TEntity>.GetAsyncEnumerator(CancellationToken cancellationToken) =>
        new TestAsyncEnumerator<TEntity>(_data.GetEnumerator());
}
