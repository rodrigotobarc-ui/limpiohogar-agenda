using LimpioHogar.Agenda.Client.Models;

namespace LimpioHogar.Agenda.Client.Services;

public interface IPagoService
{
    Task<List<Pago>> ObtenerTodosAsync();
    Task<List<Pago>> ObtenerPorMesAsync(int anio, int mes);
    Task<List<Pago>> ObtenerPorServicioAsync(Guid servicioId);
    Task<Pago?> ObtenerPorIdAsync(Guid id);
    Task CrearPagosDeServicioAsync(Servicio servicio);
    Task<Pago> ConfirmarPagoAsync(Guid id);
    Task EliminarAsync(Guid id);
}

public class PagoService : IPagoService
{
    private readonly ISupabaseService _supabase;

    public PagoService(ISupabaseService supabase)
    {
        _supabase = supabase;
    }

    public Task<List<Pago>> ObtenerTodosAsync()
        => _supabase.GetAsync<Pago>("pagos", "order=fecha_pago.desc");

    public Task<List<Pago>> ObtenerPorMesAsync(int anio, int mes)
    {
        var inicio = new DateOnly(anio, mes, 1);
        var fin = inicio.AddMonths(1).AddDays(-1);
        return _supabase.GetAsync<Pago>("pagos",
            $"fecha_pago=gte.{inicio:yyyy-MM-dd}&fecha_pago=lte.{fin:yyyy-MM-dd}&order=fecha_pago.asc");
    }

    public Task<List<Pago>> ObtenerPorServicioAsync(Guid servicioId)
        => _supabase.GetAsync<Pago>("pagos", $"servicio_id=eq.{servicioId}");

    public Task<Pago?> ObtenerPorIdAsync(Guid id)
        => _supabase.GetByIdAsync<Pago>("pagos", id);

    public async Task CrearPagosDeServicioAsync(Servicio servicio)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Now);

        var ingresoCliente = new Pago
        {
            ServicioId = servicio.Id,
            FechaPago = hoy,
            Monto = servicio.MontoTotal,
            Tipo = "ingreso_cliente"
        };

        var egresoTrabajadora = new Pago
        {
            ServicioId = servicio.Id,
            FechaPago = hoy,
            Monto = servicio.MontoTrabajadora,
            Tipo = "egreso_trabajadora"
        };

        await _supabase.InsertAsync("pagos", ingresoCliente);
        await _supabase.InsertAsync("pagos", egresoTrabajadora);
    }

    public async Task<Pago> ConfirmarPagoAsync(Guid id)
    {
        var pago = await _supabase.GetByIdAsync<Pago>("pagos", id);
        if (pago is null) throw new InvalidOperationException("Pago no encontrado");
        pago.Confirmado = true;
        return await _supabase.UpdateAsync("pagos", id, pago);
    }

    public Task EliminarAsync(Guid id)
        => _supabase.DeleteAsync("pagos", id);
}
