
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ITHelpdesk.Application.DTOs.Auth;
using ITHelpdesk.Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ITHelpdesk.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOptions<JwtSettings> _jwtOptions;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _jwtOptions = jwtOptions;
    }

    public async Task<AuthResponseDto> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email
        };

        var createResult = await _userManager.CreateAsync(
            user,
            request.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                createResult.Errors.Select(error => error.Description));

            throw new InvalidOperationException(errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(
            user,
            "Requester");

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            var errors = string.Join(
                "; ",
                roleResult.Errors.Select(error => error.Description));

            throw new InvalidOperationException(
                $"Could not assign default role: {errors}");
        }

        return await GenerateTokenAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(
            user,
            request.Password);

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        return await GenerateTokenAsync(user);
    }

    private async Task<AuthResponseDto> GenerateTokenAsync(
        ApplicationUser user)
    {
        var jwtSettings = _jwtOptions.Value;

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add EmployeeId to JWT if the account is linked to an Employee.
        if (user.EmployeeId.HasValue)
        {
            claims.Add(new Claim(
                "employee_id",
                user.EmployeeId.Value.ToString()));
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(
            jwtSettings.AccessTokenExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            ExpiresAtUtc = expiresAt,
            Email = user.Email ?? string.Empty,
            Roles = roles
        };
    }
}
