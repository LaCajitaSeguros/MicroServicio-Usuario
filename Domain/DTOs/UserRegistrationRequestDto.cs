using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class UserRegistrationRequestDto
    {
        [Required]
        public required string Name { get; set; }
        [Required]
        public required string LastName { get; set; }
        [Required]
        public required string Dni { get; set; }
        [Required]
        public required string EmailAddress { get; set; }
        [Required]
        public required string Password { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }



    }
}
