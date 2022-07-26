using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Core.Entities
{
    public class ExternalRatings : BaseEntity
    {
        public ExternalRatings(string source, string value)
        {
            Source = source;
            Value = value;
        }

        public string Source { get; private set; }
        public string Value { get; private set; }
        public int IdMovie { get; private set; }
        public Movie Movie { get; private set; }

        public void Update(string source, string value)
        {
            Source = source;
            Value = value;
        }
    }
}
