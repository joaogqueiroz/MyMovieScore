using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Core.Entities
{
    public class Movie : BaseEntity
    {
        public Movie(string idIMDb, int userId, string name, string description, DateTime releaseDate, string genre, bool watched, float userScore)
        {
            IdIMDb = idIMDb;
            UserId = userId;
            Name = name;
            Description = description;
            ReleaseDate = releaseDate;
            Genre = genre;
            Watched = watched;
            UserScore = userScore;
        }

        public string IdIMDb { get; private set; }
        public int UserId { get; private set; }
        public User User { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public DateTime ReleaseDate { get; private set; }
        public string Genre { get; private set; }
        public bool Watched { get; private set; }
        public float UserScore { get; private set; }
        public void Update(bool watched, float userScore)
        {
            Watched = watched;
            UserScore = userScore;
        }
    }
}
