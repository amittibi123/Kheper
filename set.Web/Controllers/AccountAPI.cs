using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // נדרש עבור FirstOrDefaultAsync
using set.Web.Data; // <--- זה התיקון הקריטי! זה מחבר את הקונטרולר לתיקיית ה-Data

namespace set.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountAPI : ControllerBase
    {
        // הזרקת ה-DbContext ישירות לפעולת ה-Login
        [HttpPost("login")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, [FromServices] AppDbContext db)
        {
            // בדיקה מול מסד הנתונים
            var user = await db.Users.FirstOrDefaultAsync(u => 
                u.Username == request.Username && u.Password == request.Password);

            if (user != null)
            {
                return Ok(new { Message = "Login successful", UserId = user.Id });
            }

            return Unauthorized(new { Message = "Invalid username or password" });
        }
        [HttpPost("register")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Register([FromBody] LoginRequest request, [FromServices] AppDbContext db)
        {
            // בדיקה אם המשתמש כבר קיים
            if (await db.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest(new { Message = "User already exists" });
            }

            var newUser = new User 
            { 
                Username = request.Username ?? "", 
                Password = request.Password ?? "" 
            };

            db.Users.Add(newUser);
            await db.SaveChangesAsync();

            return Ok(new { Message = "Registration successful" });
        }
    }

    public class LoginRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}