using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using set.Web.Data;

namespace set.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountAPI : ControllerBase
    {
        [HttpPost("login")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, [FromServices] AppDbContext db)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => 
                u.Username == request.Username && u.Password == request.Password);

            if (user != null)
            {
                // מחזירים גם את נתוני המייל כדי שה-App יוכל להתעדכן
                return Ok(new { 
                    Message = "Login successful", 
                    UserId = user.Id,
                    Email = user.Email,
                    EmailPassword = user.EmailPassword
                });
            }

            return Unauthorized(new { Message = "Invalid username or password" });
        }

        [HttpPost("register")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, [FromServices] AppDbContext db)
        {
            if (await db.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest(new { Message = "משתמש כבר קיים" });

            var newUser = new User 
            { 
                Username = request.Username ?? "", 
                Password = request.Password ?? "",
                Email = request.Email,
                EmailPassword = request.EmailPassword
            };

            db.Users.Add(newUser);
            await db.SaveChangesAsync();
            return Ok(new { Message = "נרשמת בהצלחה" });
        }

        // --- DTOs ---

        public class RegisterRequest
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
            public string? Email { get; set; }
            public string? EmailPassword { get; set; }
        }

        public class LoginRequest // <--- הוספת המחלקה החסרה
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
            // הוספת השדות האלו למניעת שגיאות כששולחים אובייקט מלא מה-Client
            public string? Email { get; set; }
            public string? EmailPassword { get; set; }
        }
    }
}