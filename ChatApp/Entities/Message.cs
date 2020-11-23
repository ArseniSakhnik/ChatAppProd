using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChatApp.Entities
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        [JsonIgnore]
        public User Sender { get; set; }
        public string SenderUsername { get; set; }
        [JsonIgnore]
        public Dialog Dialog { get; set; }
        [Required, MinLength(1)]    
        public string Text { get; set; }

    }
}
