using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class VerifyCodeRequestDto
    {
        public string EmailAddress { get; set; }
        public string Code { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Dni { get; set; }
    }
}
