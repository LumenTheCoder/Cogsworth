using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using Newtonsoft.Json;
using DiscordBotCore;
using System.Net.NetworkInformation;
using System.Net;
using DiscordBotCore.AdminBot;

namespace DiscordBot
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        private readonly ulong[] GAMING_MESSAGES = { 529884359386202120, 529881177939509249, 402896764446834688 };
        private readonly ulong[] FRONT_DESK1_MESSAGES = { 557428958623301632 };
        private readonly ulong[] FRONT_DESK2_MESSAGES = { 557417887661424670, 557417910855925760, 557417932716638224 };
        private readonly ulong[] LANGUAGES_MESSAGES = { 554851978967121922, 554855130600701953, 554856439391322123, 554856499994951709 };
        private readonly ulong AIRLOCK_MESSAGE = 405497289704996876;
        private readonly ulong FEEDS_MESSAGE = 554840498175606784;

        private DiscordSocketClient _client;
        private AdminBot adminBot;

        private string BotUsername;

        private IRole inGameRole;
        private IRole streamingRole;
        private IRole starCitizenRole;

        private SocketGuild CurrentGuild;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("authentication.json", false, false);

            Configuration = builder.Build();

            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Info,
                DefaultRetryMode = RetryMode.AlwaysRetry,
            });
            _client.Log += Log;

            string token = Configuration["auth:token"];
            BotUsername = Configuration["auth:BotUsername"];

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            adminBot = new AdminBot();
            _client.MessageReceived += MessageReceived;
            _client.GuildMemberUpdated += GuildMemberUpdated;
            _client.ReactionAdded += ReactionRoleAdd;
            _client.ReactionRemoved += ReactionRoleRemove;

            _client.GuildAvailable += GuildAvailable;

            await Task.Delay(-1);
        }

        private async Task GuildMemberUpdated(SocketGuildUser oldInfo, SocketGuildUser newInfo)
        {
            await Task.Run(() => { UpdateInGame(newInfo); });
        }

        private Task GuildAvailable(SocketGuild guild)
        {
            CurrentGuild = guild;
            return Task.FromResult(0);
        }

        private void UpdateInGame(SocketGuildUser user)
        {
            IActivity activity = user.Activity;

            if (activity != null)
            {
                if (inGameRole == null)
                {
                    inGameRole = CurrentGuild.Roles.FirstOrDefault(x => x.Name == "In Game");
                }

                if (streamingRole == null)
                {
                    streamingRole = CurrentGuild.Roles.FirstOrDefault(x => x.Name == "Streaming");
                }

                if (starCitizenRole == null)
                {
                    starCitizenRole = CurrentGuild.Roles.FirstOrDefault(x => x.Name == "In the Verse");
                }

                if (user.Activity.Type == ActivityType.Streaming)
                {
                    user.AddRoleAsync(streamingRole);
                }
                else
                {
                    user.RemoveRoleAsync(streamingRole);
                }

                if (user.Activity.Type == ActivityType.Playing)
                {
                    user.AddRoleAsync(inGameRole);

                    if (user.Activity.Name == "Star Citizen")
                    {
                        user.AddRoleAsync(starCitizenRole);
                    }
                    else
                    {
                        user.RemoveRoleAsync(starCitizenRole);
                    }
                }
                else
                {
                    user.RemoveRoleAsync(inGameRole);
                    user.RemoveRoleAsync(starCitizenRole);
                }
            }
            else
            {
                user.RemoveRoleAsync(inGameRole);
                user.RemoveRoleAsync(starCitizenRole);
            }
        }

        private async Task ReactionRoleAdd(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            string Section = null;

            if (GAMING_MESSAGES.Contains(message.Id))
            {
                Section = "Games";
            }

            if (FRONT_DESK1_MESSAGES.Contains(message.Id))
            {
                Section = "Front Desk 1";
            }

            if (FRONT_DESK2_MESSAGES.Contains(message.Id))
            {
                Section = "Front-Desk2";
            }

            if (AIRLOCK_MESSAGE == message.Id)
            {
                Section = "Airlock";
            }

            if (FEEDS_MESSAGE == message.Id)
            {
                Section = "list-of-feeds";
            }

            if (LANGUAGES_MESSAGES.Contains(message.Id))
            {
                Section = "Languages";
            }

            if (Section != null)
            {
                var OrigMessage = await message.DownloadAsync();
                var GuildUser = CurrentGuild.GetUser(reaction.UserId);
                var role = CurrentGuild.Roles.FirstOrDefault(x => x.Name == adminBot.GetRoleFromReaction(reaction, Section));
                if (role != null)
                {
                    await Task.Run(() => { GuildUser.AddRoleAsync(role); });
                }
            }
        }

        private async Task ReactionRoleRemove(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            string Section = null;

            if (GAMING_MESSAGES.Contains(message.Id))
            {
                Section = "Games";
            }

            if (FRONT_DESK1_MESSAGES.Contains(message.Id))
            {
                Section = "Front Desk 1";
            }

            if (FRONT_DESK2_MESSAGES.Contains(message.Id))
            {
                Section = "Front-Desk2";
            }

            if (AIRLOCK_MESSAGE == message.Id)
            {
                Section = "Airlock";
            }

            if (FEEDS_MESSAGE == message.Id)
            {
                Section = "list-of-feeds";
            }

            if (LANGUAGES_MESSAGES.Contains(message.Id))
            {
                Section = "Languages";
            }

            if (Section != null)
            {
                var OrigMessage = await message.DownloadAsync();
                var GuildUser = CurrentGuild.GetUser(reaction.UserId);
                var role = CurrentGuild.Roles.FirstOrDefault(x => x.Name == adminBot.GetRoleFromReaction(reaction, Section));
                if (role != null)
                {
                    await Task.Run(() => { GuildUser.RemoveRoleAsync(role); });
                }
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage message)
        {
            string content = SanitizeContent(message.Content);
            string response = "";
            
            if (content.Substring(0, 1) == adminBot.CommandPrefix)
            {
                string command = content.Substring(1, content.Length - 1);
                response = adminBot.RunCommand(command, message);
            }

            if (!string.IsNullOrEmpty(response))
            {
                await message.Channel.SendMessageAsync(response);
            }
        }

        private string SanitizeContent(string message)
        {
            string sanitized = message;
            sanitized = Regex.Replace(sanitized, "<.*?>", string.Empty);
            if (sanitized.Substring(0, 1) == " ")
            {
                sanitized = sanitized.Substring(1, sanitized.Length - 1);
            }
            return sanitized;
        }
    }
}

