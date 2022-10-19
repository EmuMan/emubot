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
    public class ReactionRemovedHandler : ReactionHandler
    {
        public ReactionRemovedHandler(IServiceProvider services, DiscordSocketClient client) : base(services, client)
        {
        }

        public override Task Initialize()
        {
            _client.ReactionRemoved += HandleReactionAsync;
            return Task.CompletedTask;
        }

        private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            await ProcessReactionRaw(message, channel, reaction, add: false);
        }
    }
}
