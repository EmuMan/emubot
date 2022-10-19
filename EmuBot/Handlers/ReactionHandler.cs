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
    public class ReactionHandler
    {

        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _client;

        public ReactionHandler(IServiceProvider services, DiscordSocketClient client)
        {
            _services = services;
            _client = client;
        }

        public Task Initialize()
        {
            _client.ReactionAdded += HandleReactionAsync;
            return Task.CompletedTask;
        }

        private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            if (!reaction.User.IsSpecified) return;
            if (reaction.User.Value.IsBot) return;
            IUserMessage reactionMsg = message.GetOrDownloadAsync().Result;
            IMessageChannel reactionChannel = channel.GetOrDownloadAsync().Result;
            if (reactionChannel is not IGuildChannel guildChannel) return;

            GuildTracker gt = _services.GetRequiredService<GuildTracker>();
            GuildInfo gi = gt.LookupGuild(guildChannel.Guild);
            TrackedMessage? tm = gi.GetMessage(reactionMsg.Id);
            if (tm is not null)
                await tm.ProcessReaction(reaction.Emote, reaction.User.Value);

            return;
        }

    }
}
