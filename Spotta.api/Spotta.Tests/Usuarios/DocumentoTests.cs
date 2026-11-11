using Spotta.Domain.Usuarios;

namespace Spotta.Tests.Usuarios;

public class DocumentoTests
{
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
