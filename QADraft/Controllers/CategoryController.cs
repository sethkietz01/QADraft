using Microsoft.AspNetCore.Mvc;

namespace QADraft.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
