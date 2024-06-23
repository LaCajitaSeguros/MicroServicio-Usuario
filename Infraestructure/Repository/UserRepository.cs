using Domain.Entities;
using Infraestructure.Persistence;
using Infraestructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infraestructure.Command
{
    public class UserRepository : IUserRepository
    {

        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddUserAsync(User user)
        {
            _dbContext.User.Add(user);
            await _dbContext.SaveChangesAsync();
        }


        public async Task<User>GetByCodeAsync(string code)
        {
            return await _dbContext.User.FirstOrDefaultAsync(c => c.Code == code);

        }

        public async Task<string> GetByMailAsync(string mail)
        {
            return await _dbContext.User
                   .Where(u => u.EmailAddress == mail)
                    .Select(u => u.UserId)
                    .FirstOrDefaultAsync();
        }

        public async Task<string> GetUserNameAsync(string id)
        {
            return await _dbContext.User
                   .Where(u => u.UserId == id)
                    .Select(u => u.Name)
                    .FirstOrDefaultAsync();
        }
    }
}
