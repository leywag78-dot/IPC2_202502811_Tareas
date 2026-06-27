using Microsoft.EntityFrameworkCore;
using IntegracionDatos.API.Data;
using IntegracionDatos.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar DbContext - Corrección 1: Usar el método correcto
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar HttpClient para el servicio - Corrección 2: Configurar correctamente
builder.Services.AddHttpClient<AlumnoService>(client =>
{
    client.BaseAddress = new Uri("https://api.usac.edu/v1/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Registrar servicios - Corrección 3: Usar el método correcto
builder.Services.AddScoped<AlumnoService>();

var app = builder.Build();

// Configurar pipeline de solicitudes
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Aplicar migraciones automáticamente - Corrección 4: Manejar posibles errores
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al aplicar migraciones a la base de datos");
    }
}

app.Run();
