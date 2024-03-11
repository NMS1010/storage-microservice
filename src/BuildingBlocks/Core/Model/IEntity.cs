namespace BuildingBlocks.Core.Model
{
    public interface IEntity<T> : IAuditable where T : notnull
    {
        public T Id { get; set; }
    }
}
