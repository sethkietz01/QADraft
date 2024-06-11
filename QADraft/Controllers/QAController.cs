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
        public IActionResult AddQA()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            var users = _context.Users.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.FirstName} {u.LastName}"
            }).ToList();

            GeekQA model = new GeekQA
            {
                ErrorDate = DateTime.Now,
                FoundOn = DateTime.Now,
                Users = users
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult AddQA(GeekQA model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.GeekQAs.Add(model);
                    _context.SaveChanges();

                    //Get user and flag if QA severity is 10
                    var user = _context.Users.FirstOrDefault(u => u.Id == model.CommittedById);
                    if (user != null)
                    {
                        AutomaticFlag(user, model);
                    }


                    return RedirectToAction("AddQA");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving GeekQA");
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the QA. Please try again.");
                }
            }

            // If model state is invalid, repopulate the users list
            model.Users = _context.Users.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.FirstName} {u.LastName}"
            }).ToList();

            return View(model);
        }


        /*
         *      UTIL FLAG FUNCTION
         */ 
        public void AutomaticFlag(User user, GeekQA model)
        {
            if (model.Severity == 10)
            {
                user.isFlagged = true;
                _context.SaveChanges();
            }

        }


        [HttpGet]
        public async Task<IActionResult> Filter(string dateFilter, DateTime? startDate, DateTime? endDate, int? committedBy, int? loggedBy, string category)
        {
            ViewBag.Users = _context.Users.ToList();
            ViewBag.Categories = new List<SelectListItem>
            {
                new SelectListItem { Value = "Snipe-It", Text = "Snipe-It" },
                new SelectListItem { Value = "DocuSign", Text = "DocuSign" },
                new SelectListItem { Value = "Processes", Text = "Processes" },
                new SelectListItem { Value = "Desk Conduct", Text = "Desk Conduct" },
                new SelectListItem { Value = "Other", Text = "Other" }
            };

            var qas = _context.GeekQAs.AsQueryable();

            if (!string.IsNullOrEmpty(dateFilter))
            {
                if (startDate.HasValue && endDate.HasValue)
                {
                    // Both start date and end date are provided
                    qas = qas.Where(q => q.ErrorDate >= startDate.Value && q.ErrorDate <= endDate.Value);
                }
                else if (startDate.HasValue)
                {
                    // Only start date is provided, filter from start date to current date
                    qas = qas.Where(q => q.ErrorDate >= startDate.Value && q.ErrorDate <= DateTime.Now);
                }
                else if (endDate.HasValue)
                {
                    // Only end date is provided, filter from the minimum date to current date
                    qas = qas.Where(q => q.ErrorDate >= DateTime.MinValue && q.ErrorDate <= endDate.Value);
                }
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



        [HttpGet]
        public IActionResult YourQAs()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }
            var id = GetId();
            var user = _context.Users.SingleOrDefault(u => u.Id == id);
            var qa = _context.GeekQAs.Where(q => q.CommittedById == user.Id).Include(q => q.FoundBy).ToList();

            return View(qa);
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




        [HttpGet]
        public IActionResult QADescriptions()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }
            return View("_QADescriptions");
        }





        [HttpPost]
        public IActionResult ManageQA(int qaId, string action, string source)
        {
            Debug.WriteLine("ManagQA reached");
            Debug.WriteLine("Action: ", action);
            if (action == "Update")
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
                    return RedirectToAction("AllGeekQAs");
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

        public int? GetId()
        {
            return HttpContext.Session.GetInt32("Id");
        }



    }
}
