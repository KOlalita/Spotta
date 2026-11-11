using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spotta.Domain.Membresias;
using Spotta.Domain.Usuarios;

namespace Spotta.Infrastructure.Membresias;

internal sealed class AjusteMembresiaConfiguracion : IEntityTypeConfiguration<AjusteMembresia>
{
    public void Configure(EntityTypeBuilder<AjusteMembresia> e)
    {
        e.ToTable(t =>
        {
            t.HasCheckConstraint("ck_ajuste_dias", "dias <> 0");
            t.HasCheckConstraint("ck_ajuste_motivo", "length(trim(motivo)) > 0");
        });
        e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        e.Property(x => x.CreadoEn).HasDefaultValueSql("now()");
        e.HasOne(x => x.Membresia).WithMany(m => m.Ajustes).HasForeignKey(x => x.MembresiaId);
        e.HasOne<Perfil>().WithMany().HasForeignKey(x => x.CreadoPor)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
