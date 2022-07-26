using Microsoft.EntityFrameworkCore;
using MyMovieScore.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Infrastructure.Persistence
{
    public class MyMovieScoreDbContext : DbContext
    {
        public MyMovieScoreDbContext(DbContextOptions<MyMovieScoreDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<ExternalRatings> ExternalRatings { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
