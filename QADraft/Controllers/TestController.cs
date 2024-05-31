using Microsoft.AspNetCore.Mvc;
using QADraft.Utilities;
using System.Diagnostics;

namespace QADraft.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            // Test password hashing
            string password = "testpassword";
            string hashedPassword = PasswordHasher.HashPassword(password);

            // Verify the hashed password
            bool isVerified = PasswordHasher.VerifyPassword(password, hashedPassword);

            // Print to Visual Studio Output window
            Debug.WriteLine($"Original Password: {password}");
            Debug.WriteLine($"Hashed Password: {hashedPassword}");
            Debug.WriteLine($"Verification: {isVerified}");

            // Return some content to ensure the endpoint is working
            return Content($"Original Password: {password}, Hashed Password: {hashedPassword}, Verification: {isVerified}");
        }
    }
}
