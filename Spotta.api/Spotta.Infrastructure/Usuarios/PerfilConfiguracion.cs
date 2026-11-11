using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spotta.Domain.Usuarios;

namespace Spotta.Infrastructure.Usuarios;

public sealed class PerfilConfiguracion : IEntityTypeConfiguration<Perfil>
{
    /// <summary>El índice que impone RN-03. La aplicación lo reconoce al violarse.</summary>
    public const string IndiceDniUnico = "ux_perfil_dni_por_gimnasio";

    public void Configure(EntityTypeBuilder<Perfil> e)
    {
        // El identificador lo pone Supabase: es el mismo que auth.users.id
        e.Property(x => x.Id).ValueGeneratedNever();
        e.Property(x => x.Rol).HasConversion<string>().HasMaxLength(20);
        e.Property(x => x.CreadoEn).HasDefaultValueSql("now()");

        e.HasIndex(x => new { x.GimnasioId, x.Dni })
            .IsUnique()
            .HasDatabaseName(IndiceDniUnico);

        e.HasOne(x => x.Gimnasio).WithMany().HasForeignKey(x => x.GimnasioId);
        e.HasOne<Perfil>().WithMany().HasForeignKey(x => x.CreadoPor)
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne<Perfil>().WithMany().HasForeignKey(x => x.DadoDeBajaPor)
            .OnDelete(DeleteBehavior.Restrict);

        e.Ignore(x => x.EstaDeBaja);
    }
}
