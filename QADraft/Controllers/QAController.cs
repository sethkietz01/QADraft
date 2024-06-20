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
using QADraft.Utilities;

namespace QADraft.Controllers
{
    public class QAController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICompositeViewEngine _viewEngine;

        public QAController(ApplicationDbContext context,ICompositeViewEngine viewEngine)
        {
            _context = context;
            _viewEngine = viewEngine;
        }



        // Display the AddQA page
        [HttpGet]
        public IActionResult AddQA()
        {
            // Verify that the user is logged in, if not direct them to login page
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }

            // Get every user's id and name and place into a list
            var users = _context.Users.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.FirstName} {u.LastName}"
            }).ToList();

            // Create a new GeekQA instance and assign it the current DateTime and list of users.
            GeekQA model = new GeekQA
            {
                ErrorDate = DateTime.Now,
                FoundOn = DateTime.Now,
                Users = users
            };

            // Return the AddQA view with the GeekQA instance model. This will auto-fill the related input fields.
            return View(model);
        }

        // Process the AddQA POST
        [HttpPost]
        public IActionResult AddQA(GeekQA model)
        {
            // Verify that the submitted model state is valid (checks types and if all required model attributes are there)
            if (ModelState.IsValid)
            {
                try
                {
                    // Set the currently logged in User for the "SubmittedBy" field.
                    model.SubmittedBy = SessionUtil.GetFullName(HttpContext);
                    _context.GeekQAs.Add(model);
                    _context.SaveChanges();

                    // Automatically flag the user that commited an error (if it needs to be flagged).
                    var user = _context.Users.FirstOrDefault(u => u.Id == model.CommittedById);
                    if (user != null)
                    {
                        AutomaticFlag(user, model);
                    }

                    // Direct the user back to the inital AddQA page
                    return RedirectToAction("AddQA");
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the QA. Please try again.");
                }
            }

            // If model state is invalid, repopulate the users list
            model.Users = _context.Users.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.FirstName} {u.LastName}"
            }).ToList();

            // Direct the user back to the same page
            return View(model);
        }

        // Automatically flag the user's account if the QA's severity is 10.
        public void AutomaticFlag(User user, GeekQA model)
        {
            if (model.Severity == 10)
            {
                // Set the attribute isFlagged to true
                user.isFlagged = true;
                // Save the change to the database
                _context.SaveChanges();
            }

        }

        // Display the Filter page
        [HttpGet]
        public async Task<IActionResult> Filter(string dateFilter, DateTime? startDate, DateTime? endDate, int? committedBy, int? loggedBy, string category)
        {
            // Pass all users into the viewbag to populate drop-down selectors
            ViewBag.Users = _context.Users.ToList();
            // Pass all error categories into viewbag
            ViewBag.Categories = new List<SelectListItem>
            {
                new SelectListItem { Value = "Snipe-It", Text = "Snipe-It" },
                new SelectListItem { Value = "DocuSign", Text = "DocuSign" },
                new SelectListItem { Value = "Processes", Text = "Processes" },
                new SelectListItem { Value = "Desk Conduct", Text = "Desk Conduct" },
                new SelectListItem { Value = "Other", Text = "Other" }
            };

            // Fetch all QAs
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
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }
            var id = SessionUtil.GetId(HttpContext);
            var user = _context.Users.SingleOrDefault(u => u.Id == id);
            var qa = _context.GeekQAs.Where(q => q.CommittedById == user.Id).Include(q => q.FoundBy).ToList();

            return View(qa);
        }



        [HttpGet]
        public IActionResult AllGeekQAs()
        {
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }
            var qas = _context.GeekQAs
                .Include(q => q.CommittedBy)
                .Include(q => q.FoundBy)
                .ToList();

            //code to get data for piechart
            Console.WriteLine("Call PieChart");
            ViewBag.categoryDict = GetQADict("category");
            ViewBag.natureDict = GetQADict("nature");

            return View("_AllGeekQAs", qas);
        }




        [HttpGet]
        public IActionResult FlaggedAccounts()
        {
            if (!SessionUtil.IsAuthenticated(HttpContext))
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
            if (!SessionUtil.IsAuthenticated(HttpContext))
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
            if (action == "View/Edit")
            {
                Debug.WriteLine(qaId);
                Debug.WriteLine("Edit QA");
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
        public IActionResult ViewQA(int id)
        {
            if (!SessionUtil.IsAuthenticated(HttpContext))
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

            Console.WriteLine("QA:" + qa.Id);
            Console.WriteLine(qa.Users);
            Console.WriteLine(qa.CategoryOfError);
            Console.WriteLine(qa.NatureOfError);

            return View(qa);
        }


        [HttpGet]
        public IActionResult EditQA(int id)
        {
            Debug.WriteLine("id:");
            Debug.WriteLine(id);
            if (!SessionUtil.IsAuthenticated(HttpContext))
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
            if (!SessionUtil.IsAuthenticated(HttpContext))
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

        // Create the dictionary needed for making pie chart
        public Dictionary<string, int> GetQADict(string type)
        {
            // Get all QA categories and natures from db
            var qas = _context.GeekQAs
                .Select(qa => new { qa.CategoryOfError, qa.NatureOfError })
                .ToList();
            // Initilize empty string:int dictionary
            var Dict = new Dictionary<string, int>();
            // if-else to retrieve target QA attribute
            if (type == "category")
            {
                // Get each category using.GroupBy, then convert it to a dictionary
                Dict = qas
                    .GroupBy(qa => qa.CategoryOfError)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            else if (type == "nature")
            {
                // Get each nature using.GroupBy, then convert it to a dictionary
                Dict = qas
                    .GroupBy(qa => qa.NatureOfError)
                    .ToDictionary(g => g.Key, g => g.Count());
            }

            // Return the fetched dictionary
            return Dict;
        }

        // Display the category / nature piechart
        [HttpGet]
        public IActionResult PieChart()
        {
            // Return the piechart view
            return View();
        }

    }
}
