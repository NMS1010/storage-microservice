using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Identity.Identity.Models;
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
            var user = await _userManager.FindByEmailAsync(context.UserName);

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, context.Password, false);

            if (signInResult.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id),
                    new(ClaimTypes.Name, user.UserName),
                    new(ClaimTypes.Email, user.Email),
                    new(ClaimTypes.GivenName, user.FirstName + " " + user.LastName),
                };

                foreach (var role in roles)
                {
                    claims.Add(new(ClaimTypes.Role, role));
                }

                context.Result = new GrantValidationResult(
                    subject: user.Id,
                    authenticationMethod: "custom",
                    claims: claims
                );

                return;
            }

            context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, "Invalid Credentials");
        }
    }
}
