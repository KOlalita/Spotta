using Spotta.Domain;

namespace Spotta.Tests;

/// <summary>
/// Comprueba las suposiciones de configuración que, si fallan, lo hacen tarde y
/// en producción: la zona horaria y el formato de documento.
/// </summary>
public class ConfiguracionDelGimnasioTests
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

    [Theory]
    [InlineData("70123456")]        // DNI
    [InlineData("70 12 34 56")]     // con espacios
    [InlineData("70-123-456")]      // con guiones
    [InlineData("001234567")]       // carné de extranjería
    public void Acepta_los_documentos_peruanos(string valor)
    {
        Assert.True(Documento.EsValido(valor));
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234567")]         // 7 dígitos, corto
    [InlineData("1234567890")]      // 10 dígitos, largo
    [InlineData("12345678A")]       // un DNI español mal tecleado aquí
    [InlineData("ABCDEFGH")]
    public void Rechaza_lo_que_no_es_un_documento(string valor)
    {
        Assert.False(Documento.EsValido(valor));
    }

    /// <summary>
    /// Normalizar quita separadores, no letras. Si quitara letras, un
    /// «12345678A» se guardaría como un DNI impecable de 8 dígitos y el error de
    /// tecleo no saldría nunca a la luz.
    /// </summary>
    [Fact]
    public void Normalizar_no_se_come_las_letras()
    {
        Assert.Equal("12345678A", Documento.Normalizar("1234-5678 A"));
    }

    [Fact]
    public void Distingue_dni_de_carne_de_extranjeria()
    {
        Assert.Equal(TipoDocumento.Dni, Documento.TipoDe("70123456"));
        Assert.Equal(TipoDocumento.CarneExtranjeria, Documento.TipoDe("001234567"));
    }

    [Fact]
    public void Enmascara_dejando_los_cuatro_ultimos()
    {
        Assert.Equal("····3456", Documento.Enmascarar("70123456"));
    }
}
