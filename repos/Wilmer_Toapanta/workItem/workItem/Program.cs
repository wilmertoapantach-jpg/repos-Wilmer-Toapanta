using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using workItem.Data;
using workItem.Repository;
using workItem.Repository.IRepository;
using workItem.Service;
using workItem.Service.IService;
using workItem.Shared;

// Configuracion principal de la aplicacion WorkItem API
var myAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

// Obtener cadena de conexion y origenes CORS de appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
string[] origins = builder.Configuration.GetSection("Cors").Get<string[]>()!;;

// Registrar controladores MVC
builder.Services.AddControllers();

// Registrar AutoMapper con perfil de mapeo personalizado
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingConfig>();
});

// Registrar exploradores de endpoints para Swagger
builder.Services.AddEndpointsApiExplorer();

// Registrar generador de documentacion Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WorkItem API",
        Version = "v1"
    });
});

// Configurar politica CORS para permitir solicitudes desde origenes especificos
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins(origins)
              .WithMethods("PUT", "POST", "GET", "OPTIONS")
              .AllowAnyHeader();
    });
});

// Registrar DbContextFactory para soportar consultas paralelas
builder.Services.AddDbContextFactory<workManagementContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// Registrar Repositorio y Servicio mediante inyeccion de dependencias
builder.Services.AddScoped<IWorkItemRepository, WorkItemRepository>();
builder.Services.AddScoped<IWorkItemService, WorkItemService>();

// Registrar HttpClient para comunicacion con API externa de usuarios
builder.Services.AddHttpClient("UserApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7098");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Construir la aplicacion web con la configuracion registrada
var app = builder.Build();

// Configurar y habilitar Swagger solo en ambiente de desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WorkItem API V1");
    });
}

// Middleware para redirigir solicitudes HTTP a HTTPS
app.UseHttpsRedirection();

// Middleware para autorizacion
app.UseAuthorization();

// Aplicar la politica CORS definida
app.UseCors(myAllowSpecificOrigins);

// Mapear rutas de los controladores
app.MapControllers();

// Iniciar la aplicacion
app.Run();
