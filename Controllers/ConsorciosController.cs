using Microsoft.AspNetCore.Mvc;

namespace GestionDeConsorciosMVC.Controllers
{
    public class ConsorciosController : Controller
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
        public IActionResult Details(int id)
        {
            ViewData["ConsorcioId"] = id;
            return View();
        }
    }
}
