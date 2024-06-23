using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infraestructure.Repository
{
    public interface IUserRepository 
    {
      Task AddUserAsync(User user);
      Task<User> GetByCodeAsync(string code);
      Task<string> GetByMailAsync(string mail);
      Task<string> GetUserNameAsync(string id);
    }
}
