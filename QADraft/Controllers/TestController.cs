using Microsoft.AspNetCore.Mvc;
using QADraft.Utilities;

namespace QADraft.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            // Test password hashing
            string password = "testpassword";
            string hashedPassword = PasswordHasher.HashPassword(password);

            return Content($"Original Password: {password}, Hashed Password: {hashedPassword}");
        }
    }
}
