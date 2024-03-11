namespace BuildingBlocks.Core.Model
{
    public interface IAggregate<T> : IEntity<T>
    {
        //IReadOnlyList<IDomainEvent> Events { get; }
        //void ClearDomainEvents();
    }
}
