using Microsoft.EntityFrameworkCore;
using Spotta.Api;
using Spotta.Infrastructure;

// Puerta de arranque: el primer administrador no puede crearse por el endpoint
// de alta, que exige un administrador ya autenticado (RN-01).
if (args is ["sembrar-admin", .. var resto])
{
    return await SembrarAdmin.EjecutarAsync(resto);
}

var builder = WebApplication.CreateBuilder(args);

// La cadena real vive en user-secrets o en el entorno, nunca en el repositorio.
var cadena = builder.Configuration.GetConnectionString("Postgres")
             ?? "Host=localhost;Port=5432;Database=spotta;Username=postgres;Password=postgres";

builder.Services.AddDbContext<SpottaDb>(o => o.UseNpgsql(cadena));

// CORS sólo hace falta para el panel web: en React Native no existe.
const string PoliticaClientes = "clientes";
builder.Services.AddCors(o => o.AddPolicy(PoliticaClientes, p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:Origenes").Get<string[]>()
                 ?? ["http://localhost:5173"])
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(PoliticaClientes);

app.MapGet("/api/salud", async (SpottaDb db, CancellationToken ct) =>
{
    var baseDeDatos = await db.Database.CanConnectAsync(ct) ? "conectada" : "sin conexion";
    return TypedResults.Ok(new Salud("Spotta.Api", "0.1.0", baseDeDatos, DateTimeOffset.UtcNow));
})
.WithName("Salud");

app.Run();
return 0;

internal sealed record Salud(string Servicio, string Version, string BaseDeDatos, DateTimeOffset Ahora);
