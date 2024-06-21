using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Auth
{
    public class AuthResult
    {
        public string Token { get; set; }

        public bool Result { get; set; }

        public List<string> Errors { get; set; }
        public string Message { get; set; }

        public string UserId { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }


    }
}
