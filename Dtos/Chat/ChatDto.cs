using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Chat
{
    public class ChatDto
    {
        public string UserId { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}