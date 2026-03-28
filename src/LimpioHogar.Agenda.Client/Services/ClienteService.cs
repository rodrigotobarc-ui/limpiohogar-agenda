using LimpioHogar.Agenda.Client.Models;

namespace LimpioHogar.Agenda.Client.Services;

public interface IClienteService
{
    Task<List<Cliente>> ObtenerTodosAsync();
    Task<List<Cliente>> ObtenerActivosAsync();
    Task<Cliente?> ObtenerPorIdAsync(Guid id);
    Task<Cliente> CrearAsync(Cliente cliente);
    Task<Cliente> ActualizarAsync(Cliente cliente);
    Task EliminarAsync(Guid id);
}

public class ClienteService : IClienteService
{
    private readonly ISupabaseService _supabase;
    private readonly ISupabaseAuthService _auth;

    public ClienteService(ISupabaseService supabase, ISupabaseAuthService auth)
    {
        _supabase = supabase;
        _auth = auth;
    }

    public Task<List<Cliente>> ObtenerTodosAsync()
        => _supabase.GetAsync<Cliente>("clientes", "order=nombre.asc");

    public Task<List<Cliente>> ObtenerActivosAsync()
        => _supabase.GetAsync<Cliente>("clientes", "activo=eq.true&order=nombre.asc");

    public Task<Cliente?> ObtenerPorIdAsync(Guid id)
        => _supabase.GetByIdAsync<Cliente>("clientes", id);

    public Task<Cliente> CrearAsync(Cliente cliente)
    {
        cliente.UserId = _auth.GetUserId() ?? throw new InvalidOperationException("Usuario no autenticado");
        return _supabase.InsertAsync("clientes", cliente);
    }

    public Task<Cliente> ActualizarAsync(Cliente cliente)
        => _supabase.UpdateAsync("clientes", cliente.Id, cliente);

    public Task EliminarAsync(Guid id)
        => _supabase.DeleteAsync("clientes", id);
}
