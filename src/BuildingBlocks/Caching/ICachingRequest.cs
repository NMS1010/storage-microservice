namespace BuildingBlocks.Caching
{
    public interface ICachingRequest
    {
        string CacheKey { get; }
        DateTime? ExpirationTime { get; }
    }
}
