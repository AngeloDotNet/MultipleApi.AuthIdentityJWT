using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace WebApi.Authentication.Services.Application;

public class IdentityService : IIdentityService
{
    private readonly JwtSettings jwtSettings;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;

    public IdentityService(IOptions<JwtSettings> jwtSettingsOptions, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        jwtSettings = jwtSettingsOptions.Value;
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    public async Task<AuthViewModel> LoginAsync(LoginInputModel inputModel)
    {
        var signInResult = await signInManager.PasswordSignInAsync(inputModel.UserName, inputModel.Password, false, false);
        if (!signInResult.Succeeded)
        {
            return null;
        }

        var user = await userManager.FindByNameAsync(inputModel.UserName);
        var userRoles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, inputModel.UserName),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email)
            }.Union(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var loginResponse = CreateToken(claims);

        user.RefreshToken = loginResponse.RefreshToken;
        user.RefreshTokenExpirationDate = DateTime.UtcNow.AddMinutes(jwtSettings.RefreshTokenExpirationMinutes);

        await userManager.UpdateAsync(user);

        return loginResponse;
    }

    private AuthViewModel CreateToken(IEnumerable<Claim> claims)
    {
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecurityKey));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(jwtSettings.Issuer, jwtSettings.Audience, claims,
            DateTime.UtcNow, DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes), signingCredentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

        var response = new AuthViewModel
        {
            AccessToken = accessToken,
            RefreshToken = GenerateRefreshToken()
        };

        return response;

        static string GenerateRefreshToken()
        {
            var randomNumber = new byte[256];
            using var generator = RandomNumberGenerator.Create();
            generator.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }
    }

    private ClaimsPrincipal ValidateAccessToken(string accessToken)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecurityKey)),
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var user = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out var securityToken);

            if (securityToken is JwtSecurityToken jwtSecurityToken && jwtSecurityToken.Header.Alg == SecurityAlgorithms.HmacSha256)
            {
                return user;
            }
        }
        catch
        {
        }

        return null;
    }

    public async Task<AuthViewModel> RefreshTokenAsync(RefreshTokenInpuModel inputModel)
    {
        var user = ValidateAccessToken(inputModel.AccessToken);
        if (user != null)
        {
            var userId = user.GetId();
            var dbUser = await userManager.FindByIdAsync(userId.ToString());

            if (dbUser?.RefreshToken == null || dbUser?.RefreshTokenExpirationDate < DateTime.UtcNow || dbUser?.RefreshToken != inputModel.RefreshToken)
            {
                return null;
            }

            var loginResponse = CreateToken(user.Claims);

            dbUser.RefreshToken = loginResponse.RefreshToken;
            dbUser.RefreshTokenExpirationDate = DateTime.UtcNow.AddMinutes(jwtSettings.RefreshTokenExpirationMinutes);

            await userManager.UpdateAsync(dbUser);

            return loginResponse;
        }

        return null;
    }

    public async Task<RegisterViewModel> RegisterAsync(RegisterInputModel request)
    {
        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            result = await userManager.AddToRoleAsync(user, RoleNames.User);
        }

        var response = new RegisterViewModel
        {
            Succeeded = result.Succeeded,
            Errors = result.Errors.Select(e => e.Description)
        };

        return response;
    }
}