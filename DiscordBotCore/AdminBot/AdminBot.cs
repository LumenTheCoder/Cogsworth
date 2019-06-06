using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.Extensions.Configuration.Binder;
using System.IO;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Threading;
using Discord;

namespace DiscordBotCore.AdminBot
{
    public class AdminBot
    {
        public static IConfigurationRoot Configuration { get; set; }
        public string CommandPrefix
        {
            get
            {
                return Configuration["options:prefix"];
            }
        }
        public AdminBot()
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("commands.json", false, true)
             .AddJsonFile("Front-Desk.json", false, true)
             .AddJsonFile("Airlock.json", false, true)
             .AddJsonFile("Games.json", false, true)
             .AddJsonFile("Feeds.json", false, true)
             .AddJsonFile("Languages.json", false, true);

            Configuration = builder.Build();
        }

        public string GetRoleFromReaction(SocketReaction reaction, string SectionString)
        {
            var emojiRoles = new List<EmojiRoleModel>();
            Configuration.GetSection(SectionString).Bind(emojiRoles);

            string RoleName = null;
            EmojiRoleModel roleModel = emojiRoles.FirstOrDefault(x => x.EmojiId == reaction.Emote.Name);
            if(roleModel != null)
            {
                RoleName = roleModel.RoleId;
            }
            return RoleName;
        }

        public string RunCommand(string command, SocketMessage message)
        {
            string response = "";
            string commandWord = "";
            string[] commandArray = command.Split(' ');
            if (commandArray.Length > 0)
            {
                commandWord = commandArray[0].ToLower();
            }
            string commandParameters = command.Substring(commandWord.Length, command.Length - commandWord.Length);
            string authorMention = message.Author.Mention;
            switch (commandWord)
            {
                case "":
                    response = "I didn't hear a command in there.";
                    break;
                case "help":
                    var commandModels = new List<CommandModel>();
                    Configuration.GetSection("commands").Bind(commandModels);
                    response += "Commands: ";
                    foreach(var x in commandModels)
                    {
                        response += string.Format("\n{0}",x.Command);
                    }
                    break;
                default:
                    int commandIndex = 0;
                    bool nullCommand = false;
                    while (!nullCommand)
                    {
                        string sharedKey = "commands:" + commandIndex + ":";
                        string commandName = Configuration[sharedKey + "Command"];
                        nullCommand = commandName == null;
                        if (nullCommand)
                        {
                            response = authorMention + " I do not know this command.";
                            break;
                        }
                        else if (command == commandName)
                        {
                            response = authorMention + " " + Configuration[sharedKey + "Response"];

                            break;
                        }
                        else
                        {
                            commandIndex++;
                        }
                    }
                    break;
            }
            return response;
        }
    }
}
