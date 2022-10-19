﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EmuBot.Handlers
{
    public class InteractionHandler
    {

        private readonly DiscordSocketClient _client;
        private readonly InteractionService _handler;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;

        public InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services, IConfiguration configuration)
        {
            _client = client;
            _handler = handler;
            _services = services;
            _configuration = configuration;
        }

        public async Task InitializeAsync()
        {
            await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _client.Ready += ReadyAsync;
            _client.InteractionCreated += HandleInteraction;
            _handler.Log += x =>
            {
                Console.WriteLine(x);
                return Task.CompletedTask;
            };
        }

        private async Task ReadyAsync()
        {
#if DEBUG
            await _handler.RegisterCommandsToGuildAsync(_configuration.GetValue<ulong>("guild"), true);
#else
            await _handler.RegisterCommandsGloballyAsync();
#endif
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_client, interaction);

                var result = await _handler.ExecuteCommandAsync(context, _services);

                if (!result.IsSuccess)
                    switch (result.Error)
                    {
                        case InteractionCommandError.UnmetPrecondition:
                            Console.WriteLine("Unmet Precondition");
                            break;
                        default:
                            break;
                    }
            }
            catch
            {
                // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if (interaction.Type is Discord.InteractionType.ApplicationCommand)
                    await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }

    }
}
