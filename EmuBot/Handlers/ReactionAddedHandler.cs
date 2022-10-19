using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using EmuBot.Models;

namespace EmuBot.Handlers
{
    public class ReactionAddedHandler : ReactionHandler
    {
        public ReactionAddedHandler(IServiceProvider services, DiscordSocketClient client) : base(services, client)
        {
        }

        public override Task Initialize()
        {
            _client.ReactionAdded += HandleReactionAsync;
            return Task.CompletedTask;
        }

        private async Task HandleReactionAsync(Cacheable<IUserMessage,ulong> message, Cacheable<IMessageChannel,ulong> channel, SocketReaction reaction)
        {
            await ProcessReactionRaw(message, channel, reaction, add: true);
        }

    }
}
