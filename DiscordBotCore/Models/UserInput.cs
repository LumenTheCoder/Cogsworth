using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models
{
    public partial class UserInput
    {
        public UserInput()
        {
            UserInputRelationshipBotOutput = new HashSet<UserInputRelationship>();
            UserInputRelationshipUserReply = new HashSet<UserInputRelationship>();
        }

        public Guid Id { get; set; }
        public String Input { get; set; }

        public virtual ICollection<UserInputRelationship> UserInputRelationshipBotOutput { get; set; }
        public virtual ICollection<UserInputRelationship> UserInputRelationshipUserReply { get; set; }
    }
}
