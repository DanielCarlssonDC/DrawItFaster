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


        // Words for game
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Word>().HasData(
                new Word { WordID = 1, WordString = "Apple" },
                new Word { WordID = 2, WordString = "Car" },
                new Word { WordID = 3, WordString = "Dog" },
                new Word { WordID = 4, WordString = "Cat" },
                new Word { WordID = 5, WordString = "House" },
                new Word { WordID = 6, WordString = "Sun" },
                new Word { WordID = 7, WordString = "Tree" },
                new Word { WordID = 8, WordString = "Computer" },
                new Word { WordID = 9, WordString = "Coffee" },
                new Word { WordID = 10, WordString = "Bike" }
            );
        }
    }
}