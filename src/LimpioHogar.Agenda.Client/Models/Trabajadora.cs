namespace LimpioHogar.Agenda.Client.Models;

public class Trabajadora
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? MetroCercano { get; set; }
    public string EstadoMigratorio { get; set; } = "no_informado";

    // Datos Bancarios
    public string? BancoNombreTitular { get; set; }
    public string? BancoRut { get; set; }
    public string? Banco { get; set; }
    public string? BancoTipoCuenta { get; set; }
    public string? BancoNumeroCuenta { get; set; }
    public string? BancoCorreo { get; set; }
    public bool Activa { get; set; } = true;
    public string? Notas { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public string NombreCompleto => $"{Nombre} {Apellido}";
}
