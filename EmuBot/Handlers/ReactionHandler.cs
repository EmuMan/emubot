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
    public abstract class ReactionHandler
    {
        protected readonly IServiceProvider _services;
        protected readonly DiscordSocketClient _client;

        public ReactionHandler(IServiceProvider services, DiscordSocketClient client)
        {
            _services = services;
            _client = client;
        }

        public abstract Task Initialize();

        protected async Task ProcessReactionRaw(
            Cacheable<IUserMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> channel,
            SocketReaction reaction,
            bool add)
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
            {
                if (add)
                    await tm.ProcessReactionAdd(reaction.Emote, reaction.User.Value);
                else
                    await tm.ProcessReactionRemove(reaction.Emote, reaction.User.Value);
            }
        }

    }
}
