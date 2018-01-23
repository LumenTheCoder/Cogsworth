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
        private DiscordSocketClient _client;
        private AdminBot adminBot;
        private string BotUsername;
        private IRole inGameRole;
        private IRole streamingRole;
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
            await UpdateInGame(newInfo);
        }

        private Task GuildAvailable(SocketGuild guild)
        {
            CurrentGuild = guild;
            return Task.FromResult(0);
        }

        private async Task UpdateInGame(SocketGuildUser user)
        {
            bool inGame = user.Game.HasValue;

            if (inGameRole == null)
            {
                inGameRole = _client.GetGuild(user.Guild.Id).Roles.FirstOrDefault(x => x.Name == "In Game");
            }

            if (streamingRole == null)
            {
                streamingRole = _client.GetGuild(user.Guild.Id).Roles.FirstOrDefault(x => x.Name == "Streaming");
            }

            if (inGame)
            {
                if (user.Game.Value.StreamType != StreamType.NotStreaming)
                {
                    await user.AddRoleAsync(streamingRole);
                }
                else
                {
                    await user.RemoveRoleAsync(streamingRole);
                }
                await user.AddRoleAsync(inGameRole);
            }
            else
            {
                await user.RemoveRoleAsync(inGameRole);
                await user.RemoveRoleAsync(streamingRole);
            }
        }

        private async Task ReactionRoleAdd(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            string Section = null;

            switch (message.Id)
            {
                case 389157769397272577:
                case 389158652285812736:
                case 389158814739464204:
                    Section = "Careers";
                    break;

                case 405497289704996876:
                    Section = "Airlock";
                    break;

                case 402896764446834688:
                case 402896898505310218:
                    Section = "Games";
                    break;
            }

            if (Section != null)
            {
                var OrigMessage = await message.DownloadAsync();
                var GuildUser = CurrentGuild.GetUser(reaction.UserId);
                var role = CurrentGuild.Roles.FirstOrDefault(x => x.Name == adminBot.GetRoleFromReaction(reaction, Section));
                if (role != null)
                {
                    await GuildUser.AddRoleAsync(role);
                }
            }
        }

        private async Task ReactionRoleRemove(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            string Section = null;

            switch (message.Id)
            {
                case 389157769397272577:
                case 389158652285812736:
                case 389158814739464204:
                    Section = "Careers";
                    break;

                case 405497289704996876:
                    Section = "Airlock";
                    break;

                case 402896764446834688:
                case 402896898505310218:
                    Section = "Games";
                    break;
            }

            if (Section != null)
            {
                var OrigMessage = await message.DownloadAsync();
                var GuildUser = CurrentGuild.GetUser(reaction.UserId);
                var role = CurrentGuild.Roles.FirstOrDefault(x => x.Name == adminBot.GetRoleFromReaction(reaction, Section));
                if (role != null)
                {
                    await GuildUser.RemoveRoleAsync(role);
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
            //if (!sfw)
            //{
            //    await message.DeleteAsync();
            //}
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

