﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Models
{
    /// <summary>
    /// Запрос на отправку сообщения
    /// </summary>
    public class SendMessageRequest
    {
        public int RecipientId { get; set; }
        public string Text { get; set; }
    }
}
