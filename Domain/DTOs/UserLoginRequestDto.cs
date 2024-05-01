using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class UserLoginRequestDto 
    {
        [Required]
        public required string EmailAddress { get; set; }
        [Required]
        public required string Password { get; set; }


    }
}
