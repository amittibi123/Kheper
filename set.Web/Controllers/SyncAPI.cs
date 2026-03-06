using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using set.Web.Data;

namespace set.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SyncAPI : ControllerBase
    {
        // ── GET: return all tasks + packages for a user ─────────
        [HttpGet("get/{userId}")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetUserData(int userId, [FromServices] AppDbContext db)
        {
            var tasks = await db.TodoTasks
                .Where(t => t.UserId == userId)
                .Select(t => new TaskDto
                {
                    Task        = t.Task,
                    Description = t.Description,
                    CreatedAt   = t.CreatedAt,
                    DueDate     = t.DueDate,
                    Path        = t.Path
                })
                .ToListAsync();

            var packages = await db.Packages
                .Where(p => p.UserId == userId)
                .Select(p => new PackageDto
                {
                    Name       = p.Name,
                    Path       = p.Path,
                    ParentPath = p.ParentPath
                })
                .ToListAsync();

            return Ok(new { Tasks = tasks, Packages = packages });
        }

        // ── POST: replace user data with what the client sends ──
        [HttpPost("sync")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Sync([FromBody] SyncRequest request, [FromServices] AppDbContext db)
        {
            // Delete old records for this user
            db.TodoTasks.RemoveRange(db.TodoTasks.Where(t => t.UserId == request.UserId));
            db.Packages.RemoveRange(db.Packages.Where(p => p.UserId == request.UserId));

            // Insert fresh records
            if (request.Tasks != null)
            {
                db.TodoTasks.AddRange(request.Tasks.Select(t => new TodoTask
                {
                    Task        = t.Task,
                    Description = t.Description,
                    CreatedAt   = t.CreatedAt,
                    DueDate     = t.DueDate,
                    Path        = t.Path,
                    UserId      = request.UserId
                }));
            }

            if (request.Packages != null)
            {
                db.Packages.AddRange(request.Packages.Select(p => new Package
                {
                    Name       = p.Name,
                    Path       = p.Path,
                    ParentPath = p.ParentPath,
                    UserId     = request.UserId
                }));
            }

            await db.SaveChangesAsync();
            return Ok(new { message = "Synced" });
        }

        // ── DTOs ───────────────────────────────────────────────

        public class TaskDto
        {
            public string    Task        { get; set; } = string.Empty;
            public string    Description { get; set; } = string.Empty;
            public DateTime  CreatedAt   { get; set; }
            public DateTime? DueDate     { get; set; }
            public string    Path        { get; set; } = "/";
        }

        public class PackageDto
        {
            public string Name       { get; set; } = string.Empty;
            public string Path       { get; set; } = "/";
            public string ParentPath { get; set; } = "/";
        }

        public class SyncRequest
        {
            public int               UserId   { get; set; }
            public List<TaskDto>?    Tasks    { get; set; }
            public List<PackageDto>? Packages { get; set; }
        }
    }
}
