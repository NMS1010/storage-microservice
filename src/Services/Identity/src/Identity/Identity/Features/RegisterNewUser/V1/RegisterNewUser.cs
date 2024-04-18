using BuildingBlocks.Constants;
using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.CustomAPIResponse;
using BuildingBlocks.Web;
using FluentValidation;
using Identity.Identity.Exceptions;
using Identity.Identity.Models;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;

namespace Identity.Identity.Features.RegisterNewUser.V1
{
    public record RegisterNewUserCommand(
        string Email,
        string Password,
        string ConfirmPassword,
        string FirstName,
        string LastName
    ) : ICommand<RegisterNewUserResult>;

    public record RegisterNewUserResult(
        string Id,
        string Email,
        string FirstName,
        string LastName
    );

    public record RegisterNewUserRequestDto(
        string Email,
        string Password,
        string ConfirmPassword,
        string FirstName,
        string LastName
    );

    public record RegisterNewUserResponseDto(
        string Id,
        string Email,
        string FirstName,
        string LastName
    );

    public class RegisterNewUserValidator : AbstractValidator<RegisterNewUserCommand>
    {
        public RegisterNewUserValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password);
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
        }
    }

    public class RegisterNewUserEndpoint : IMinimalEndpoint
    {
        public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
        {
            builder.MapPost($"{EndpointConfig.BaseAPIPath}/identity/register-user",
                async (RegisterNewUserRequestDto request, IMediator _mediator) =>
                {
                    var command = request.Adapt<RegisterNewUserCommand>();

                    var result = await _mediator.Send(command);

                    var response = result.Adapt<RegisterNewUserResponseDto>();

                    return Results.Ok(new APIResponse<RegisterNewUserResponseDto>(StatusCodes.Status200OK, response));
                }
            )
            .WithName("RegisterUser")
            .WithApiVersionSet(builder.NewApiVersionSet("Identity").Build())
            .Produces<RegisterNewUserResponseDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Register new User")
            .WithDescription("Register new user")
            .WithOpenApi()
            .HasApiVersion(1.0);

            return builder;
        }
    }

    public class RegisterNewUserCommandHandler(
        UserManager<AppUser> _userManager
    )
    : ICommandHandler<RegisterNewUserCommand, RegisterNewUserResult>
    {
        public async Task<RegisterNewUserResult> Handle(RegisterNewUserCommand command, CancellationToken cancellationToken)
        {
            var user = command.Adapt<AppUser>();
            user.UserName = command.Email.Replace("@", "");
            var result = await _userManager.CreateAsync(user, command.Password);

            if (result.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(user, Common.SystemRole.USER);

                if (!roleResult.Succeeded)
                {
                    throw new RegisterUserException(string.Join(',', roleResult.Errors.Select(e => e.Description)));
                }

                return user.Adapt<RegisterNewUserResult>();
            }

            throw new RegisterUserException(string.Join(',', result.Errors.Select(e => e.Description)));
        }
    }
}
