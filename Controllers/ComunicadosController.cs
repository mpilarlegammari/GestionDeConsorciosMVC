using Microsoft.AspNetCore.Mvc;

namespace GestionDeConsorciosMVC.Controllers
{
    public class ComunicadosController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public IActionResult MisComunicados()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            ViewData["ComunicadoId"] = id;
            return View();
        }
    }
}
