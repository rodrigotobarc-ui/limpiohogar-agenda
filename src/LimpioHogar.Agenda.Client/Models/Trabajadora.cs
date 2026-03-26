namespace LimpioHogar.Agenda.Client.Models;

public class Trabajadora
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? Comuna { get; set; }
    public string EstadoMigratorio { get; set; } = "no_informado";
    public bool Activa { get; set; } = true;
    public string? Notas { get; set; }
    public DateTime CreatedAt { get; set; }

    public string NombreCompleto => $"{Nombre} {Apellido}";
}
