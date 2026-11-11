using Spotta.Domain.Gimnasios;

namespace Spotta.Tests.Gimnasios;

/// <summary>
/// Comprueba las suposiciones de configuración que, si fallan, lo hacen tarde y
/// en producción.
/// </summary>
public class GimnasioTests
{
    private static readonly Gimnasio Gimnasio = new() { Nombre = "Spotta Gym", AforoMaximo = 80 };

    [Fact]
    public void La_zona_horaria_por_defecto_es_la_de_Peru()
    {
        Assert.Equal("America/Lima", Gimnasio.ZonaHoraria);
        Assert.Equal("PEN", Gimnasio.Moneda);
    }

    /// <summary>
    /// Windows usa sus propios nombres de zona ("SA Pacific Standard Time"), no
    /// los de IANA. .NET los traduce desde la 6, pero si alguien despliega sin
    /// ICU esto revienta al calcular la racha, no al arrancar. Mejor que falle
    /// aquí.
    /// </summary>
    [Fact]
    public void La_zona_horaria_resuelve_en_esta_maquina()
    {
        var zona = TimeZoneInfo.FindSystemTimeZoneById(Gimnasio.ZonaHoraria);
        Assert.NotNull(zona);
    }

    /// <summary>
    /// Perú no tiene horario de verano, así que todas las semanas duran 168
    /// horas y el cálculo de racha (RN-31) se ahorra el caso límite. Si este
    /// test se pone rojo, es que el gimnasio ya no está en Perú y hay que
    /// revisar la racha.
    /// </summary>
    [Fact]
    public void Peru_no_cambia_la_hora_en_todo_el_ano()
    {
        var zona = TimeZoneInfo.FindSystemTimeZoneById(Gimnasio.ZonaHoraria);
        var desfases = Enumerable.Range(0, 365)
            .Select(d => zona.GetUtcOffset(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc).AddDays(d)))
            .Distinct()
            .ToList();

        Assert.Single(desfases);
        Assert.Equal(TimeSpan.FromHours(-5), desfases[0]);
    }

    /// <summary>
    /// Las 22:00 del domingo 8 de noviembre en Lima son las 03:00 del lunes 9 en
    /// UTC. Si «hoy» se calculara en UTC, una membresía que vence el domingo se
    /// daría por vencida tres horas antes de tiempo.
    /// </summary>
    [Fact]
    public void La_fecha_local_es_la_de_Lima_y_no_la_de_UTC()
    {
        var domingoNocheEnLima = new DateTimeOffset(2026, 11, 9, 3, 0, 0, TimeSpan.Zero);

        Assert.Equal(new DateOnly(2026, 11, 8), Gimnasio.FechaLocal(domingoNocheEnLima));
    }
}
