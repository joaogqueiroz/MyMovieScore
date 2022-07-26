using Microsoft.EntityFrameworkCore;
using MyMovieScore.Core.Entities;
using MyMovieScore.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMovieScore.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MyMovieScoreDbContext _dbContext;
        public UserRepository(MyMovieScoreDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<User> GetByIdAsync(int id)
        {
            return await _dbContext.Users.SingleOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> GetUserByLoginAndPasswordAsync(string email, string passwordHash)
        {
            return await _dbContext
                               .Users
                               .SingleOrDefaultAsync(u => u.Email == email && u.Password == passwordHash);
        }

        public async Task AddAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
            _dbContext.SaveChanges();
        }
    }
}
