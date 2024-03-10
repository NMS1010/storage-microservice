using BuildingBlocks.Constants;
using EasyCaching.Core;
using MediatR;

namespace BuildingBlocks.Caching
{
    public class CachingRequestBehavior<TRequest, TResponse>(
        IEasyCachingProvider _cachingProvider
    ) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull, IRequest<TResponse>
        where TResponse : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not ICachingRequest cachingRequest)
            {
                //  No caching request, continue to the next pipeline behavior
                return await next();
            }

            string cacheKey = cachingRequest.CacheKey;

            var cacheResponse = await _cachingProvider.GetAsync<TResponse>(cacheKey, cancellationToken);

            // If the response is cached, return it
            if (cacheResponse.HasValue)
            {
                return cacheResponse.Value;
            }

            var response = await next();

            var ttl = cachingRequest.ExpirationTime
                ?? DateTime.Now.AddHours(AppConstants.DEFAULT_CACHING_TTL_IN_HOURS);

            // Otherwise, cache the response
            await _cachingProvider.SetAsync(cacheKey, response, ttl.TimeOfDay, cancellationToken);

            return response;
        }
    }
}
