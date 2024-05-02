using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Configuration
{
    public class SmtpSettings
    {
        public String Server { get; set; }
        public int Port { get; set; }
        public String SenderName { get; set; }
        public String UserName { get; set; }
        public String SenderEmail { get; set; }
        public String Password { get; set; }


    }
}
