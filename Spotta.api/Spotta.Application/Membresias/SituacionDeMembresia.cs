using Spotta.Domain.Membresias;

namespace Spotta.Application.Membresias;

/// <summary>
/// En qué punto está un socio con sus membresías, en una sola palabra. Es lo
/// que pinta el listado de recepción y lo que el socio ve en su perfil.
/// </summary>
public sealed record SituacionDeMembresia(string Estado, DateOnly? FechaFin, int? DiasRestantes)
{
    public const string Vigente = "vigente";
    public const string Programada = "programada";
    public const string Vencida = "vencida";
    public const string SinMembresia = "sin_membresia";

    /// <summary>
    /// Manda la vigente; si no hay, la próxima que empieza; si tampoco, la última
    /// que terminó. El estado de cada membresía sale de sus fechas (RN-16).
    /// </summary>
    public static SituacionDeMembresia De(IEnumerable<Membresia> membresias, DateOnly hoy)
    {
        var lista = membresias.ToList();

        var vigente = lista.FirstOrDefault(m => m.EstadoEn(hoy) == EstadoMembresia.Vigente);
        if (vigente is not null)
            return new(Vigente, vigente.FechaFin, vigente.DiasRestantesEn(hoy));

        var programada = lista
            .Where(m => m.EstadoEn(hoy) == EstadoMembresia.Programada)
            .MinBy(m => m.FechaInicio);
        if (programada is not null)
            return new(Programada, programada.FechaFin, null);

        var ultima = lista.MaxBy(m => m.FechaFin);
        return ultima is null
            ? new(SinMembresia, null, null)
            : new(Vencida, ultima.FechaFin, 0);
    }
}
