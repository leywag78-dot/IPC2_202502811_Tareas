using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using IntegracionDatos.API.Models;
using IntegracionDatos.API.Data;

namespace IntegracionDatos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlumnosController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<AlumnosController> _logger;

        public AlumnosController(ApplicationDbContext dbContext, ILogger<AlumnosController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpPost("carga-masiva")]
        [RequestSizeLimit(100_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 100_000_000)]
        public async Task<IActionResult> CargarAlumnosMasivo(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest(new { mensaje = "No se recibió ningún archivo." });
            }

            string extension = Path.GetExtension(archivo.FileName).ToLower();
            if (extension != ".csv")
            {
                return BadRequest(new { mensaje = "Formato no válido. Solo se permite CSV." });
            }

            const int BATCH_SIZE = 1000;
            var batch = new List<Alumno>();
            int totalProcesados = 0;
            int totalErroneos = 0;
            var errores = new List<string>();

            try
            {
                await using (var stream = archivo.OpenReadStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string? header = await reader.ReadLineAsync();
                    int numeroLinea = 1;

                    while ((linea = await reader.ReadLineAsync()) != null)
                    {
                        numeroLinea++;

                        if (string.IsNullOrWhiteSpace(linea))
                            continue;

                        try
                        {
                            var alumno = MapearLineaACSV(linea, numeroLinea);
                            batch.Add(alumno);
                            totalProcesados++;

                            if (batch.Count >= BATCH_SIZE)
                            {
                                await GuardarLoteAsync(batch);
                                batch.Clear();
                            }
                        }
                        catch (Exception ex)
                        {
                            totalErroneos++;
                            errores.Add($"Error en línea {numeroLinea}: {ex.Message}");
                        }
                    }

                    if (batch.Any())
                    {
                        await GuardarLoteAsync(batch);
                    }
                }

                return Ok(new
                {
                    mensaje = "Procesamiento completado exitosamente.",
                    registrosProcesados = totalProcesados,
                    registrosErroneos = totalErroneos,
                    errores = errores.Any() ? errores : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar el archivo");
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
            string[] partes = linea.Split(',');

            if (partes.Length < 5)
            {
                throw new FormatException($"Se esperaban 5 columnas, se encontraron {partes.Length}");
            }

            return new Alumno
            {
                Carne = partes[0].Trim(),
                Nombre = partes[1].Trim(),
                Apellido = partes[2].Trim(),
                Email = partes[3].Trim(),
                Edad = int.TryParse(partes[4].Trim(), out int edad) ? edad : null,
                FechaRegistro = DateTime.UtcNow
            };
        }
    }
}