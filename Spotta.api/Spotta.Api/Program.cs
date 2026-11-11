using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spotta.Api.Autenticacion;
using Spotta.Api.Errores;
using Spotta.Application;
using Spotta.Application.Comun;
using Spotta.Application.Usuarios;
using Spotta.Infrastructure;
using Spotta.Infrastructure.Supabase;

// Puerta de arranque: el primer administrador no puede crearse por el endpoint
// de alta, que exige un administrador ya autenticado (RN-01). La configuración
// se arma aquí, en el anfitrión, y no dentro del caso de uso.
if (args is ["sembrar-admin", .. var resto])
{
    var configuracion = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true)
        .AddJsonFile("appsettings.Development.json", optional: true)
        .AddEnvironmentVariables()
        .Build();

    return await SembrarAdmin.EjecutarAsync(resto, configuracion);
}

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;
var urlSupabase = cfg["Supabase:Url"]
                  ?? throw new InvalidOperationException("Falta Supabase:Url en appsettings.json.");

// ── Datos y casos de uso ────────────────────────────────────────────────────
builder.Services.AddDbContext<SpottaDb>(o => o.UseNpgsql(CadenaDePostgres.Desde(cfg)));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHttpClient<ClienteAuthSupabase>(http =>
    ClienteAuthSupabase.Configurar(http, urlSupabase, cfg["Supabase:ServiceRoleKey"] ?? ""));
builder.Services.AddCasosDeUso();

// ── Identidad ───────────────────────────────────────────────────────────────
// La API no emite sesiones: valida el JWT de Supabase contra las claves
// públicas del proyecto (JWKS). No comparte ningún secreto con Supabase y no
// le pregunta nada en cada petición.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.Authority = $"{urlSupabase}/auth/v1";
        o.MapInboundClaims = false;
        o.TokenValidationParameters.ValidAudience = "authenticated";
        o.TokenValidationParameters.RoleClaimType = RolDesdeSupabase.TipoDeClaim;
    });
builder.Services.AddTransient<IClaimsTransformation, RolDesdeSupabase>();

// Por defecto todo exige sesión: un endpoint nuevo sin [Authorize] nace
// cerrado, no abierto. Lo público se declara con [AllowAnonymous].
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

// CORS sólo hace falta para el panel web: en React Native no existe.
const string PoliticaClientes = "clientes";
builder.Services.AddCors(o => o.AddPolicy(PoliticaClientes, p => p
    .WithOrigins(cfg.GetSection("Cors:Origenes").Get<string[]>() ?? ["http://localhost:5173"])
    .AllowAnyHeader()
    .AllowAnyMethod()));

// ── Errores ─────────────────────────────────────────────────────────────────
// Todo error sale como ProblemDetails. Un rechazo de negocio no es un fallo
// del servidor, así que no se registra como tal.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorDeErroresDeNegocio>();
builder.Services.Configure<ExceptionHandlerOptions>(o =>
    o.SuppressDiagnosticsCallback = c => c.Exception is ErrorDeNegocio);

// ── HTTP ────────────────────────────────────────────────────────────────────
// Los enums viajan como texto en minúsculas ("administrador", "efectivo"),
// igual que los códigos de error. Un número en su lugar se rechaza.
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false)))
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = contexto =>
    {
        var problema = new ValidationProblemDetails(contexto.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "La petición no tiene el formato esperado.",
        };
        problema.Extensions["code"] = "peticion_mal_formada";
        return new BadRequestObjectResult(problema) { ContentTypes = { "application/problem+json" } };
    });
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

app.UseCors(PoliticaClientes);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
return 0;
