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
        public IMessage RoleMessage { get; set; }
        public List<ServerModel> Servers
        {
            get
            {
                List<ServerModel> value = new List<ServerModel>();
                Configuration.GetSection("Servers").Bind(value);
                return value;
            }
        }
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
             .AddJsonFile("servers.json", false, true)
             .AddJsonFile("EmojiRoles.json", false, true);

            Configuration = builder.Build();
        }

        public string GetRoleFromReaction(SocketReaction reaction)
        {
            var emojiRoles = new List<EmojiRoleModel>();
            Configuration.GetSection("EmojiRoles").Bind(emojiRoles);

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
            ServerModel server;
            string parameterError = "This command requires parameters.";
            string authorMention = message.Author.Mention;
            switch (commandWord)
            {
                case "":
                    response = "I didn't hear a command in there.";
                    break;
                case "status":
                    if (string.IsNullOrWhiteSpace(commandParameters))
                    {
                        response = parameterError;
                    }
                    else
                    {
                        server = GetServer(commandParameters);
                        if (server == null)
                        {
                            response = string.Format("I can't find \"{0}\"", commandParameters);
                        }
                        else
                        {
                            response = string.Format("{0} is {1}!", server.Name, IsServerOnline(server.Port) ? "Online" : "Offline");
                        }
                    }
                    break;
                case "restart":
                case "start":
                    if (string.IsNullOrWhiteSpace(commandParameters))
                    {
                        response = parameterError;
                    }
                    else
                    {
                        server = GetServer(commandParameters);
                        if (server == null)
                        {
                            response = string.Format("I can't find \"{0}\"", commandParameters);
                        }
                        else
                        {
                            if (StartServer(server))
                            {
                                //TODO: sleep for x seconds then check if server is online
                                response = string.Format("Starting up the {0} server!", server.Name);
                            }
                            else
                            {
                                response = string.Format("I'm afraid I can't do that, {0}. The {1} server is already online.", authorMention, server.Name);
                            }
                        }
                    }
                    break;
                case "servers":
                    response = "Server List: ";
                    foreach (var x in Servers)
                    {
                        response += string.Format("\n{0}: {1}\nAddress: {2}\nPort: {3}", x.Name, IsServerOnline(x.Port) ? "Online" : "Offline", x.Address, x.Port);
                    }
                    break;
                case "help":
                    var commandModels = new List<CommandModel>();
                    Configuration.GetSection("commands").Bind(commandModels);
                    response += "Commands: ";
                    foreach(var x in commandModels)
                    {
                        response += string.Format("\n{0}",x.Command);
                    }
                    response += "\nstatus (server name)\nrestart (server name)\nservers";
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


        private ServerModel GetServer(string serverName)
        {
            ServerModel server = null;
            string[] serverNameArray = serverName.Split(' ');
            foreach (string namePart in serverNameArray)
            {
                if (!string.IsNullOrWhiteSpace(namePart))
                {
                    server = Servers.FirstOrDefault(x => x.Name.ToLower().Contains(namePart.ToLower()));
                }
                if (server != null)
                {
                    break;
                }
            }
            return server;
        }

        private bool IsServerOnline(int port)
        {
            return IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners().Any(x => x.Port == port);
        }

        private bool StartServer(ServerModel server)
        {
            bool success = false;
            if (!IsServerOnline(server.Port))
            {
                RunBatch(server.Restart);
                success = true;
            }
            return success;
        }

        private void RunBatch(string filePath)
        {
            System.Diagnostics.Process.Start(filePath);
        }

        //private long IPStringToInt(string addr)
        //{
        //    // careful of sign extension: convert to uint first;
        //    // unsigned NetworkToHostOrder ought to be provided.
        //    IPAddress address = IPAddress.Parse(addr);
        //    byte[] bytes = address.GetAddressBytes();
        //    //Array.Reverse(bytes); // flip big-endian(network order) to little-endian
        //    uint intAddress = BitConverter.ToUInt32(bytes, 0);
        //    return (long)intAddress;
        //}
    }
}
