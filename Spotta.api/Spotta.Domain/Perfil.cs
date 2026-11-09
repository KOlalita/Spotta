namespace Spotta.Domain;

/// <summary>
/// Espejo de <c>auth.users</c>: comparte su identificador. Supabase guarda email
/// y contraseña; aquí vive todo lo demás (RN-02). Nunca se borra (RN-08).
/// </summary>
public sealed class Perfil
{
    /// <summary>El mismo identificador que <c>auth.users.id</c>.</summary>
    public Guid Id { get; set; }

    public Guid GimnasioId { get; set; }
    public Gimnasio? Gimnasio { get; set; }

    public required string Nombre { get; set; }
    public required string Apellidos { get; set; }

    /// <summary>Normalizado en mayúsculas y sin separadores. Único por gimnasio (RN-03).</summary>
    public required string Dni { get; set; }

    public required string Email { get; set; }
    public Rol Rol { get; set; }

    /// <summary>
    /// Falso mientras la contraseña siga siendo el DNI. Mueve el aviso del
    /// perfil (RN-05).
    /// </summary>
    public bool ContrasenaCambiada { get; set; }

    public DateTimeOffset? DadoDeBajaEn { get; set; }
    public Guid? DadoDeBajaPor { get; set; }

    public DateTimeOffset CreadoEn { get; set; }
    public Guid? CreadoPor { get; set; }

    public ICollection<Membresia> Membresias { get; set; } = [];

    public bool EstaDeBaja => DadoDeBajaEn is not null;

    /// <summary>Lo que se muestra en los listados del panel (RN-06).</summary>
    public string DniParcial => Dni.Length <= 4 ? Dni : string.Concat("····", Dni[^4..]);
}
