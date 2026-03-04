using Microsoft.EntityFrameworkCore;

namespace set.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // כאן אנחנו מגדירים אילו טבלאות יהיו לנו
        public DbSet<User> Users { get; set; }

        public DbSet<TodoTask> TodoTasks { get; set; }
    }

    public class TodoTask
    {
        public int Id { get; set; } // <--- זה השדה שהיה חסר!
        public string Task { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }

        // שדה שיחבר את המשימה למשתמש ספציפי ב-DB
        public int UserId { get; set; } 
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? EmailPassword { get; set; }

        // הקשר למשימות
        public List<TodoTask> Tasks { get; set; } = new();
    }
}