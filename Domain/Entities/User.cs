using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public required string UserId { get; set; }

        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Dni { get; set; }
        public required string EmailAddress { get; set; }
        public required string Password { get; set; }


    }
}
