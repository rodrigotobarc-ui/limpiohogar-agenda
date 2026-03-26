using LimpioHogar.Agenda.Client.Models;

namespace LimpioHogar.Agenda.Client.Services;

public interface IServicioService
{
    Task<List<Servicio>> ObtenerTodosAsync();
    Task<List<Servicio>> ObtenerPorSemanaAsync(DateOnly inicioSemana, DateOnly finSemana);
    Task<List<Servicio>> ObtenerPorFechaAsync(DateOnly fecha);
    Task<List<Servicio>> ObtenerPorTrabajadoraAsync(Guid trabajadoraId);
    Task<List<Servicio>> ObtenerPorClienteAsync(Guid clienteId);
    Task<Servicio?> ObtenerPorIdAsync(Guid id);
    Task<Servicio> CrearAsync(Servicio servicio);
    Task<Servicio> ActualizarAsync(Servicio servicio);
    Task<Servicio> CambiarEstadoAsync(Guid id, string nuevoEstado);
    Task EliminarAsync(Guid id);
}

public class ServicioService : IServicioService
{
    private readonly ISupabaseService _supabase;

    public ServicioService(ISupabaseService supabase)
    {
        _supabase = supabase;
    }

    public Task<List<Servicio>> ObtenerTodosAsync()
        => _supabase.GetAsync<Servicio>("servicios", "order=fecha.desc,hora.asc");

    public Task<List<Servicio>> ObtenerPorSemanaAsync(DateOnly inicioSemana, DateOnly finSemana)
        => _supabase.GetAsync<Servicio>("servicios",
            $"fecha=gte.{inicioSemana:yyyy-MM-dd}&fecha=lte.{finSemana:yyyy-MM-dd}&order=fecha.asc,hora.asc");

    public Task<List<Servicio>> ObtenerPorFechaAsync(DateOnly fecha)
        => _supabase.GetAsync<Servicio>("servicios",
            $"fecha=eq.{fecha:yyyy-MM-dd}&order=hora.asc");

    public Task<List<Servicio>> ObtenerPorTrabajadoraAsync(Guid trabajadoraId)
        => _supabase.GetAsync<Servicio>("servicios",
            $"trabajadora_id=eq.{trabajadoraId}&order=fecha.desc");

    public Task<List<Servicio>> ObtenerPorClienteAsync(Guid clienteId)
        => _supabase.GetAsync<Servicio>("servicios",
            $"cliente_id=eq.{clienteId}&order=fecha.desc");

    public Task<Servicio?> ObtenerPorIdAsync(Guid id)
        => _supabase.GetByIdAsync<Servicio>("servicios", id);

    public Task<Servicio> CrearAsync(Servicio servicio)
    {
        var montos = Servicio.ObtenerMontos(servicio.Plan);
        servicio.MontoTotal = montos.Total;
        servicio.MontoTrabajadora = montos.Trabajadora;
        servicio.MontoAgencia = montos.Agencia;
        return _supabase.InsertAsync("servicios", servicio);
    }

    public Task<Servicio> ActualizarAsync(Servicio servicio)
        => _supabase.UpdateAsync("servicios", servicio.Id, servicio);

    public async Task<Servicio> CambiarEstadoAsync(Guid id, string nuevoEstado)
    {
        var servicio = await _supabase.GetByIdAsync<Servicio>("servicios", id);
        if (servicio is null) throw new InvalidOperationException("Servicio no encontrado");
        servicio.Estado = nuevoEstado;
        return await _supabase.UpdateAsync("servicios", id, servicio);
    }

    public Task EliminarAsync(Guid id)
        => _supabase.DeleteAsync("servicios", id);
}
