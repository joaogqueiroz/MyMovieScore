using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Core.DTOs
{
    public partial class ImDbInforDto
    {
        public string ImdbId { get; set; }
        public string Title { get; set; }
        public string Plot { get; set; }
        public string Released { get; set; }
        public string Genre { get; set; }
        public List<Rating> Ratings { get; set; }
    }

    public partial class Rating
    {
        public string Source { get; set; }
        public string Value { get; set; }
    }
}
