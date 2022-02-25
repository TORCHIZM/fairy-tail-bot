using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FairyTail_Bot.Modules;

namespace FairyTail_Bot
{
    class Program
    {
        DiscordSocketClient _client;
        CommandHandler _handler;

        static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                MessageCacheSize = 400,
                LogLevel = LogSeverity.Verbose
            });

            _client.Log += Log;
            //_client.MessageReceived += PUG.MessageReceived;
            _client.ReactionAdded += PUG.ReactionAdded;
            _client.UserVoiceStateUpdated += PUG.VoiceStateUpdated;

            await _client.SetGameAsync("Fairy Tail", "https://twitch.tv/torchizm/", ActivityType.Streaming);
            await _client.SetStatusAsync(UserStatus.Online);

            await _client.LoginAsync(TokenType.Bot, "your bot token here");

            await _client.StartAsync();

            Global.Client = _client;
            _handler = new CommandHandler();
            await _handler.InitializeAsync(_client);

            await ConsoleInput();
            await Task.Delay(-1);
        }

        private async Task ConsoleInput()
        {
            var input = string.Empty;
            while (input.Trim().ToLower() != "block")
            {
                input = Console.ReadLine();
                if (input.Trim().ToLower() == "message")
                    ConsoleSendMessage();
            }
        }

        private async void ConsoleSendMessage()
        {
            Console.WriteLine("Select the guild:");
            var guild = GetSelectedGuild(_client.Guilds);
            var textChannel = GetSelectedTextChannel(guild.TextChannels);
            var msg = string.Empty;
            while (msg.Trim() == string.Empty)
            {
                Console.WriteLine("Your message:");
                msg = Console.ReadLine();
            }

            await textChannel.SendMessageAsync(msg);
        }

        private SocketTextChannel GetSelectedTextChannel(IEnumerable<SocketTextChannel> channels)
        {
            var textChannels = channels.ToList();
            var maxIndex = textChannels.Count - 1;
            for (var i = 0; i <= maxIndex; i++)
                Console.WriteLine($"{i} - {textChannels[i].Name}");

            var selectedIndex = -1;
            while (selectedIndex < 0 || selectedIndex > maxIndex)
            {
                var success = int.TryParse(Console.ReadLine().Trim(), out selectedIndex);
                if (!success)
                {
                    Console.WriteLine("That was an invalid index, try again.");
                    selectedIndex = -1;
                }
            }

            return textChannels[selectedIndex];
        }

        private SocketGuild GetSelectedGuild(IEnumerable<SocketGuild> guilds)
        {
            var socketGuilds = guilds.ToList();
            var maxIndex = socketGuilds.Count - 1;
            for (var i = 0; i <= maxIndex; i++)
            {
                Console.WriteLine($"{i} - {socketGuilds[i].Name}");
            }

            var selectedIndex = -1;
            while (selectedIndex < 0 || selectedIndex > maxIndex)
            {
                var success = int.TryParse(Console.ReadLine().Trim(), out selectedIndex);
                if (!success)
                {
                    Console.WriteLine("That was an invalid index, try again.");
                    selectedIndex = -1;
                }
            }

            return socketGuilds[selectedIndex];
        }

        private async Task Log(LogMessage msg) => Console.WriteLine($"{msg.Severity} {msg.Message}");
    }
}