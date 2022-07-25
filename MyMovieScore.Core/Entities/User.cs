using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Core.Entities
{
    public class User : BaseEntity
    {
        public User(string email, string password, string name)
        {
            Email = email;
            Password = password;
            Name = name;
        }

        public string Email { get; private set; }
        public string Password { get; private set; }
        public string Name { get; private set; }
        public List<Movie> Movies { get; private set; }

    }
}
