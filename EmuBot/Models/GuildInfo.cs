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

        public GuildInfo(IGuild guild, IServiceProvider services)
        {
            Guild = guild;
            Messages = new();
            _services = services;
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
            TrackedMessage tm = new(this, messageId, _services);
            Messages.Add(messageId, tm);
            return tm;
        }

    }
}
