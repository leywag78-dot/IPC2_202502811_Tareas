**Fecha:** 26 de Junio, 2026  
**Estudiante:** Wagner Maximiliano Ley Monroy
**Carné:** 202502811

---

## Parte 1: Evaluación Conceptual y Buenas Prácticas

### Tabla Comparativa de Formatos Masivos

|Formato|Ventajas|Desventajas|
|---|---|---|
|**CSV**|• Ligero y de fácil lectura  <br>• Amplio soporte en múltiples aplicaciones  <br>• Bajo consumo de recursos  <br>• Fácil de generar y procesar|• No soporta datos jerárquicos o anidados  <br>• Sin validación de tipos de datos  <br>• Problemas con caracteres especiales y separadores  <br>• Sin metadatos o esquema definido|
|**XML**|• Soporte para datos jerárquicos y anidados  <br>• Auto-descripción mediante etiquetas  <br>• Validación mediante esquemas (XSD)  <br>• Estándar empresarial robusto|• Alta sobrecarga de tamaño (verboso)  <br>• Mayor consumo de memoria y CPU  <br>• Parsing más complejo y lento  <br>• Curva de aprendizaje más pronunciada|

---

### 1. Diferenciación de Procesos: Serialización y Deserialización

**Serialización:**  
Es el proceso de convertir un objeto en memoria (como una instancia de una clase `Alumno`) a un formato intercambiable, típicamente JSON o XML, para su almacenamiento o transmisión. Con `System.Text.Json`, se utiliza el método `JsonSerializer.Serialize()`.

csharp

string json = JsonSerializer.Serialize(alumno);

**Deserialización:**  
Es el proceso inverso: transformar datos en formato JSON (recibidos desde una API o archivo) a un objeto de tipo específico en memoria. Con `System.Text.Json`, se utiliza `JsonSerializer.Deserialize<T>()`.

csharp

Alumno alumno = JsonSerializer.Deserialize<Alumno>(jsonString);

**Diferencias técnicas clave:**

- **Dirección:** Serialización = objeto → JSON; Deserialización = JSON → objeto
    
- **Propósito:** Serialización para persistencia/envío; Deserialización para procesamiento/lectura
    
- **Manejo de tipos:** En deserialización, se debe especificar el tipo destino (`<T>`)
    
- **Configuración:** Ambos procesos pueden usar `JsonSerializerOptions` para personalizar el comportamiento
    

---

### 2. El Antipatrón del Rendimiento: Problema N+1

**¿Qué es el problema N+1?**  
Es un antipatrón de rendimiento donde se realizan múltiples consultas a la base de datos en lugar de una sola consulta optimizada. Ocurre cuando, durante la lectura de un archivo masivo, se ejecuta una consulta individual por cada registro procesado (1 consulta para obtener la lista + N consultas para cada elemento), resultando en N+1 llamadas a la base de datos.

**Ejemplo del problema:**

csharp

// MAL: 1 consulta para obtener la lista + N consultas individuales
var alumnos = await dbContext.Alumnos.ToListAsync();
foreach (var alumno in alumnos)
{
    var cursos = await dbContext.Cursos
        .Where(c => c.AlumnoId == alumno.Id)
        .ToListAsync(); // Se ejecuta N veces
}

**Estrategia de optimización por lotes (Batching):**

1. **Lectura por lotes (Batch Reading):** Procesar el archivo en bloques de tamaño definido (ej. 1000 registros) en lugar de todo el archivo de una vez.
    
2. **Inserción por lotes (Batch Insertion):** Acumular registros en una lista intermedia y realizar una sola operación de inserción:
    
    - Usar `AddRange()` para agregar múltiples entidades
        
    - Una única llamada a `SaveChangesAsync()` al final del ciclo
        

**Código optimizado:**

csharp

const int BATCH_SIZE = 1000;
var batch = new List<Alumno>();
await using var reader = new StreamReader(fileStream);
string line;
while ((line = await reader.ReadLineAsync()) != null)
{
    var alumno = MapearAlumno(line);
    batch.Add(alumno);
    if (batch.Count >= BATCH_SIZE)
    {
        dbContext.Alumnos.AddRange(batch);
        await dbContext.SaveChangesAsync();
        batch.Clear();
    }
}
// Procesar el último lote
if (batch.Any())
{
    dbContext.Alumnos.AddRange(batch);
    await dbContext.SaveChangesAsync();
}

**Beneficios del Batching:**

- Reduce drásticamente el número de llamadas a la base de datos (de N+1 a N/BATCH_SIZE + 1)
    
- Minimiza la sobrecarga de red y transacciones
    
- Evita la saturación de memoria RAM
    
- Mejora significativamente el rendimiento en archivos masivos


## Parte 2: Implementación Práctica en C#

### Desafío 1: Consumo de Endpoints y Deserialización

