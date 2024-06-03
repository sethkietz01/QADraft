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

        [HttpGet]
        public IActionResult BetweenDates()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }
            return PartialView("_BetweenDates");
        }

        [HttpPost]
        public IActionResult BetweenDates(DateTime startDate, DateTime endDate)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            var qas = _context.GeekQAs
                .Include(q => q.CommittedBy)
                .Include(q => q.FoundBy)
                .Where(q => q.ErrorDate >= startDate && q.ErrorDate <= endDate)
                .ToList();
            return PartialView("_BetweenDates", qas);
        }

        [HttpGet]
        public IActionResult AllGeekQAs()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }
            var qas = _context.GeekQAs
                .Include(q => q.CommittedBy)
                .Include(q => q.FoundBy)
                .ToList();
            
            return View("_AllGeekQAs", qas);
        }

        [HttpGet]
        public IActionResult FlaggedAccounts()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            var flaggedAccounts = _context.GeekQAs
                .Include(q => q.CommittedBy)
                .Include(q => q.FoundBy)
                .Where(q => q.Severity >= 10) // Example condition for flagged accounts
                .ToList();
            return PartialView("_FlaggedAccounts", flaggedAccounts);
        }

        [HttpPost]
        public IActionResult ManageQA(int qaId, string action, string source)
        {
            if (action == "Update QA")
            {
                Debug.WriteLine("Update QA Reached");
                return RedirectToAction("EditQA", new { id = qaId });
            }
            else if (action == "Delete QA")
            {
                Debug.WriteLine("Delete QA reached");
                return RedirectToAction("DeleteQA", new { id = qaId, src = source });
            }

            return RedirectToAction("Index"); // Default redirection
        }

        [HttpGet]
        public IActionResult EditQA(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            var qa = _context.GeekQAs.Find(id);
            if (qa == null)
            {
                return NotFound();
            }

            var users = _context.Users.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.FirstName} {u.LastName}"
            }).ToList();

            qa.Users = users;
            return View(qa);
        }

        [HttpPost]
        public IActionResult EditQA(GeekQA model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var qa = _context.GeekQAs.Find(model.Id);
                    if (qa == null)
                    {
                        return NotFound();
                    }

                    qa.CommittedById = model.CommittedById;
                    qa.FoundById = model.FoundById;
                    qa.CategoryOfError = model.CategoryOfError;
                    qa.NatureOfError = model.NatureOfError;
                    qa.Severity = model.Severity;
                    qa.CustomerName = model.CustomerName;
                    qa.UnitId = model.UnitId;
                    qa.ErrorDate = model.ErrorDate;
                    qa.FoundOn = model.FoundOn;
                    qa.Description = model.Description;

                    _context.Update(qa);
                    _context.SaveChanges();
                    return RedirectToAction("QAMenu", new { button = 0 });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating GeekQA");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the QA. Please try again.");
                }
            }

            model.Users = _context.Users.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.FirstName} {u.LastName}"
            }).ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult DeleteQA(int id, string src)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }
            var findQA = _context.GeekQAs.SingleOrDefault(q => q.Id == id);
            if (findQA != null)
            {
                _context.Remove(findQA);
                _context.SaveChanges();
            }
            if (src == "Filter")
                return RedirectToAction("Filter");

            return RedirectToAction("AllGeekQAs");
        }

        [HttpGet]
        public async Task<IActionResult> Filter(string dateFilter, DateTime? startDate, DateTime? endDate, int? committedBy, int? loggedBy, string category)
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

            var qas = _context.GeekQAs.AsQueryable();

            if (!string.IsNullOrEmpty(dateFilter) && startDate.HasValue && endDate.HasValue)
            {
                qas = qas.Where(q => q.ErrorDate >= startDate.Value && q.ErrorDate <= endDate.Value);
            }

            if (committedBy.HasValue)
            {
                qas = qas.Where(q => q.CommittedById == committedBy.Value);
            }

            if (loggedBy.HasValue)
            {
                qas = qas.Where(q => q.FoundById == loggedBy.Value);
            }

            if (!string.IsNullOrEmpty(category))
            {
                qas = qas.Where(q => q.CategoryOfError == category);
            }

            var model = await qas.ToListAsync();

            ViewBag.InitialContent = await RenderViewAsync("_Filter", model);
            return PartialView("_Filter", model);
        }

        private async Task<string> RenderViewAsync(string viewName, object model)
        {
            ViewData.Model = model;
            using (var writer = new StringWriter())
            {
                var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} does not match any available view");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return writer.GetStringBuilder().ToString();
            }
        }

        public bool IsAuthenticated()
        {
            return HttpContext.Session.GetString("IsAuthenticated") == "true";
        }
    }
}
