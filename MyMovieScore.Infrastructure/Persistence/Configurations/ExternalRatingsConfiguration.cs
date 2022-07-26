using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyMovieScore.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Infrastructure.Persistence.Configurations
{
    public class ExternalRatingsConfiguration : IEntityTypeConfiguration<ExternalRatings>
    {
        public void Configure(EntityTypeBuilder<ExternalRatings> builder)
        {
            builder
              .HasKey(e => e.Id);

            builder
              .HasOne(e => e.Movie)
              .WithMany(e => e.ExternalRatings)
              .HasForeignKey(e => e.IdMovie);
        }
    }
}
