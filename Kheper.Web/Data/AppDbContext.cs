using Microsoft.EntityFrameworkCore;

namespace Kheper.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<TodoTask> TodoTasks { get; set; }
        public DbSet<Package> Packages { get; set; }  // ← was ToDoPackage, now matches class name
    }

    // ─── Package (Folder) ──────────────────────────────────────
    public class Package
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = "/";
        public string ParentPath { get; set; } = "/";

        // Link to the user who owns this package
        public int UserId { get; set; }
        public User? User { get; set; }
    }

    // ─── TodoTask ──────────────────────────────────────────────
    public class TodoTask
    {
        public int Id { get; set; }
        public string Task { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }
        public string Path { get; set; } = "/";

        // Link to user
        public int UserId { get; set; }
        public User? User { get; set; }
    }

    // ─── User ──────────────────────────────────────────────────
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? EmailPassword { get; set; }

        // Navigation properties
        public List<TodoTask> Tasks { get; set; } = new();
        public List<Package> Packages { get; set; } = new();  // ← added
    }
}