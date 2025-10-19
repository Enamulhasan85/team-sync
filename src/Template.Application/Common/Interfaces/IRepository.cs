using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDbGenericRepository.Models;
using Template.Application.Common.Models;

namespace Template.Application.Common.Interfaces
{
    public interface IRepository<T, TKey> where T : IDocument<TKey> where TKey : IEquatable<TKey>
    {
        Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>>? predicate, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate, CancellationToken cancellationToken = default);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

        Task<PaginatedResult<T>> GetPaginatedAsync(
            int page,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            Expression<Func<T, object>>? orderBy = null,
            bool orderByDescending = false,
            CancellationToken cancellationToken = default);

        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
    }
}
