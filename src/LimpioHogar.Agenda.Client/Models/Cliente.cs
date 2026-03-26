namespace LimpioHogar.Agenda.Client.Models;

public class Cliente
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public string Comuna { get; set; } = string.Empty;
    public string Tipo { get; set; } = "casa";
    public string? Notas { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public string NombreCompleto => $"{Nombre} {Apellido}";
}
