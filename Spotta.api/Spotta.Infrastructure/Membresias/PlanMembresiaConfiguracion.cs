using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spotta.Domain.Gimnasios;
using Spotta.Domain.Membresias;

namespace Spotta.Infrastructure.Membresias;

internal sealed class PlanMembresiaConfiguracion : IEntityTypeConfiguration<PlanMembresia>
{
    public void Configure(EntityTypeBuilder<PlanMembresia> e)
    {
        e.ToTable(t =>
        {
            t.HasCheckConstraint("ck_plan_duracion", "duracion_dias > 0");
            t.HasCheckConstraint("ck_plan_precio", "precio >= 0");
        });
        e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        e.Property(x => x.Precio).HasPrecision(10, 2);
        e.Property(x => x.CreadoEn).HasDefaultValueSql("now()");
        e.HasOne<Gimnasio>().WithMany().HasForeignKey(x => x.GimnasioId);
        e.Ignore(x => x.EstaArchivado);
    }
}
