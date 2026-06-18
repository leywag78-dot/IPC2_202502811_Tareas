using Microsoft.AspNetCore.Mvc;
using ControlAcademicMvc.Models;

namespace ControlAcademicMvc.Controllers
{
    public class EstudianteController : Controller
    {
        // Base de datos en memoria
        private static readonly List<Estudiante> _estudiantes = new()
        {
            new Estudiante { Carne = 2026012, Nombre = "Fernando Velasquez", Promedio = 91.5 },
            new Estudiante { Carne = 2026045, Nombre = "Maria Mercedes", Promedio = 84.0 }
        };

        // GET: /Estudiante/Listar
        public IActionResult Listar()
        {
            return View(_estudiantes);
        }
    }
}
