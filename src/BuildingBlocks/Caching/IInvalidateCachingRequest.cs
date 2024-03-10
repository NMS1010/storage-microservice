namespace BuildingBlocks.Caching
{
    public interface IInvalidateCachingRequest
    {
        string CacheKey { get; }
    }
}
