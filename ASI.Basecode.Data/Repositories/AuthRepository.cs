namespace ASI.Basecode.Data.Repositories;

using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

public class AuthRepository : IAuthRepository
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public AuthRepository(SignInManager<User> signInManager, UserManager<User> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<(bool Succeeded, bool IsLockedOut)> PasswordSignInAsync(User user, string password, bool isPersistent, bool lockoutOnFailure)
    {
        //  verify the password and get the sign-in result
        var result = await _signInManager.PasswordSignInAsync(user.UserName, password, isPersistent, lockoutOnFailure);

        if (result.Succeeded)
        {
            // Get the actual user with fresh data
            var signedInUser = await _userManager.FindByNameAsync(user.UserName);
            if (signedInUser != null)
            {
                // Create claims list
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, signedInUser.UserName),
                    new Claim(ClaimTypes.GivenName, signedInUser.FirstName),
                    new Claim(ClaimTypes.Surname, signedInUser.LastName),
                    new Claim(ClaimTypes.Email, signedInUser.Email ?? string.Empty),
                    new Claim("UserId", signedInUser.Id.ToString())
                };

                // Add role claims
                var roles = await _userManager.GetRolesAsync(signedInUser);
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                // Sign out the current session
                await _signInManager.SignOutAsync();

                // Sign in with custom claims
                await _signInManager.SignInWithClaimsAsync(signedInUser, isPersistent, claims);
            }
        }

        return (result.Succeeded, result.IsLockedOut);
    }

    public Task SignOutAsync() => _signInManager.SignOutAsync();
}