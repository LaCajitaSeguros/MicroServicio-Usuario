﻿using Domain.Entities;
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



    }
}
