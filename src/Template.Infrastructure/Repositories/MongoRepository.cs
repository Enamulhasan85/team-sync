using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDbGenericRepository;
using MongoDbGenericRepository.Models;
using Template.Application.Common.Interfaces;
using Template.Application.Common.Models;
using Template.Domain.Common;

namespace Template.Infrastructure.Repositories
{
    public class MongoRepository<T, TKey> : IRepository<T, TKey>
        where T : IDocument<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly IMongoDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMongoCollection<T> _collection;

        public MongoRepository(IMongoDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _collection = _context.GetCollection<T>();
        }

        private FilterDefinition<T> ApplySoftDeleteFilter(FilterDefinition<T> filter)
        {
            if (typeof(AuditableEntity<TKey>).IsAssignableFrom(typeof(T)))
            {
                var softDeleteFilter = Builders<T>.Filter.Eq("IsDeleted", false);
                return Builders<T>.Filter.And(filter, softDeleteFilter);
            }
            return filter;
        }

        public async Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var filter = Builders<T>.Filter.Where(x => x.Id.Equals(id));
            filter = ApplySoftDeleteFilter(filter);
            return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>>? predicate, CancellationToken cancellationToken = default)
        {
            var filter = predicate != null
                ? Builders<T>.Filter.Where(predicate)
                : Builders<T>.Filter.Empty;

            filter = ApplySoftDeleteFilter(filter);
            return await _collection.Find(filter).ToListAsync(cancellationToken);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate, CancellationToken cancellationToken = default)
        {
            var filter = predicate != null
                ? Builders<T>.Filter.Where(predicate)
                : Builders<T>.Filter.Empty;

            filter = ApplySoftDeleteFilter(filter);
            var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            return count > 0;
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            var filter = predicate != null
                ? Builders<T>.Filter.Where(predicate)
                : Builders<T>.Filter.Empty;

            filter = ApplySoftDeleteFilter(filter);
            var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            return (int)count;
        }

        public async Task<PaginatedResult<T>> GetPaginatedAsync(
            int page,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            Expression<Func<T, object>>? orderBy = null,
            bool orderByDescending = false,
            CancellationToken cancellationToken = default)
        {
            var filter = predicate != null
                ? Builders<T>.Filter.Where(predicate)
                : Builders<T>.Filter.Empty;

            filter = ApplySoftDeleteFilter(filter);

            var skipNumber = (page - 1) * pageSize;

            var query = _collection.Find(filter);

            if (orderBy != null)
            {
                var sortDefinition = orderByDescending
                    ? Builders<T>.Sort.Descending(orderBy)
                    : Builders<T>.Sort.Ascending(orderBy);
                query = query.Sort(sortDefinition);
            }

            var items = await query
                .Skip(skipNumber)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            var total = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

            return new PaginatedResult<T>(items, (int)total, page, pageSize);
        }

        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity is AuditableEntity<TKey> auditable)
            {
                auditable.MarkCreated(_currentUserService.UserId);
            }

            await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
            return entity;
        }

        public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity is AuditableEntity<TKey> auditable)
            {
                auditable.MarkModified(_currentUserService.UserId);
            }

            var filter = Builders<T>.Filter.Eq(x => x.Id, entity.Id);
            filter = ApplySoftDeleteFilter(filter);
            await _collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
        }

        public async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity != null)
            {
                if (entity is AuditableEntity<TKey> auditable)
                {
                    auditable.MarkDeleted(_currentUserService.UserId);
                    var filter = Builders<T>.Filter.Eq(x => x.Id, id);
                    await _collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
                }
                else
                {
                    var filter = Builders<T>.Filter.Eq(x => x.Id, id);
                    await _collection.DeleteOneAsync(filter, cancellationToken);
                }
            }
        }
    }
}


