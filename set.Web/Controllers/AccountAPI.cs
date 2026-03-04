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
            // 1. קודם כל מוצאים את המשתמש
            var user = await db.Users.FirstOrDefaultAsync(u => 
                u.Username == request.Username && u.Password == request.Password);

            // 2. בודקים אם הוא בכלל קיים לפני שממשיכים
            if (user == null)
            {
                return Unauthorized(new { message = "שם משתמש או סיסמה שגויים" });
            }

            // 3. רק עכשיו, כשאנחנו בטוחים שיש משתמש, מושכים את המשימות שלו
            var tasks = await db.TodoTasks
                .Where(t => t.UserId == user.Id)
                .ToListAsync();

            // 4. מחזירים את התשובה
            return Ok(new { 
                Message = "Login successful", 
                UserId = user.Id,
                Email = user.Email,
                EmailPassword = user.EmailPassword,
                // וודא שכאן כתוב Tasks עם T גדולה כדי להתאים ל-LoginResponse ב-Client
                Tasks = tasks.Select(t => new 
                {
                    t.Task,
                    t.Description,
                    t.CreatedAt,
                    t.DueDate
                }).ToList()
            });
        }

        [HttpPost("register")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, [FromServices] AppDbContext db)
        {
            // 1. בדיקה אם המשתמש קיים
            if (await db.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest(new { message = "משתמש כבר קיים" });

            // 2. יצירת המשתמש
            var newUser = new User 
            { 
                Username = request.Username ?? "", 
                Password = request.Password ?? "",
                Email = request.Email,
                EmailPassword = request.EmailPassword
            };

            // 3. שמירה ראשונה - כדי לקבל ID מהבסיס נתונים!
            db.Users.Add(newUser);
            await db.SaveChangesAsync(); 

            // עכשיו ל-newUser.Id יש ערך אמיתי (למשל 1, 2, 3...)

            // 4. יצירת המשימות עם ה-ID האמיתי
            if (request.tasks != null && request.tasks.Any())
            {
                var newTodoTasks = request.tasks.Select(t => new set.Web.Data.TodoTask 
                { 
                    Task = t.Task, 
                    Description = t.Description, 
                    CreatedAt = t.CreatedAt, 
                    DueDate = t.DueDate,
                    UserId = newUser.Id // עכשיו זה ה-ID הנכון
                }).ToList();

                db.TodoTasks.AddRange(newTodoTasks);
                
                // 5. שמירה שנייה - עבור המשימות
                await db.SaveChangesAsync();
            }

            return Ok(new { message = "נרשמת בהצלחה" });
        }

        public class TodoTask
        {
            public string Task { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;

            public DateTime CreatedAt { get; set; }

            public DateTime? DueDate { get; set; }

            public int UserId { get; set; }

            public TodoTask() {CreatedAt = DateTime.Now; }
        }

    

        // --- DTOs ---

        public class RegisterRequest
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
            public string? Email { get; set; }
            public string? EmailPassword { get; set; }
            public List<TodoTask>? tasks { get; set; }
        }

        public class LoginRequest // <--- הוספת המחלקה החסרה
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
            // הוספת השדות האלו למניעת שגיאות כששולחים אובייקט מלא מה-Client
            public string? Email { get; set; }
            public string? EmailPassword { get; set; }

            public List<TodoTask>? tasks { get; set; }
        }

        [HttpDelete("clear-database")]
        public async Task<IActionResult> ClearDatabase([FromServices] AppDbContext db)
        {
            // מחיקת כל המשימות
            db.TodoTasks.RemoveRange(db.TodoTasks);
            // מחיקת כל המשתמשים
            db.Users.RemoveRange(db.Users);
            
            await db.SaveChangesAsync();
            return Ok(new { message = "הנתונים נמחקו בהצלחה" });
        }
    }
}