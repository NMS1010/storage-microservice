using BuildingBlocks.Core.Event;
using BuildingBlocks.Core.Model;
using BuildingBlocks.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.EFCore
{
    public class AppDbContextBase(
        DbContextOptions options,
        ICurrentUserProvider _currentUserProvider,
        ILogger<AppDbContextBase> _logger
    ) : DbContext(options), IDbContext
    {
        private IDbContextTransaction _currentTransaction;
        private readonly string _functionName = $"{nameof(AppDbContextBase)} =>";

        public new DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken)
        {
            if (_currentTransaction != null)
            {
                return;
            }
            _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken)
        {
            try
            {
                await SaveChangeAsync(cancellationToken);
                await _currentTransaction.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                _currentTransaction?.Dispose();
                _currentTransaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                _currentTransaction?.Dispose();
                _currentTransaction = null;
            }
        }

        public async Task<int> SaveChangeAsync(CancellationToken cancellationToken)
        {
            OnBeforeSaving();

            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            // handle concurrency update
            //ref: https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations#resolving-concurrency-conflicts
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, $"{_functionName} {nameof(SaveChangeAsync)} Error on saving changes. Error Message = {ex.Message}");
                foreach (var entry in ex.Entries)
                {
                    // get current database values
                    var databaseValues = entry.GetDatabaseValues();

                    if (databaseValues == null)
                    {
                        _logger.LogError($"{_functionName} {nameof(SaveChangeAsync)} The record no longer exists in the database, The record has been deleted by another user.");
                        throw;
                    }

                    // Refresh the original values to bypass next concurrency check
                    entry.OriginalValues.SetValues(databaseValues);
                }

                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_functionName} {nameof(SaveChangeAsync)} Error on saving changes. Error Message = {ex.Message}");
                throw;
            }
        }

        private void OnBeforeSaving()
        {
            try
            {
                foreach (var entry in ChangeTracker.Entries<IAggregate>())
                {
                    var now = DateTime.UtcNow;
                    var userId = _currentUserProvider.GetUserId();
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            entry.Entity.CreatedAt = now;
                            entry.Entity.CreatedBy = userId;
                            break;

                        case EntityState.Modified:
                            entry.Entity.LastModifiedAt = now;
                            entry.Entity.LastModifiedBy = userId;
                            entry.Entity.Version++;
                            break;

                        case EntityState.Deleted:
                            entry.State = EntityState.Modified;
                            entry.Entity.LastModifiedAt = now;
                            entry.Entity.LastModifiedBy = userId;
                            entry.Entity.IsDeleted = true;
                            entry.Entity.Version++;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_functionName} {nameof(OnBeforeSaving)} Error on before saving changes. Error Message = {ex.Message}");
                throw;
            }
        }

        public IReadOnlyList<IDomainEvent> GetDomainEvents()
        {
            var domainEntities = ChangeTracker
                .Entries<IAggregate>()
                .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
                .Select(x => x.Entity)
                .ToList();

            var domainEvents = domainEntities
                .SelectMany(x => x.DomainEvents)
                .ToList();

            domainEntities.ForEach(entity => entity.ClearDomainEvents());

            return domainEvents;
        }
    }
}
