using Microsoft.AspNetCore.Mvc;

namespace GestionDeConsorciosMVC.Controllers
{
    public class ExpensasController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Generar()
        {
            return View();
        }

        [HttpGet]
        public IActionResult MisExpensas()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            ViewData["ExpensaId"] = id;
            return View();
        }
    }
}
