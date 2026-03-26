using System.Net.Http.Json;
using System.Text.Json;

namespace LimpioHogar.Agenda.Client.Services;

public interface ISupabaseService
{
    Task<List<T>> GetAsync<T>(string table, string? query = null);
    Task<T?> GetByIdAsync<T>(string table, Guid id);
    Task<T> InsertAsync<T>(string table, T item);
    Task<T> UpdateAsync<T>(string table, Guid id, T item);
    Task DeleteAsync(string table, Guid id);
}

public class SupabaseService : ISupabaseService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;

    public SupabaseService(HttpClient http)
    {
        _http = http;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<T>> GetAsync<T>(string table, string? query = null)
    {
        var url = $"/rest/v1/{table}?select=*";
        if (!string.IsNullOrEmpty(query))
            url += $"&{query}";

        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<T>>(_jsonOptions) ?? [];
    }

    public async Task<T?> GetByIdAsync<T>(string table, Guid id)
    {
        var url = $"/rest/v1/{table}?id=eq.{id}&select=*";
        var items = await _http.GetFromJsonAsync<List<T>>(url, _jsonOptions);
        return items is { Count: > 0 } ? items[0] : default;
    }

    public async Task<T> InsertAsync<T>(string table, T item)
    {
        var response = await _http.PostAsJsonAsync($"/rest/v1/{table}", item, _jsonOptions);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<T>>(_jsonOptions);
        return result![0];
    }

    public async Task<T> UpdateAsync<T>(string table, Guid id, T item)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, $"/rest/v1/{table}?id=eq.{id}")
        {
            Content = JsonContent.Create(item, options: _jsonOptions)
        };
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<T>>(_jsonOptions);
        return result![0];
    }

    public async Task DeleteAsync(string table, Guid id)
    {
        var response = await _http.DeleteAsync($"/rest/v1/{table}?id=eq.{id}");
        response.EnsureSuccessStatusCode();
    }
}
