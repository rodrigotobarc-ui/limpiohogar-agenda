using System.Text.Json.Serialization;

namespace LimpioHogar.Agenda.Client.Models;

public class Pago
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Guid Id { get; set; }
    public Guid ServicioId { get; set; }
    public DateOnly FechaPago { get; set; }
    public int Monto { get; set; }
    public string Tipo { get; set; } = string.Empty; // ingreso_cliente | egreso_trabajadora
    public bool Confirmado { get; set; }
    public string? Notas { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime CreatedAt { get; set; }

    [JsonIgnore]
    public Servicio? Servicio { get; set; }
}
