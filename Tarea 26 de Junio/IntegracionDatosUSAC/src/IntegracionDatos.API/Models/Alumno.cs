namespace IntegracionDatos.API.Models
{
    public class Alumno
    {
        public int Id { get; set; }
        public string Carne { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int? Edad { get; set; }
        public DateTime? FechaRegistro { get; set; }
    }
}