using BuildingBlocks.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BuildingBlocks.EFCore
{
    public class AppDbContextBase(
        ICurrentUserProvider _currentUserProvider
    ) : DbContext, IDbContext
    {
        private IDbContextTransaction _currentTransaction;

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

        public Task ExecuteTransactionAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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

        public Task<int> SaveChangeAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private void OnBeforeSaving()
        {

        }
    }
}
