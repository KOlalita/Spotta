using System.Text;
using Microsoft.EntityFrameworkCore;
using Spotta.Domain;

namespace Spotta.Infrastructure;

public sealed class SpottaDb(DbContextOptions<SpottaDb> options) : DbContext(options)
{
    public DbSet<Gimnasio> Gimnasios => Set<Gimnasio>();
    public DbSet<Perfil> Perfiles => Set<Perfil>();
    public DbSet<PlanMembresia> Planes => Set<PlanMembresia>();
    public DbSet<Membresia> Membresias => Set<Membresia>();
    public DbSet<AjusteMembresia> Ajustes => Set<AjusteMembresia>();
    public DbSet<Acceso> Accesos => Set<Acceso>();
    public DbSet<PaseDispositivo> PasesDispositivo => Set<PaseDispositivo>();
    public DbSet<PaseConsumido> PasesConsumidos => Set<PaseConsumido>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<NotificacionLectura> LecturasDeNotificacion => Set<NotificacionLectura>();
    public DbSet<DispositivoPush> DispositivosPush => Set<DispositivoPush>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Gimnasio>(e =>
        {
            e.ToTable(t =>
            {
                t.HasCheckConstraint("ck_gimnasio_aforo", "aforo_maximo > 0");
                t.HasCheckConstraint("ck_gimnasio_ventana",
                    "ventana_presencia_min BETWEEN 15 AND 480");
                t.HasCheckConstraint("ck_gimnasio_dias_racha",
                    "dias_minimos_racha BETWEEN 1 AND 7");
            });
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Moneda).HasMaxLength(3);
            e.Property(x => x.CreadoEn).HasDefaultValueSql("now()");
        });

        b.Entity<Perfil>(e =>
        {
            // El identificador lo pone Supabase: es el mismo que auth.users.id
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Rol).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.CreadoEn).HasDefaultValueSql("now()");

            e.HasIndex(x => new { x.GimnasioId, x.Dni })
                .IsUnique()
                .HasDatabaseName("ux_perfil_dni_por_gimnasio");

            e.HasOne(x => x.Gimnasio).WithMany().HasForeignKey(x => x.GimnasioId);
            e.HasOne<Perfil>().WithMany().HasForeignKey(x => x.CreadoPor)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Perfil>().WithMany().HasForeignKey(x => x.DadoDeBajaPor)
                .OnDelete(DeleteBehavior.Restrict);

            e.Ignore(x => x.EstaDeBaja);
            e.Ignore(x => x.DniParcial);
        });

        b.Entity<PlanMembresia>(e =>
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
        });

        b.Entity<Membresia>(e =>
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
            e.HasOne(x => x.Usuario).WithMany(p => p.Membresias).HasForeignKey(x => x.UsuarioId);
            e.HasOne(x => x.Plan).WithMany().HasForeignKey(x => x.PlanId);
            e.HasOne<Perfil>().WithMany().HasForeignKey(x => x.CobradoPor)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<AjusteMembresia>(e =>
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
        });

        b.Entity<Acceso>(e =>
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
        });

        b.Entity<PaseDispositivo>(e =>
        {
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CreadoEn).HasDefaultValueSql("now()");
            e.HasIndex(x => x.UsuarioId)
                .HasDatabaseName("ix_pase_dispositivo_activo")
                .HasFilter("revocado_en IS NULL");
            e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
            e.HasOne<Perfil>().WithMany().HasForeignKey(x => x.RevocadoPor)
                .OnDelete(DeleteBehavior.Restrict);
            e.Ignore(x => x.EstaRevocado);
        });

        b.Entity<PaseConsumido>(e =>
        {
            e.HasKey(x => new { x.DispositivoId, x.Paso });
            e.Property(x => x.ConsumidoEn).HasDefaultValueSql("now()");
            e.HasOne<PaseDispositivo>().WithMany().HasForeignKey(x => x.DispositivoId);
        });

        b.Entity<Notificacion>(e =>
        {
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Titulo).HasMaxLength(120);
            e.Property(x => x.EnviadaEn).HasDefaultValueSql("now()");
            e.HasOne<Gimnasio>().WithMany().HasForeignKey(x => x.GimnasioId);
            e.HasOne<Perfil>().WithMany().HasForeignKey(x => x.EnviadaPor)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<NotificacionLectura>(e =>
        {
            e.HasKey(x => new { x.NotificacionId, x.UsuarioId });
            e.Property(x => x.LeidaEn).HasDefaultValueSql("now()");
            e.HasOne<Notificacion>().WithMany().HasForeignKey(x => x.NotificacionId);
            e.HasOne<Perfil>().WithMany().HasForeignKey(x => x.UsuarioId);
        });

        b.Entity<DispositivoPush>(e =>
        {
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.VistoEn).HasDefaultValueSql("now()");
            e.HasIndex(x => x.ExpoPushToken).IsUnique();
            e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
        });

        ProhibirBorradoEnCascada(b);
        AplicarNombresEnMinusculaConGuiones(b);
    }

    /// <summary>
    /// Aquí no se borra nada (RN-08): un usuario se da de baja, un plan se
    /// archiva. EF pone cascada por defecto en las relaciones obligatorias, y eso
    /// convierte un borrado accidental en la pérdida silenciosa del historial de
    /// accesos y cobros. Mejor que falle y se note.
    ///
    /// Las dos excepciones son tablas hijas sin valor propio: el antirreplay y
    /// las marcas de lectura.
    /// </summary>
    private static void ProhibirBorradoEnCascada(ModelBuilder b)
    {
        Type[] puedenCaerConSuPadre = [typeof(PaseConsumido), typeof(NotificacionLectura)];

        foreach (var entidad in b.Model.GetEntityTypes())
        {
            if (puedenCaerConSuPadre.Contains(entidad.ClrType)) continue;

            foreach (var ajena in entidad.GetForeignKeys())
                ajena.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    /// <summary>
    /// Postgres pliega a minúsculas cualquier identificador sin comillas, así que
    /// <c>FechaInicio</c> acabaría como <c>fechainicio</c> y habría que entrecomillar
    /// en cada consulta a mano. Con esto, el modelo en C# va en PascalCase y el
    /// esquema en snake_case, sin anotar una sola columna.
    /// </summary>
    private static void AplicarNombresEnMinusculaConGuiones(ModelBuilder b)
    {
        foreach (var entidad in b.Model.GetEntityTypes())
        {
            entidad.SetTableName(ConGuiones(entidad.ClrType.Name));

            foreach (var propiedad in entidad.GetProperties())
                propiedad.SetColumnName(ConGuiones(propiedad.Name));

            // Los nombres de claves e índices que pone EF ya vienen separados
            // ("PK_perfil", "IX_acceso_usuario_id"): sólo hay que bajarlos a
            // minúsculas. Pasarlos por ConGuiones los dejaría como "i_x_…".
            foreach (var clave in entidad.GetKeys())
                clave.SetName(clave.GetName()!.ToLowerInvariant());

            foreach (var ajena in entidad.GetForeignKeys())
                ajena.SetConstraintName(ajena.GetConstraintName()!.ToLowerInvariant());

            foreach (var indice in entidad.GetIndexes())
                indice.SetDatabaseName(indice.GetDatabaseName()!.ToLowerInvariant());
        }
    }

    private static string ConGuiones(string nombre)
    {
        var sb = new StringBuilder(nombre.Length + 8);
        for (var i = 0; i < nombre.Length; i++)
        {
            var c = nombre[i];
            if (char.IsUpper(c))
            {
                if (i > 0 && nombre[i - 1] != '_') sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else sb.Append(c);
        }
        return sb.ToString();
    }
}
