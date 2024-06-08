using System.ComponentModel.DataAnnotations;

namespace Autenticacion.Controllers
{
    public class CheckEmailRequestDto
    {

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

    }
}