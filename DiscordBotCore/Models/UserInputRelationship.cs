using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models
{
    public partial class UserInputRelationship
    {
        public Guid Id { get; set; }
        public Guid BotOutputId { get; set; }
        public Guid UserReplyId { get; set; }
        public int TimesReplied { get; set; }

        public virtual UserInput BotOutput { get; set; }
        public virtual UserInput UserReply { get; set; }
    }
}
