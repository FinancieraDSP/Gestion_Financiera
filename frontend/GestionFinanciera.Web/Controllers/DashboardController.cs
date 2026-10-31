using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class DashboardController : Controller
    {
        private readonly ApiClient _api;

        public DashboardController(ApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            var modelo = (await _api.GetAsync<DashboardViewModel>("api/dashboard")).Requerir();
            return View(modelo);
        }
    }
}