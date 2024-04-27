namespace BuildingBlocks.Core.Model
{
    public interface IEntity<T> : IAuditable, IVersion where T : notnull
    {
        public T Id { get; set; }
    }
}
