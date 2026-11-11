using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spotta.Infrastructure.Migraciones
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gimnasio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    zona_horaria = table.Column<string>(type: "text", nullable: false),
                    moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    aforo_maximo = table.Column<int>(type: "integer", nullable: false),
                    ventana_presencia_min = table.Column<int>(type: "integer", nullable: false),
                    dias_minimos_racha = table.Column<int>(type: "integer", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gimnasio", x => x.id);
                    table.CheckConstraint("ck_gimnasio_aforo", "aforo_maximo > 0");
                    table.CheckConstraint("ck_gimnasio_dias_racha", "dias_minimos_racha BETWEEN 1 AND 7");
                    table.CheckConstraint("ck_gimnasio_ventana", "ventana_presencia_min BETWEEN 15 AND 480");
                });

            migrationBuilder.CreateTable(
                name: "perfil",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    gimnasio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    apellidos = table.Column<string>(type: "text", nullable: false),
                    dni = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    contrasena_cambiada = table.Column<bool>(type: "boolean", nullable: false),
                    dado_de_baja_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    dado_de_baja_por = table.Column<Guid>(type: "uuid", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    creado_por = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_perfil", x => x.id);
                    table.ForeignKey(
                        name: "fk_perfil_gimnasio_gimnasio_id",
                        column: x => x.gimnasio_id,
                        principalTable: "gimnasio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_perfil_perfil_creado_por",
                        column: x => x.creado_por,
                        principalTable: "perfil",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_perfil_perfil_dado_de_baja_por",
                        column: x => x.dado_de_baja_por,
                        principalTable: "perfil",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "plan_membresia",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    gimnasio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    duracion_dias = table.Column<int>(type: "integer", nullable: false),
                    precio = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    archivado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plan_membresia", x => x.id);
                    table.CheckConstraint("ck_plan_duracion", "duracion_dias > 0");
                    table.CheckConstraint("ck_plan_precio", "precio >= 0");
                    table.ForeignKey(
                        name: "fk_plan_membresia_gimnasio_gimnasio_id",
                        column: x => x.gimnasio_id,
                        principalTable: "gimnasio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dispositivo_push",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expo_push_token = table.Column<string>(type: "text", nullable: false),
                    push_activo = table.Column<bool>(type: "boolean", nullable: false),
                    visto_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dispositivo_push", x => x.id);
                    table.ForeignKey(
                        name: "fk_dispositivo_push_perfil_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "perfil",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notificacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    gimnasio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    cuerpo = table.Column<string>(type: "text", nullable: false),
                    enviada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    enviada_por = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notificacion", x => x.id);
                    table.ForeignKey(
                        name: "fk_notificacion_gimnasio_gimnasio_id",
                        column: x => x.gimnasio_id,
                        principalTable: "gimnasio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notificacion_perfil_enviada_por",
                        column: x => x.enviada_por,
                        principalTable: "perfil",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pase_dispositivo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    secreto_cifrado = table.Column<byte[]>(type: "bytea", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    revocado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revocado_por = table.Column<Guid>(type: "uuid", nullable: true),
                    ultimo_uso_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pase_dispositivo", x => x.id);
                    table.ForeignKey(
                        name: "fk_pase_dispositivo_perfil_revocado_por",
                        column: x => x.revocado_por,
                        principalTable: "perfil",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pase_dispositivo_perfil_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "perfil",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "membresia",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    gimnasio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_nombre = table.Column<string>(type: "text", nullable: false),
                    duracion_dias = table.Column<int>(type: "integer", nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateOnly>(type: "date", nullable: false),
                    precio_cobrado = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    metodo_pago = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cobrado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    cobrado_por = table.Column<Guid>(type: "uuid", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_membresia", x => x.id);
                    table.CheckConstraint("ck_membresia_duracion", "duracion_dias > 0");
                    table.CheckConstraint("ck_membresia_periodo", "fecha_fin >= fecha_inicio");
                    table.CheckConstraint("ck_membresia_precio", "precio_cobrado >= 0");
                    table.ForeignKey(
                        name: "fk_membresia_gimnasio_gimnasio_id",
                        column: x => x.gimnasio_id,
                        principalTable: "gimnasio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_membresia_perfil_cobrado_por",
                        column: x => x.cobrado_por,
                        principalTable: "perfil",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_membresia_perfil_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "perfil",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_membresia_plan_membresia_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plan_membresia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notificacion_lectura",
                columns: table => new
                {
                    notificacion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    leida_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notificacion_lectura", x => new { x.notificacion_id, x.usuario_id });
                    table.ForeignKey(
                        name: "fk_notificacion_lectura_notificacion_notificacion_id",
                        column: x => x.notificacion_id,
                        principalTable: "notificacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_notificacion_lectura_perfil_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "perfil",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pase_consumido",
                columns: table => new
                {
                    dispositivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paso = table.Column<long>(type: "bigint", nullable: false),
                    consumido_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pase_consumido", x => new { x.dispositivo_id, x.paso });
                    table.ForeignKey(
                        name: "fk_pase_consumido_pase_dispositivo_dispositivo_id",
                        column: x => x.dispositivo_id,
                        principalTable: "pase_dispositivo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "acceso",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    gimnasio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    membresia_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ocurrido_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    resultado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    motivo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    manual = table.Column<bool>(type: "boolean", nullable: false),
                    registrado_por = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_acceso", x => x.id);
                    table.ForeignKey(
                        name: "fk_acceso_gimnasio_gimnasio_id",
                        column: x => x.gimnasio_id,
                        principalTable: "gimnasio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_acceso_membresia_membresia_id",
                        column: x => x.membresia_id,
                        principalTable: "membresia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_acceso_perfil_registrado_por",
                        column: x => x.registrado_por,
                        principalTable: "perfil",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_acceso_perfil_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "perfil",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ajuste_membresia",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    membresia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dias = table.Column<int>(type: "integer", nullable: false),
                    motivo = table.Column<string>(type: "text", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    creado_por = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ajuste_membresia", x => x.id);
                    table.CheckConstraint("ck_ajuste_dias", "dias <> 0");
                    table.CheckConstraint("ck_ajuste_motivo", "length(trim(motivo)) > 0");
                    table.ForeignKey(
                        name: "fk_ajuste_membresia_membresia_membresia_id",
                        column: x => x.membresia_id,
                        principalTable: "membresia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ajuste_membresia_perfil_creado_por",
                        column: x => x.creado_por,
                        principalTable: "perfil",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_acceso_aforo",
                table: "acceso",
                columns: new[] { "gimnasio_id", "ocurrido_en" },
                descending: new[] { false, true },
                filter: "resultado = 'Permitido'");

            migrationBuilder.CreateIndex(
                name: "ix_acceso_membresia_id",
                table: "acceso",
                column: "membresia_id");

            migrationBuilder.CreateIndex(
                name: "ix_acceso_registrado_por",
                table: "acceso",
                column: "registrado_por");

            migrationBuilder.CreateIndex(
                name: "ix_acceso_usuario",
                table: "acceso",
                columns: new[] { "usuario_id", "ocurrido_en" },
                descending: new[] { false, true },
                filter: "resultado = 'Permitido'");

            migrationBuilder.CreateIndex(
                name: "ix_ajuste_membresia_creado_por",
                table: "ajuste_membresia",
                column: "creado_por");

            migrationBuilder.CreateIndex(
                name: "ix_ajuste_membresia_membresia_id",
                table: "ajuste_membresia",
                column: "membresia_id");

            migrationBuilder.CreateIndex(
                name: "ix_dispositivo_push_expo_push_token",
                table: "dispositivo_push",
                column: "expo_push_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dispositivo_push_usuario_id",
                table: "dispositivo_push",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_membresia_cobrado_por",
                table: "membresia",
                column: "cobrado_por");

            migrationBuilder.CreateIndex(
                name: "ix_membresia_gimnasio_id",
                table: "membresia",
                column: "gimnasio_id");

            migrationBuilder.CreateIndex(
                name: "ix_membresia_plan_id",
                table: "membresia",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_membresia_usuario_id",
                table: "membresia",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificacion_enviada_por",
                table: "notificacion",
                column: "enviada_por");

            migrationBuilder.CreateIndex(
                name: "ix_notificacion_gimnasio_id",
                table: "notificacion",
                column: "gimnasio_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificacion_lectura_usuario_id",
                table: "notificacion_lectura",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_pase_dispositivo_activo",
                table: "pase_dispositivo",
                column: "usuario_id",
                filter: "revocado_en IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_pase_dispositivo_revocado_por",
                table: "pase_dispositivo",
                column: "revocado_por");

            migrationBuilder.CreateIndex(
                name: "ix_perfil_creado_por",
                table: "perfil",
                column: "creado_por");

            migrationBuilder.CreateIndex(
                name: "ix_perfil_dado_de_baja_por",
                table: "perfil",
                column: "dado_de_baja_por");

            migrationBuilder.CreateIndex(
                name: "ux_perfil_dni_por_gimnasio",
                table: "perfil",
                columns: new[] { "gimnasio_id", "dni" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_plan_membresia_gimnasio_id",
                table: "plan_membresia",
                column: "gimnasio_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "acceso");

            migrationBuilder.DropTable(
                name: "ajuste_membresia");

            migrationBuilder.DropTable(
                name: "dispositivo_push");

            migrationBuilder.DropTable(
                name: "notificacion_lectura");

            migrationBuilder.DropTable(
                name: "pase_consumido");

            migrationBuilder.DropTable(
                name: "membresia");

            migrationBuilder.DropTable(
                name: "notificacion");

            migrationBuilder.DropTable(
                name: "pase_dispositivo");

            migrationBuilder.DropTable(
                name: "plan_membresia");

            migrationBuilder.DropTable(
                name: "perfil");

            migrationBuilder.DropTable(
                name: "gimnasio");
        }
    }
}
