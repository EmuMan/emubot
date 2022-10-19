using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EmuBot.Models;
using EmuBot.Handlers;

namespace EmuBot
{

    public class EmuBot
    {

        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _services;

        private readonly DiscordSocketConfig _socketConfig = new()
        {
            GatewayIntents = GatewayIntents.Guilds |
                             GatewayIntents.GuildMembers |
                             GatewayIntents.GuildEmojis |
                             GatewayIntents.GuildMessages |
                             GatewayIntents.DirectMessages |
                             GatewayIntents.GuildMessageReactions,
            AlwaysDownloadUsers = true,
            MessageCacheSize = 1024,
        };

        public EmuBot()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("config.json", false, false)
                .Build();

            _services = new ServiceCollection()
                .AddSingleton(this)
                .AddSingleton(_configuration)
                .AddSingleton(_socketConfig)
                .AddSingleton(new Random((int)DateTime.UtcNow.Ticks))
                .AddSingleton<GuildTracker>()
                .AddSingleton<GuildInfo>()
                .AddSingleton<TrackedMessage>()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<InteractionService>()
                .AddSingleton<ReactionHandler>()
                .AddSingleton<InteractionHandler>()
                .BuildServiceProvider();
        }

        public async Task MainAsync()
        {
            var client = _services.GetRequiredService<DiscordSocketClient>();

            client.Log += Log;
            client.Ready += AsyncOnReady;

            await _services.GetRequiredService<ReactionHandler>().Initialize();
            await _services.GetRequiredService<InteractionHandler>().InitializeAsync();

            await client.LoginAsync(TokenType.Bot, _configuration["discord_token"]);
            await client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

        public Task Log(LogMessage msg)
        {
            if (msg.Exception is CommandException cmdException)
            {
                Console.WriteLine($"[Command/{msg.Severity}] {cmdException.Command.Aliases.First()}"
                                 + $" failed to execute in {cmdException.Context.Channel}.");
                Console.WriteLine(cmdException);
            }
            else
                Console.WriteLine($"[General/{msg.Severity}] {msg}");
            return Task.CompletedTask;
        }

        public Task Log(LogSeverity severity, string source, string message, Exception? exception = null)
        {
            return Log(new LogMessage(severity, source, message, exception));
        }

        private async Task AsyncOnReady()
        {
            var guildTracker = _services.GetRequiredService<GuildTracker>();
            guildTracker.LoadFromFile("data.json");
            await Log(new LogMessage(LogSeverity.Info, "OnReady", "EmuBot has been created"));
            // TODO: verify that this definitely works and properly broadcasts exceptions
            _ = guildTracker.StartSaveLoop()
                .ContinueWith(t => Log(new LogMessage(LogSeverity.Critical, "SaveLoop", "Save loop failed, aborting.", t.Exception)),
                              TaskContinuationOptions.OnlyOnFaulted);
        }

    }
}
