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

namespace DiscordBot
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        private DiscordSocketClient _client;
        private ChatBot chatBot;
        private const string BotUsername = "Sorrien Bot";

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("commands.json", false, true)
             .AddJsonFile("authentication.json", false, false); //git ignore this file

            Configuration = builder.Build();

            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Info
            });
            _client.Log += Log;

            string token = Configuration["auth:token"];

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            chatBot = new ChatBot();

            _client.MessageReceived += MessageReceived;
            _client.GuildMemberUpdated += GuildMemberUpdated;

            _client.GuildAvailable += GuildAvailable;
            Console.ReadLine();

        }

        private async Task GuildMemberUpdated(SocketGuildUser oldInfo, SocketGuildUser newInfo)
        {
            await UpdateInGame(newInfo);
        }

        private async Task GuildAvailable(SocketGuild guild)
        {
            foreach (SocketGuildUser user in guild.Users)
            {
                await UpdateInGame(user);
            }
        }

        private async Task UpdateInGame(SocketGuildUser user)
        {
            bool inGame = user.Game.HasValue;
            IRole inGameRole = _client.GetGuild(user.Guild.Id).Roles.FirstOrDefault(x => x.Name == "In Game");
            if (inGame)
            {
                await user.AddRoleAsync(inGameRole);
            }
            else
            {
                await user.RemoveRoleAsync(inGameRole);
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage message)
        {
            string prefix = Configuration["options:prefix"];
            string content = SanitizeContent(message);
            if (content.Substring(0, 1) == prefix)
            {
                int commandIndex = 0;
                bool nullCommand = false;
                while (!nullCommand)
                {
                    string sharedKey = "commands:" + commandIndex + ":";
                    string commandName = Configuration[sharedKey + "Command"];
                    nullCommand = commandName == null;
                    if (nullCommand)
                    {
                        break;
                    }
                    else if (content.Substring(1, content.Length - 1) == commandName)
                    {
                        string response = Configuration[sharedKey + "Response"];
                        await message.Channel.SendMessageAsync(message.Author.Mention + " " + response);
                        break;
                    }
                    else
                    {
                        commandIndex++;
                    }
                }
            }
            else if (message.MentionedUsers.SingleOrDefault(x => x.Username == BotUsername) != null && message.Author.Username != BotUsername)
            {
                string response = chatBot.GetResponse(content, message.Author.Username + message.Author.Id);
                await message.Channel.SendMessageAsync(message.Author.Mention + " " + response);
            }
        }

        public string SanitizeContent(SocketMessage message)
        {
            string sanitized = message.Content;
            sanitized = Regex.Replace(sanitized, "<.*?>", string.Empty);
            return sanitized;
        }
    }
}

