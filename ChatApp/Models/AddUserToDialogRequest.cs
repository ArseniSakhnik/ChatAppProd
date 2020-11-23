using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Models
{
    public class AddUserToDialogRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public int DialogId { get; set; }
    }
}
