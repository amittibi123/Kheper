using Microsoft.EntityFrameworkCore;

namespace set.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // כאן אנחנו מגדירים אילו טבלאות יהיו לנו
        public DbSet<User> Users { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty; // הוספת ערך ברירת מחדל
        public string Password { get; set; } = string.Empty;
    }
}