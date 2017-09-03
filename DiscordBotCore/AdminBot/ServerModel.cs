using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBotCore
{
    public class ServerModel
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public string Restart { get; set; }
    }
}
