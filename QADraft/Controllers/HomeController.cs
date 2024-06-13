using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QADraft.Data;
using QADraft.Models;
using QADraft.Utilities;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;
using System.Collections;
using QADraft.ViewModels;

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

        /*
         * TEST
        */

        public Dictionary<string, int> GetQADict(string type)
        {
            // Get all QA categories and natures from db
            var qas = _context.GeekQAs
                .Select(qa => new { qa.CategoryOfError, qa.NatureOfError })
                .ToList();
            var Dict = new Dictionary<string, int>();
            if (type == "category") {
                //convert into two lists, one for natures one for categories
                Dict = qas
                    .GroupBy(qa => qa.CategoryOfError)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            else if (type == "nature") {
                Dict = qas
                    .GroupBy(qa => qa.NatureOfError)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            Console.WriteLine(Dict);
            foreach (var kvp in Dict)
            {
                Console.WriteLine($"Category: {kvp.Key}, Count: {kvp.Value}");
            }
            return Dict;
        }

        public IActionResult TestChart()
        {
            ViewBag.categoryDict = GetQADict("category");
            ViewBag.natureDict = GetQADict("nature");

            return View();
        }

        public IActionResult PieChart()
        {

            return View();
        }

        /*
         * TEST END
        */ 

        public IActionResult Index()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            ViewBag.Layout = GetLayout();

            var viewModel = new EventsViewModel
            {
                EventList = _context.Events.ToList(), // Assuming _context is your ApplicationDbContext instance
                SingleEvent = new Events() // You can initialize this as needed
            };

            return View(viewModel);
        }


        // This is the initial login referencer
        [HttpGet]
        public IActionResult Login()
        {
            if (IsAuthenticated())
            {
                return RedirectToAction("Index");
            }
            ViewBag.Layout = GetLayout();

            return View();
        }

        // This is the actual function to see if the user has the correct login credentials, if not it redirects back to the login screen.
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            Debug.WriteLine("Login attempt");
            var user = _context.Users.SingleOrDefault(u => u.Username == username);

            if (user != null)
            {
                Debug.WriteLine($"User found: {username}");
                if (PasswordHasher.VerifyPassword(password, user.Password))
                {
                    Debug.WriteLine("Password verified");
                    HttpContext.Session.SetString("IsAuthenticated", "true");
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("FirstName", user.FirstName);
                    HttpContext.Session.SetString("LastName", user.LastName);
                    HttpContext.Session.SetInt32("Id", user.Id);
                    HttpContext.Session.SetString("Role", user.Role);

                    // Set the current DateTime as LastLogin for user in DB
                    user.LastLogin = DateTime.Now;
                    _context.SaveChanges();


                    return RedirectToAction("Index");
                }
                else
                {
                    Debug.WriteLine("Password verification failed");
                }
            }
            else
            {
                Debug.WriteLine("User not found");
            }

            ViewBag.Layout = GetLayout();
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.SetString("IsAuthenticated", "false");
            HttpContext.Session.SetString("Username", "");
            HttpContext.Session.SetInt32("Id", 0);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult PermissionsDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Outages()
        {
            return View();
        }

        public bool IsAuthenticated()
        {
            return HttpContext.Session.GetString("IsAuthenticated") == "true";
        }

        public int? GetId()
        {
            return HttpContext.Session.GetInt32("Id");
        }

        public string GetRole()
        {
            return HttpContext.Session.GetString("Role");
        }

        public string GetLayout()
        {
            string role = HttpContext.Session.GetString("Role");
            if (role == "Geek")
                return "~/Views/Shared/_LayoutGeek.cshtml";

            else
                return "~/Views/Shared/_Layout.cshtml";
        }
    }
}