csharp

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
public class Alumno
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Carne { get; set; }
    public string Email { get; set; }
    public int? Edad { get; set; }
}
public class AlumnoService
{
    private readonly HttpClient _httpClient;
    public AlumnoService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _httpClient.BaseAddress = new Uri;
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }
    public async Task<Alumno> ObtenerAlumnoAsync()
    {
        try
        {
            // Configuración del serializador con insensibilidad a mayúsculas
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            // Realizar la petición GET
            HttpResponseMessage response = await _httpClient.GetAsync("alumnos");
            // Validar el código de estado HTTP
            response.EnsureSuccessStatusCode();
            // Leer el contenido JSON de la respuesta
            string jsonResponse = await response.Content.ReadAsStringAsync();
            // Deserializar el JSON a un objeto Alumno
            Alumno alumno = JsonSerializer.Deserialize<Alumno>(jsonResponse, jsonOptions);
            return alumno ?? throw new InvalidOperationException("La deserialización devolvió un objeto nulo.");
        }
        catch (HttpRequestException httpEx)
        {
            // Manejo de errores de red/HTTP
            Console.WriteLine($"Error de conexión HTTP: {httpEx.Message}");
            if (httpEx.StatusCode.HasValue)
            {
                Console.WriteLine($"Código de estado: {httpEx.StatusCode.Value}");
            }
            throw;
        }
        catch (JsonException jsonEx)
        {
            // Manejo de errores de deserialización
            Console.WriteLine($"Error al deserializar el JSON: {jsonEx.Message}");
            throw;
        }
        catch (Exception ex)
        {
            // Manejo de errores generales
            Console.WriteLine($"Error inesperado: {ex.Message}");
            throw;
        }
    }
}
// Ejemplo de uso
public class Program
{
    public static async Task Main()
    {
        using var httpClient = new HttpClient();
        var alumnoService = new AlumnoService(httpClient);
        try
        {
            Alumno alumno = await alumnoService.ObtenerAlumnoAsync();
            Console.WriteLine($"Alumno obtenido: {alumno.Nombre} {alumno.Apellido}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al procesar la solicitud: {ex.Message}");
        }
    }
}

---

### Desafío 2: Endpoint para Carga Masiva CSV

csharp

using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.EntityFrameworkCore;
[ApiController]
[Route("api/[controller]")]
public class AlumnosController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    public AlumnosController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    [HttpPost("carga-masiva")]
    [RequestSizeLimit(100_000_000)] // Límite de 100 MB
    [RequestFormLimits(MultipartBodyLengthLimit = 100_000_000)]
    public async Task<IActionResult> CargarAlumnosMasivo(IFormFile archivo)
    {
        // Validar que se haya enviado un archivo
        if (archivo == null || archivo.Length == 0)
        {
            return BadRequest(new { mensaje = "No se recibió ningún archivo. Por favor, suba un archivo CSV válido." });
        }
        // Validar extensión del archivo
        string extension = Path.GetExtension(archivo.FileName).ToLower();
        if (extension != ".csv")
        {
            return BadRequest(new { mensaje = "Formato de archivo no válido. Solo se permite CSV." });
        }
        const int BATCH_SIZE = 1000;
        var batch = new List<Alumno>();
        int totalRegistrosProcesados = 0;
        int totalRegistrosErroneos = 0;
        var errores = new List<string>();
        try
        {
            // Procesar el archivo usando StreamReader asíncrono
            await using (var stream = archivo.OpenReadStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                // Leer y descartar la cabecera (opcional)
                string header = await reader.ReadLineAsync();
                string? linea;
                int numeroLinea = 1;
                while ((linea = await reader.ReadLineAsync()) != null)
                {
                    numeroLinea++;
                    if (string.IsNullOrWhiteSpace(linea))
                        continue;
                    try
                    {
                        // Mapear la línea CSV a un objeto Alumno
                        Alumno alumno = MapearLineaACSV(linea, numeroLinea);
                        batch.Add(alumno);
                        totalRegistrosProcesados++;
                        // Si el lote alcanza el tamaño máximo, guardar en la base de datos
                        if (batch.Count >= BATCH_SIZE)
                        {
                            await GuardarLoteAsync(batch);
                            batch.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        totalRegistrosErroneos++;
                        errores.Add($"Error en línea {numeroLinea}: {ex.Message}");
                    }
                }
                // Guardar el último lote si quedan registros
                if (batch.Any())
                {
                    await GuardarLoteAsync(batch);
                    batch.Clear();
                }
            }
            // Construir la respuesta
            var resultado = new
            {
                mensaje = "Procesamiento de archivo completado exitosamente.",
                registrosProcesados = totalRegistrosProcesados,
                registrosErroneos = totalRegistrosErroneos,
                errores = errores.Any() ? errores : null
            };
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                mensaje = "Error interno al procesar el archivo.",
                detalle = ex.Message
            });
        }
    }
    private async Task GuardarLoteAsync(List<Alumno> lote)
    {
        await _dbContext.Alumnos.AddRangeAsync(lote);
        await _dbContext.SaveChangesAsync();
    }
    private Alumno MapearLineaACSV(string linea, int numeroLinea)
    {
        // Dividir la línea CSV (asumiendo que el separador es coma)
        string[] partes = linea.Split(',');
        // Validar que tenga el número correcto de columnas
        if (partes.Length < 5)
        {
            throw new FormatException($"La línea tiene {partes.Length} columnas, pero se esperaban al menos 5.");
        }
        // Mapear los campos (se debe ajustar según el formato del CSV)
        return new Alumno
        {
            Carne = partes[0].Trim(),
            Nombre = partes[1].Trim(),
            Apellido = partes[2].Trim(),
            Email = partes[3].Trim(),
            Edad = int.TryParse(partes[4].Trim(), out int edad) ? edad : (int?)null
        };
    }
}
// Modelo de Alumno para Entity Framework
public class Alumno
{
    public int Id { get; set; }
    public string Carne { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Email { get; set; }
    public int? Edad { get; set; }
}
// Contexto de base de datos
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Alumno> Alumnos { get; set; }
}

---

## Parte 3: Referencias Bibliográficas

- Facultad de Ingeniería, USAC. (2026). _Sesión 20: Integración de Datos. Consumo de APIs Externas y Carga Masiva (CSV/XML)_. Laboratorio del curso Introducción a la Programación y Computación 2. Guatemala.