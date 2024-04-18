using Identity.Identity.Exceptions;
using Identity.Identity.Models;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Identity.Configurations
{
    public class UserValidator(
        SignInManager<AppUser> _signInManager,
        UserManager<AppUser> _userManager
    ) : IResourceOwnerPasswordValidator
    {
        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var user = await _userManager.FindByEmailAsync(context.UserName)
                ?? throw new UserNotFoundException("User not found");

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, context.Password, false);

            if (signInResult.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var claims = new List<Claim>
                {
                    new(JwtClaimTypes.Subject, user.Id),
                    new(JwtClaimTypes.Name, user.UserName),
                    new(JwtClaimTypes.Email, user.Email),
                    new(JwtClaimTypes.GivenName, user.FirstName + " " + user.LastName),
                };

                foreach (var role in roles)
                {
                    claims.Add(new(JwtClaimTypes.Role, role));
                }

                context.Result = new GrantValidationResult(
                    subject: user.Id,
                    authenticationMethod: "password",
                    claims: claims
                );

                return;
            }

            context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, "Invalid Credentials");
        }
    }
}
