using LimpioHogar.Agenda.Client.Models;

namespace LimpioHogar.Agenda.Client.Services;

public interface ITrabajadoraService
{
    Task<List<Trabajadora>> ObtenerTodasAsync();
    Task<List<Trabajadora>> ObtenerActivasAsync();
    Task<Trabajadora?> ObtenerPorIdAsync(Guid id);
    Task<Trabajadora> CrearAsync(Trabajadora trabajadora);
    Task<Trabajadora> ActualizarAsync(Trabajadora trabajadora);
    Task EliminarAsync(Guid id);
}

public class TrabajadoraService : ITrabajadoraService
{
    private readonly ISupabaseService _supabase;
    private readonly ISupabaseAuthService _auth;

    public TrabajadoraService(ISupabaseService supabase, ISupabaseAuthService auth)
    {
        _supabase = supabase;
        _auth = auth;
    }

    public Task<List<Trabajadora>> ObtenerTodasAsync()
        => _supabase.GetAsync<Trabajadora>("trabajadoras", "order=nombre.asc");

    public Task<List<Trabajadora>> ObtenerActivasAsync()
        => _supabase.GetAsync<Trabajadora>("trabajadoras", "activa=eq.true&order=nombre.asc");

    public Task<Trabajadora?> ObtenerPorIdAsync(Guid id)
        => _supabase.GetByIdAsync<Trabajadora>("trabajadoras", id);

    public Task<Trabajadora> CrearAsync(Trabajadora trabajadora)
    {
        trabajadora.UserId = _auth.GetUserId() ?? throw new InvalidOperationException("Usuario no autenticado");
        return _supabase.InsertAsync("trabajadoras", trabajadora);
    }

    public Task<Trabajadora> ActualizarAsync(Trabajadora trabajadora)
        => _supabase.UpdateAsync("trabajadoras", trabajadora.Id, trabajadora);

    public Task EliminarAsync(Guid id)
        => _supabase.DeleteAsync("trabajadoras", id);
}
