using ChatApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Models
{
    /// <summary>
    /// Запрос на аннулирование токена
    /// </summary>
    public class RevokeTokenRequest
    {
        public string Token { get; set; }
    }
}
