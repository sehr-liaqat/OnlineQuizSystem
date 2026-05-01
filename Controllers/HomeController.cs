using Microsoft.AspNetCore.Mvc;
using OnlineQuizSystem.Data;

namespace OnlineQuizSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Login", "Account");
        }
    }
}