using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChatApp.Entities
{
    public class User
    {
        [Required, MinLength(1), MaxLength(50), Key]
        public string Username { get; set; }
        [JsonIgnore]
        [Required, MinLength(3)]
        public string Password { get; set; }
        [JsonIgnore]
        public List<RefreshToken> RefreshTokens { get; set; }
        public List<UserDialog> UserDialog { get; set; }

        public User()
        {
            RefreshTokens = new List<RefreshToken>();
            UserDialog = new List<UserDialog>();
        }

    }
}
