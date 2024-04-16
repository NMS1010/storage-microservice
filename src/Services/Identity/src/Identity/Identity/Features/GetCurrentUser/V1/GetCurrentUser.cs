using BuildingBlocks.Caching;
using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.CustomAPIResponse;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Web;
using FluentValidation;
using Identity.Identity.Models;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;

namespace Identity.Identity.Features.GetCurrentUser.V1
{
    public record GetCurrentUserQuery(string Id) : IQuery<GetCurrentUserResult>, ICachingRequest
    {
        public string CacheKey => "GetCurrentUser";

        public DateTime? ExpirationTime => DateTime.Now.AddHours(1);
    }

    public record GetCurrentUserResult(
        string Id,
        string Email,
        string PhoneNumber,
        string FirstName,
        string LastName
    );

    public record GetCurrentUserResponseDto(
        string Id,
        string Email,
        string PhoneNumber,
        string FirstName,
        string LastName
    );

    public class GetCurrentUserEndpoint : IMinimalEndpoint
    {
        public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
        {
            builder.MapGet($"{EndpointConfig.BaseAPIPath}/identity/current-user", async
                (ICurrentUserProvider _currentUserProvider, IMediator _mediator) =>
            {
                var query = new GetCurrentUserQuery(_currentUserProvider.GetUserId());

                var result = await _mediator.Send(query);

                var response = result.Adapt<GetCurrentUserResponseDto>();

                return Results.Ok(new APIResponse<GetCurrentUserResponseDto>(StatusCodes.Status200OK, response));
            })
            .RequireAuthorization(x =>
            {
                x.RequireAuthenticatedUser();
                x.AuthenticationSchemes = [JwtBearerDefaults.AuthenticationScheme];
            })
            .WithName("GetCurrentUser")
            .WithApiVersionSet(builder.NewApiVersionSet("Identity").Build())
            .Produces<GetCurrentUserResponseDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Get Current User")
            .WithDescription("Get Current User")
            .WithOpenApi()
            .HasApiVersion(1.0);


            return builder;
        }
    }

    public class GetCurrentUserValidator : AbstractValidator<GetCurrentUserQuery>
    {
        public GetCurrentUserValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    public class GetCurrenUserQueryHandler(
        UserManager<AppUser> _userManager
    )
    : IQueryHandler<GetCurrentUserQuery, GetCurrentUserResult>
    {
        public async Task<GetCurrentUserResult> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id) ??
                throw new NotFoundException("Cannot find current user");

            return user.Adapt<GetCurrentUserResult>();
        }
    }
}
