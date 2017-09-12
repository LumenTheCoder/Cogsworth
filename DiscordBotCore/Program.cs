using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBot.Models;
using Microsoft.Extensions.Configuration;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using DiscordBotCore.ChatBot;
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
        private ChatBot chatBot;
        private AdminBot adminBot;
        private string BotUsername;
        private IRole inGameRole;
        private IRole streamingRole;
        private bool FirstConnect;
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("authentication.json", false, false)
             .AddJsonFile("profanity.json", false, true);

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

            chatBot = new ChatBot();
            adminBot = new AdminBot();
            _client.MessageReceived += MessageReceived;
            _client.GuildMemberUpdated += GuildMemberUpdated;

            FirstConnect = true;
            _client.GuildAvailable += GuildAvailable;
            
            await Task.Delay(-1);
        }

        private async Task GuildMemberUpdated(SocketGuildUser oldInfo, SocketGuildUser newInfo)
        {
            await UpdateInGame(newInfo);
        }

        private Task GuildAvailable(SocketGuild guild)
        {
            if (FirstConnect)
            {
                FirstConnect = false;
                foreach (SocketGuildUser user in guild.Users)
                {
                    Task.Run(async () => await UpdateInGame(user));
                }
            }

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

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage message)
        {
            string content = SanitizeContent(message.Content);
            bool sfw = IsSafeForWork(content);
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
            else if (message.MentionedUsers.SingleOrDefault(x => x.Username == BotUsername) != null && message.Author.Username != BotUsername)
            {
                if (sfw)
                {
                    response = await chatBot.GetResponse(content, message.Author.Username + message.Author.Id);
                }
                else
                {
                    response = "I don't feel comfortable talking about that.";
                }
                response = message.Author.Mention + " " + response;
            }
            if (!string.IsNullOrEmpty(response))
            {
                await message.Channel.SendMessageAsync(response);
            }
        }

        private bool IsSafeForWork(string content)
        {
            bool safe = true;

            IConfigurationSection profanitySection = Configuration.GetSection("Profanity");
            var profanityList = profanitySection.AsEnumerable();
            foreach (var bad in profanityList)
            {
                if (bad.Value != null && content.Contains(bad.Value))
                {
                    safe = false;
                    break;
                }
            }

            return safe;
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

