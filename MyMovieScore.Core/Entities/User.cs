using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Core.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; private set; }
        public string Password { get; private set; }
        public string Name { get; private set; }
    }
}
