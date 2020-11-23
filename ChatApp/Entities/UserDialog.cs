using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Entities
{
    public class UserDialog
    {
        public string  Username { get; set; }
        public User User { get; set; }
        public int DialogId { get; set; }
        public Dialog Dialog { get; set; }
    }
}
