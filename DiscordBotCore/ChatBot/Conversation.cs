using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBotCore.ChatBot
{
    class Conversation
    {
        public string LastBotResponse;
        public string UserId;

        public Conversation(string botResponse, string userId)
        {
            LastBotResponse = botResponse;
            UserId = userId;
        }
    }
}
