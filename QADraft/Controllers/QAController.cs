using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QADraft.Data;
using QADraft.Models;
using System.Diagnostics;

namespace QADraft.Controllers
{
    public class QAController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<QAController> _logger;

        public QAController(ApplicationDbContext context, ILogger<QAController> logger)
        {
            _context = context;
            _logger = logger;
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

            var qa = _context.GeekQAs.Find(id); // Assuming your context name is _context and the table is GeekQAs
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

                    // Update the QA properties
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
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating GeekQA");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the QA. Please try again.");
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


        [HttpGet]
        public IActionResult DeleteQA(string id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                Debug.WriteLine("Valid ModelState");
                var findQA = _context.GeekQAs.SingleOrDefault(q => q.Id == int.Parse(id));
                if (findQA != null)
                {
                    Debug.WriteLine("User Exists");
                    _context.Remove(findQA);
                    _context.SaveChanges();
                    Debug.WriteLine("User Deleted");
                }
            }
            Debug.WriteLine("End");
            return RedirectToAction("QAMenu", "Home", new { button = 1 });

        }


        public bool IsAuthenticated()
        {
            return HttpContext.Session.GetString("IsAuthenticated") == "true";
        }


    }
}
