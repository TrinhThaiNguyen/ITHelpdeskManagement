namespace ITHelpdesk.Application.DTOs.Auth;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public string Email { get; set; } = string.Empty;

    public IList<string> Roles { get; set; } = new List<string>();
}