using EasyCaching.Core;
using MediatR;

namespace BuildingBlocks.Caching
{
    public class InvalidateCachingRequest<TRequest, TResponse>(
            IEasyCachingProvider _cachingProvider
    ) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull, IRequest<TResponse>
        where TResponse : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not IInvalidateCachingRequest invalidateCachingRequest)
            {
                // No caching invalidation request, continue to the next pipeline behavior
                return await next();
            }

            var response = await next();

            // Remove the cached response
            await _cachingProvider.RemoveAsync(invalidateCachingRequest.CacheKey, cancellationToken);

            return response;
        }
    }
}
