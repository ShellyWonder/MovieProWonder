using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MovieProWonder.Models.Database;

namespace MovieProWonder.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
            public DbSet<Collection> Collection { get; set; }
            public DbSet<Movie> Movie { get; set; }
    }
}