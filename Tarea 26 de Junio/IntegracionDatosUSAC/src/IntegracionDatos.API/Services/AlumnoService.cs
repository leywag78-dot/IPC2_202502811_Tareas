using System.Net.Http;
using System.Text.Json;
using IntegracionDatos.API.Models;

namespace IntegracionDatos.API.Services
{
    public class AlumnoService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AlumnoService> _logger;

        public AlumnoService(HttpClient httpClient, ILogger<AlumnoService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _httpClient.BaseAddress = new Uri("https://api.usac.edu/v1/");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<Alumno> ObtenerAlumnoAsync()
        {
            try
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                HttpResponseMessage response = await _httpClient.GetAsync("alumnos");
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Respuesta recibida exitosamente de la API");
                
                var alumno = JsonSerializer.Deserialize<Alumno>(jsonResponse, jsonOptions);
                
                return alumno ?? throw new InvalidOperationException("La deserialización devolvió un objeto nulo.");
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "Error de conexión HTTP");
                throw;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Error al deserializar el JSON");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en ObtenerAlumnoAsync");
                throw;
            }
        }
    }
}