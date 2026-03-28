using Microsoft.JSInterop;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;

namespace LimpioHogar.Agenda.Client.Services;

public interface ISupabaseAuthService
{
    Task InitializeAsync();
    Task LoginAsync(string email, string password);
    Task LogoutAsync();
    string? GetAccessToken();
    Guid? GetUserId();
    bool IsAuthenticated { get; }
}

public class SupabaseAuthService : ISupabaseAuthService
{
    private readonly Supabase.Client _supabaseClient;
    private readonly IJSRuntime _js;

    public SupabaseAuthService(Supabase.Client supabaseClient, IJSRuntime js)
    {
        _supabaseClient = supabaseClient;
        _js = js;
    }

    public bool IsAuthenticated => _supabaseClient.Auth.CurrentSession is not null;

    public async Task InitializeAsync()
    {
        try
        {
            var json = await _js.InvokeAsync<string?>("authStorage.get");
            if (string.IsNullOrEmpty(json))
                return;

            var session = System.Text.Json.JsonSerializer.Deserialize<StoredSession>(json);
            if (session is null || string.IsNullOrEmpty(session.AccessToken) || string.IsNullOrEmpty(session.RefreshToken))
                return;

            await _supabaseClient.Auth.SetSession(session.AccessToken, session.RefreshToken);
            await PersistSession();

            _supabaseClient.Auth.AddStateChangedListener((_, _) =>
            {
                _ = PersistSession();
            });
        }
        catch
        {
            await _js.InvokeVoidAsync("authStorage.remove");
        }
    }

    public async Task LoginAsync(string email, string password)
    {
        var session = await _supabaseClient.Auth.SignIn(email, password);
        if (session is null)
            throw new Exception("No se pudo iniciar sesion");

        await PersistSession();
    }

    public async Task LogoutAsync()
    {
        await _supabaseClient.Auth.SignOut();
        await _js.InvokeVoidAsync("authStorage.remove");
    }

    public string? GetAccessToken()
        => _supabaseClient.Auth.CurrentSession?.AccessToken;

    public Guid? GetUserId()
    {
        var id = _supabaseClient.Auth.CurrentSession?.User?.Id;
        return id is not null ? Guid.Parse(id) : null;
    }

    private async Task PersistSession()
    {
        var session = _supabaseClient.Auth.CurrentSession;
        if (session is null) return;

        var stored = new StoredSession
        {
            AccessToken = session.AccessToken!,
            RefreshToken = session.RefreshToken!
        };
        var json = System.Text.Json.JsonSerializer.Serialize(stored);
        await _js.InvokeVoidAsync("authStorage.set", json);
    }

    private class StoredSession
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
