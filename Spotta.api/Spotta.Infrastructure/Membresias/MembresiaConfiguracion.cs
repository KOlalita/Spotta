using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spotta.Domain.Gimnasios;
using Spotta.Domain.Membresias;
using Spotta.Domain.Usuarios;

namespace Spotta.Infrastructure.Membresias;

internal sealed class MembresiaConfiguracion : IEntityTypeConfiguration<Membresia>
{
    public void Configure(EntityTypeBuilder<Membresia> e)
    {
        e.ToTable(t =>
        {
            t.HasCheckConstraint("ck_membresia_periodo", "fecha_fin >= fecha_inicio");
            t.HasCheckConstraint("ck_membresia_duracion", "duracion_dias > 0");
            t.HasCheckConstraint("ck_membresia_precio", "precio_cobrado >= 0");
        });
        e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        e.Property(x => x.PrecioCobrado).HasPrecision(10, 2);
        e.Property(x => x.MetodoPago).HasConversion<string>().HasMaxLength(20);
        e.Property(x => x.CobradoEn).HasDefaultValueSql("now()");
        e.Property(x => x.CreadoEn).HasDefaultValueSql("now()");

        e.HasOne<Gimnasio>().WithMany().HasForeignKey(x => x.GimnasioId);
        e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
        e.HasOne(x => x.Plan).WithMany().HasForeignKey(x => x.PlanId);
        e.HasOne<Perfil>().WithMany().HasForeignKey(x => x.CobradoPor)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
