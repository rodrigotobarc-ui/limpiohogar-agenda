using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace LimpioHogar.Agenda.Client.Services;

public class SupabaseAuthStateProvider : AuthenticationStateProvider
{
    private readonly ISupabaseAuthService _authService;

    public SupabaseAuthStateProvider(ISupabaseAuthService authService)
    {
        _authService = authService;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = _authService.GetAccessToken();
        if (string.IsNullOrEmpty(token))
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "supabase");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    public void NotifyUserAuthentication()
        => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    public void NotifyUserLogout()
        => NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();

        try
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var kvp = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);
            if (kvp is null) return claims;

            if (kvp.TryGetValue("sub", out var sub))
                claims.Add(new Claim(ClaimTypes.NameIdentifier, sub.GetString()!));

            if (kvp.TryGetValue("email", out var email))
                claims.Add(new Claim(ClaimTypes.Email, email.GetString()!));

            if (kvp.TryGetValue("app_metadata", out var appMeta))
            {
                if (appMeta.TryGetProperty("role", out var role))
                    claims.Add(new Claim(ClaimTypes.Role, role.GetString()!));
            }
        }
        catch
        {
            // Token malformado — retornar claims vacios
        }

        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        base64 = base64.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
