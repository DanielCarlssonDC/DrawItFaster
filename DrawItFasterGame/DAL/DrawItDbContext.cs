using DrawItFaster.Models;
using DrawItFasterGame.Models;
using Microsoft.EntityFrameworkCore;

namespace DrawItFaster.DAL
{
    public class DrawItDbContext : DbContext
    {
        public DrawItDbContext(DbContextOptions<DrawItDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Word> Words { get; set; }
    }
}