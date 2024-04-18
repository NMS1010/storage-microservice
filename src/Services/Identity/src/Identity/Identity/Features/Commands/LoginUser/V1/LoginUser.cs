using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.CustomAPIResponse;
using BuildingBlocks.Web;
using FluentValidation;
using Identity.Configurations;
using Identity.Identity.Exceptions;
using Identity.Identity.Models;
using IdentityModel.Client;
using IdentityServer4.Models;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace Identity.Identity.Features.Commands.LoginUser.V1
{
    public class LoginUser
    {
        public record LoginUserCommand(
            string Email,
            string Password
        ) : ICommand<LoginUserResult>;

        public record LoginUserResult(
            string AccessToken,
            string RefreshToken,
            int ExpiresIn,
            string Scope
        );

        public record LoginUserRequestDto(
            string Email,
            string Password
        );

        public record LoginUserResponseDto(
            string AccessToken,
            string RefreshToken,
            int ExpiresIn,
            string Scope
        );

        public class LoginUserValidator : AbstractValidator<LoginUserCommand>
        {
            public LoginUserValidator()
            {
                RuleFor(x => x.Email).NotEmpty().EmailAddress();
                RuleFor(x => x.Password).NotEmpty();
            }
        }

        public class LoginUserEndpoint : IMinimalEndpoint
        {
            public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
            {
                builder.MapPost($"{EndpointConfig.BaseAPIPath}/login",
                    async (LoginUserRequestDto request, IMediator _mediator) =>
                    {
                        var command = request.Adapt<LoginUserCommand>();

                        var result = await _mediator.Send(command);

                        var response = result.Adapt<LoginUserResponseDto>();

                        return Results.Ok(new APIResponse<LoginUserResponseDto>(StatusCodes.Status200OK, response));
                    }
                )
                .WithName("LoginUser")
                .WithApiVersionSet(builder.NewApiVersionSet("Identity").Build())
                .Produces<LoginUserResponseDto>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithSummary("Login User")
                .WithDescription("Login user")
                .WithOpenApi()
                .HasApiVersion(1.0);

                return builder;
            }
        }

        public class LoginUserCommandHandler(
            SignInManager<AppUser> _signInManager,
            UserManager<AppUser> _userManager,
            IHttpClientFactory _httpClientFactory,
            IOptions<AuthOptions> _authOptions
        )
        : ICommandHandler<LoginUserCommand, LoginUserResult>
        {
            public async Task<LoginUserResult> Handle(LoginUserCommand command, CancellationToken cancellationToken)
            {
                try
                {
                    var user = await _userManager.FindByEmailAsync(command.Email)
                        ?? throw new InvalidCredentialsException("Email is incorrect");

                    var signInResult = await _signInManager.CheckPasswordSignInAsync(user, command.Password, false);

                    if (!signInResult.Succeeded)
                    {
                        throw new InvalidCredentialsException("Password is incorrect");
                    }

                    var authValue = _authOptions.Value;

                    var client = _httpClientFactory.CreateClient(authValue.ClientId);

                    DiscoveryDocumentResponse disco = await client.GetDiscoveryDocumentAsync(cancellationToken: cancellationToken);

                    var tokenRequest = new PasswordTokenRequest
                    {
                        Address = disco.TokenEndpoint,
                        GrantType = GrantType.ResourceOwnerPassword,
                        UserName = command.Email,
                        Password = command.Password,
                        ClientId = authValue.ClientId,
                        ClientSecret = authValue.ClientSecret,
                        Scope = $"offline_access {authValue.Scope}"
                    };

                    var response = await client.RequestPasswordTokenAsync(tokenRequest, cancellationToken);

                    if (response.IsError)
                    {
                        throw new LoginUserException(response.Error);
                    }

                    return new LoginUserResult(
                            response.AccessToken,
                            response.RefreshToken,
                            response.ExpiresIn,
                            response.Scope
                        );
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
