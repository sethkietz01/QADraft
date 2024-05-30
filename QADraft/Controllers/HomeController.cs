using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QADraft.Data;
using QADraft.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QADraft.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Action to render the QA Menu
        [HttpGet]
        public IActionResult QAMenu(int? button)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }
            if (button != null)
            {
                ViewBag.button = button;
            }
            else
            {
                ViewBag.button = 0;
            }

            return View(ViewBag);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (IsAuthenticated())
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            
            if (user != null && user.Password == password)
            {
                HttpContext.Session.SetString("IsAuthenticated", "true");
                HttpContext.Session.SetString("username", user.Username);
                HttpContext.Session.SetString("FirstName", user.FirstName);
                HttpContext.Session.SetString("LastName", user.LastName);
                HttpContext.Session.SetInt32("Id", user.Id);
                HttpContext.Session.SetString("Role", user.Role);

                return RedirectToAction("Index");
            }

            return View();
        }



        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.SetString("IsAuthenticated", "false");
            HttpContext.Session.SetString("username", "");
            HttpContext.Session.SetInt32("Id", -1);
            return RedirectToAction("Login");
        }

        public IActionResult Index()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpGet]
        public IActionResult QADescriptions()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }
            return PartialView("_QADescriptions");
        }

        [HttpGet]
        public IActionResult DetailedQAReports()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpGet]
        public IActionResult UserOptions()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            var qas = _context.Users
                .ToList();
            return View(qas);
        }

        [HttpGet]
        public IActionResult AddUser()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpPost]
        public IActionResult AddUser(User user)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("UserOptions");
            }
            return View(user);
        }

        [HttpGet]
        public IActionResult EditUser(string Id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }
            int intId = int.Parse(Id);
            var userData = _context.Users.SingleOrDefault(u => u.Id == intId);
            if (userData == null)
            {
                return View();
            }

            return View(userData);
        }

        [HttpPost]
        public IActionResult EditUser(User user, string id, string action)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }
            else if (ModelState.IsValid)
            {
                Debug.WriteLine("Valid ModelState");
                var existingUser = _context.Users.SingleOrDefault(u => u.Id == int.Parse(id));
                if (existingUser != null)
                {
                    Debug.WriteLine("User Exists");
                    if (action == "Update User")
                    {
                        Debug.WriteLine("Update User");
                        existingUser.Username = user.Username;
                        existingUser.Password = user.Password;
                        existingUser.FirstName = user.FirstName;
                        existingUser.LastName = user.LastName;
                        existingUser.Email = user.Email;
                        existingUser.Role = user.Role;
                        _context.SaveChanges();
                    }
                    else if (action == "Delete User")
                    {
                        _context.Remove(existingUser);
                        _context.SaveChanges();
                    }
                }
            }
            Debug.WriteLine("End");
            return RedirectToAction("UserOptions");
        }


        public IActionResult Links()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Settings()
        {
            if (!IsAuthenticated() || GetId() < 0)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.SingleOrDefault(u => u.Id == GetId());
            if (user != null)
            {
                return View(user);
            }

            return View();
        }

        [HttpPost]
        public IActionResult Settings(User updateUser)
        {
            if (!IsAuthenticated() || GetId() < 0)
            {
                return RedirectToAction("Login");
            }
            else if (ModelState.IsValid)
            {
                var user = _context.Users.SingleOrDefault(u => u.Id == GetId());
                if (user != null)
                {
                    user.Username = updateUser.Username;
                    user.Password = updateUser.Password;
                    user.FirstName = updateUser.FirstName;
                    user.LastName = updateUser.LastName;
                    user.Email = updateUser.Email;
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("Index");
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
            return PartialView("_AllGeekQAs", qas);
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
                    return RedirectToAction("Index");
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
