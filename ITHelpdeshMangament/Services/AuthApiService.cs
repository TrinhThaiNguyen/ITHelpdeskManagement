using System.Net.Http.Json;
using ITHelpdeshMangament.Models.Auth;

namespace ITHelpdeshMangament.Services;

public class AuthApiService
{
    private readonly HttpClient _httpClient;

    public AuthApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthApiResponse?> LoginAsync(
        LoginViewModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/auth/login",
            new
            {
                model.Email,
                model.Password
            },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<AuthApiResponse>(
            cancellationToken: cancellationToken);
    }
}

public class AuthApiResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public string Email { get; set; } = string.Empty;

    public IList<string> Roles { get; set; } = new List<string>();
}