using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Models
{
    /// <summary>
    /// Класс запроса на проверку пользователя на существование
    /// </summary>
    public class UserExitstsRequest
    {
        [Required]
        public string Username { get; set; }
    }
}
