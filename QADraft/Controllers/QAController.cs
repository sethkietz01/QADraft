using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using QADraft.Data;
using QADraft.Models;
using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

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

        [HttpPost]
        public IActionResult ManageQA(int qaId, string action)
        {
            if (action == "Update QA")
            {
                Debug.WriteLine("Update QA Reached");
                return RedirectToAction("EditQA", new { id = qaId });
            }
            else if (action == "Delete QA")
            {
                Debug.WriteLine("Delete QA reached");
                return RedirectToAction("DeleteQA", new { id = qaId });
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
                    return RedirectToAction("QAMenu", "Home", new { button = 1 });
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
        public IActionResult DeleteQA(string id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                var findQA = _context.GeekQAs.SingleOrDefault(q => q.Id == int.Parse(id));
                if (findQA != null)
                {
                    _context.Remove(findQA);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("QAMenu", "Home", new { button = 1 });
        }

        public async Task<IActionResult> Filter()
        {
            ViewBag.Users = _context.Users.ToList();
            var model = _context.GeekQAs.ToList();

            // Set initial content for the first tab
            ViewBag.InitialContent = await RenderViewAsync("Filter", model);
            return View(model);
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