using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace LimpioHogar.Agenda.Client.Services;

public interface IWhatsAppService
{
    Task<bool> EnviarMensajeAsync(string telefono, string mensaje);
}

public class WhatsAppService : IWhatsAppService
{
    private readonly HttpClient _http;
    private readonly ISupabaseAuthService _auth;
    private readonly string _anonKey;

    public WhatsAppService(HttpClient http, ISupabaseAuthService auth, IConfiguration config)
    {
        _http = http;
        _auth = auth;
        _anonKey = config["Supabase:AnonKey"]!;
    }

    public async Task<bool> EnviarMensajeAsync(string telefono, string mensaje)
    {
        try
        {
            var token = _auth.GetAccessToken() ?? _anonKey;
            var request = new HttpRequestMessage(HttpMethod.Post, "/functions/v1/send-whatsapp")
            {
                Content = JsonContent.Create(new { to = telefono, message = mensaje })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
