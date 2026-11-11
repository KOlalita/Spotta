using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spotta.Domain.Accesos;
using Spotta.Domain.Gimnasios;
using Spotta.Domain.Membresias;
using Spotta.Domain.Usuarios;

namespace Spotta.Infrastructure.Accesos;

internal sealed class AccesoConfiguracion : IEntityTypeConfiguration<Acceso>
{
    public void Configure(EntityTypeBuilder<Acceso> e)
    {
        e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        e.Property(x => x.Resultado).HasConversion<string>().HasMaxLength(20);
        e.Property(x => x.Motivo).HasMaxLength(40);
        e.Property(x => x.OcurridoEn).HasDefaultValueSql("now()");

        // Índices parciales: el aforo y la racha sólo miran accesos permitidos
        e.HasIndex(x => new { x.GimnasioId, x.OcurridoEn })
            .HasDatabaseName("ix_acceso_aforo")
            .HasFilter("resultado = 'Permitido'")
            .IsDescending(false, true);

        e.HasIndex(x => new { x.UsuarioId, x.OcurridoEn })
            .HasDatabaseName("ix_acceso_usuario")
            .HasFilter("resultado = 'Permitido'")
            .IsDescending(false, true);

        e.HasOne<Gimnasio>().WithMany().HasForeignKey(x => x.GimnasioId);
        e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
        e.HasOne<Membresia>().WithMany().HasForeignKey(x => x.MembresiaId);
        e.HasOne<Perfil>().WithMany().HasForeignKey(x => x.RegistradoPor)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
