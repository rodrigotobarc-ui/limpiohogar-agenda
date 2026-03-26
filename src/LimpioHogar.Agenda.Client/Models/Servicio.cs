namespace LimpioHogar.Agenda.Client.Models;

public class Servicio
{
    public Guid Id { get; set; }
    public Guid TrabajadoraId { get; set; }
    public Guid ClienteId { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly Hora { get; set; }
    public string Plan { get; set; } = "basico";
    public string Estado { get; set; } = "pendiente";
    public int MontoTotal { get; set; }
    public int MontoTrabajadora { get; set; }
    public int MontoAgencia { get; set; }
    public string? Notas { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties for display
    public Trabajadora? Trabajadora { get; set; }
    public Cliente? Cliente { get; set; }

    public static (int Total, int Trabajadora, int Agencia) ObtenerMontos(string plan) => plan switch
    {
        "basico" => (35000, 25000, 10000),
        "premium" => (55000, 40000, 15000),
        "deluxe" => (85000, 65000, 20000),
        _ => (35000, 25000, 10000)
    };
}
