using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Autenticacion.Controllers
{
    public class UserDto
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Dni { get; set; }
        public string Code { get; set; }
    }
}