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
        public IGuild Guild { get; init; }
        public Dictionary<ulong, TrackedMessage> Messages { get; init; }

        private readonly IServiceProvider _services;

        public GuildInfo(IGuild guild, Dictionary<ulong, TrackedMessage> messages, IServiceProvider services)
        {
            Guild = guild;
            Messages = messages;
            _services = services;
        }

        public GuildInfo(IGuild guild, IServiceProvider services)
            : this(guild, new(), services)
        {
        }

        public TrackedMessage? GetMessage(ulong messageId)
        {
            if (Messages.ContainsKey(messageId))
                return Messages[messageId];
            else
                return null;
        }

        public TrackedMessage TrackMessage(ulong messageId)
        {
            TrackedMessage tm = new(messageId, _services);
            Messages.Add(messageId, tm);
            return tm;
        }

    }
}
