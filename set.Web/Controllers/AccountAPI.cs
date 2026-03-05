using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using set.Web.Data;

namespace set.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountAPI : ControllerBase
    {
        // ─── LOGIN ─────────────────────────────────────────────────
        [HttpPost("login")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, [FromServices] AppDbContext db)
        {
            var user = await db.Users.FirstOrDefaultAsync(u =>
                u.Username == request.Username && u.Password == request.Password);

            if (user == null)
                return Unauthorized(new { message = "שם משתמש או סיסמה שגויים" });

            // Load tasks from DB
            var tasks = await db.TodoTasks
                .Where(t => t.UserId == user.Id)
                .ToListAsync();

            // Load packages from DB  ← added
            var packages = await db.Packages
                .Where(p => p.UserId == user.Id)
                .ToListAsync();

            return Ok(new
            {
                Message = "Login successful",
                UserId = user.Id,
                Email = user.Email,
                EmailPassword = user.EmailPassword,
                Tasks = tasks.Select(t => new
                {
                    t.Task,
                    t.Description,
                    t.CreatedAt,
                    t.DueDate,
                    t.Path
                }).ToList(),
                // ← return packages to client
                Packages = packages.Select(p => new
                {
                    p.Name,
                    p.Path,
                    p.ParentPath
                }).ToList()
            });
        }

        // ─── REGISTER ──────────────────────────────────────────────
        [HttpPost("register")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, [FromServices] AppDbContext db)
        {
            if (await db.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest(new { message = "משתמש כבר קיים" });

            var newUser = new User
            {
                Username = request.Username ?? "",
                Password = request.Password ?? "",
                Email = request.Email,
                EmailPassword = request.EmailPassword
            };

            db.Users.Add(newUser);
            await db.SaveChangesAsync(); // save first to get the real Id

            // Save tasks
            if (request.tasks != null && request.tasks.Any())
            {
                var newTodoTasks = request.tasks.Select(t => new set.Web.Data.TodoTask
                {
                    Task = t.Task,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt,
                    DueDate = t.DueDate,
                    Path = t.Path,
                    UserId = newUser.Id
                }).ToList();

                db.TodoTasks.AddRange(newTodoTasks);
            }

            // Save packages  ← added
            if (request.packages != null && request.packages.Any())
            {
                var newPackages = request.packages.Select(p => new set.Web.Data.Package
                {
                    Name = p.Name,
                    Path = p.Path,
                    ParentPath = p.ParentPath,
                    UserId = newUser.Id
                }).ToList();

                db.Packages.AddRange(newPackages);
            }

            await db.SaveChangesAsync();

            return Ok(new { message = "נרשמת בהצלחה" });
        }

        // ─── CLEAR DB (dev tool) ───────────────────────────────────
        [HttpDelete("clear-database")]
        public async Task<IActionResult> ClearDatabase([FromServices] AppDbContext db)
        {
            db.TodoTasks.RemoveRange(db.TodoTasks);
            db.Packages.RemoveRange(db.Packages);   // ← added
            db.Users.RemoveRange(db.Users);
            await db.SaveChangesAsync();
            return Ok(new { message = "הנתונים נמחקו בהצלחה" });
        }

        // ─── DTOs ──────────────────────────────────────────────────

        public class PackageDto
        {
            public string Name { get; set; } = string.Empty;
            public string Path { get; set; } = "/";
            public string ParentPath { get; set; } = "/";
        }

        public class TodoTaskDto
        {
            public string Task { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public DateTime? DueDate { get; set; }
            public string Path { get; set; } = "/";
            public TodoTaskDto() { CreatedAt = DateTime.Now; }
        }

        public class RegisterRequest
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
            public string? Email { get; set; }
            public string? EmailPassword { get; set; }
            public List<TodoTaskDto>? tasks { get; set; }
            public List<PackageDto>? packages { get; set; }   // ← added
        }

        public class LoginRequest
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
            public string? Email { get; set; }
            public string? EmailPassword { get; set; }
            public List<TodoTaskDto>? tasks { get; set; }
            public List<PackageDto>? packages { get; set; }   // ← added
        }
    }
}