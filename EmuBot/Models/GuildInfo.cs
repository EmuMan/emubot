using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace EmuBot.Models
{
    public class GuildInfo
    {
        public GuildTracker GuildTracker { get; init; }
        public ulong GuildId { get; init; }
        public Dictionary<ulong, TrackedMessage> Messages { get; init; }

        private readonly IServiceProvider _services;

        public GuildInfo(GuildTracker guildTracker, ulong guildId, Dictionary<ulong, TrackedMessage> messages, IServiceProvider services)
        {
            GuildTracker = guildTracker;
            GuildId = guildId;
            Messages = messages;
            _services = services;
        }

        public GuildInfo(GuildTracker guildTracker, ulong guildId, IServiceProvider services)
            : this(guildTracker, guildId, new(), services)
        {
        }

        public TrackedMessage? GetMessage(ulong messageId)
        {
            if (Messages.ContainsKey(messageId))
                return Messages[messageId];
            else
                return null;
        }

        public TrackedMessage GetOrTrackMessage(ulong messageId)
        {
            if (Messages.ContainsKey(messageId))
                return Messages[messageId];
            else
                return TrackMessage(messageId);
        }

        public TrackedMessage TrackMessage(ulong messageId)
        {
            TrackedMessage tm = new(this, messageId, _services);
            Messages.Add(messageId, tm);
            return tm;
        }

        public void RegisterExistingTrackedMessage(TrackedMessage tm)
        {
            Messages.Add(tm.MessageID, tm);
        }

    }
}
