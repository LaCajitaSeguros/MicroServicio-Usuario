using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class ForgotPasswordRequestDto
    {

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }



    }
}
