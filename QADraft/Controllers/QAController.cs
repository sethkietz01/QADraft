using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QADraft.Data;
using QADraft.Models;
using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;

namespace QADraft.Controllers
{
    public class QAController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QAController> _logger;
        private readonly ICompositeViewEngine _viewEngine;

        public QAController(ApplicationDbContext context, ILogger<QAController> logger, ICompositeViewEngine viewEngine)
        {
            _context = context;
            _logger = logger;
            _viewEngine = viewEngine;
        }

        [HttpGet]
        public IActionResult QAMenu(int? button)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            ViewBag.button = button ?? 0;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadContent(int button)
        {
            ViewBag.Users = _context.Users.ToList();
            ViewBag.Categories = new List<SelectListItem>
            {
                new SelectListItem { Value = "Category1", Text = "Category1" },
                new SelectListItem { Value = "Category2", Text = "Category2" },
                new SelectListItem { Value = "Category3", Text = "Category3" },
                new SelectListItem { Value = "Category4", Text = "Category4" },
                new SelectListItem { Value = "DocuSign", Text = "DocuSign" }
            };

            var model = new List<GeekQA>();

            switch (button)
            {
                case 0:
                    model = await _context.GeekQAs.ToListAsync();
                    return PartialView("_Filter", model);
                case 1:
                    return PartialView("_BetweenDates");
                case 2:
                    model = await _context.GeekQAs.Include(q => q.CommittedBy).Include(q => q.FoundBy).ToListAsync();
                    return PartialView("_AllGeekQAs", model);
                case 3:
                    var flaggedAccounts = await _context.GeekQAs
                        .Include(q => q.CommittedBy)
                        .Include(q => q.FoundBy)
                        .Where(q => q.Severity >= 10)
                        .ToListAsync();
                    return PartialView("_FlaggedAccounts", flaggedAccounts);
                case 4:
                    return PartialView("_QADescriptions");
                default:
                    model = await _context.GeekQAs.ToListAsync();
                    return PartialView("_Filter", model);
            }
        }

        // Other action methods...
        public bool IsAuthenticated()
        {
            return HttpContext.Session.GetString("IsAuthenticated") == "true";
        }
    }
}
