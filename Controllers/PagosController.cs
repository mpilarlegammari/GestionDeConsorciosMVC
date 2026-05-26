using Microsoft.AspNetCore.Mvc;

namespace GestionDeConsorciosMVC.Controllers
{
    public class PagosController : Controller
    {
        [HttpGet]
        public IActionResult InformarPago()
        {
            return View();
        }

        [HttpGet]
        public IActionResult MisPagos()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            ViewData["PagoId"] = id;
            return View();
        }
    }
}
