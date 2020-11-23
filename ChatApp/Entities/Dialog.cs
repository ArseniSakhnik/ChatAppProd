using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChatApp.Entities
{
    public class Dialog
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Message> Messages { get; set; }
        [JsonIgnore]
        public List<UserDialog> UserDialog { get; set; }

        public Dialog()
        {
            Messages = new List<Message>();
            UserDialog = new List<UserDialog>();
        }

    }
}
