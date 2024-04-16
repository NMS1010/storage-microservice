namespace BuildingBlocks.EFCore
{
    public interface IDbContext
    {
        //IReadOnlyList<IDomainEvent> Events { get; }
        Task<int> SaveChangeAsync(CancellationToken cancellationToken);
        Task BeginTransactionAsync(CancellationToken cancellationToken);
        Task CommitTransactionAsync(CancellationToken cancellationToken);
        Task RollbackTransactionAsync(CancellationToken cancellationToken);
        Task ExecuteTransactionAsync(CancellationToken cancellationToken);
    }
}
