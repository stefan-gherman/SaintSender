using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintSender.Core.Entities
{
    public class Email
    {
        public string Read { get; set; }
        public string From { get; set; }
        public DateTime DateReceived { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string UniqueID { get; set; }
    }
}
