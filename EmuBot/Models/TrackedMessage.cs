using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace EmuBot.Models
{
    public class TrackedMessage
    {
        public ulong MessageID { get; init; }
        public Dictionary<string, ulong> RoleButtons { get; init; }

        private readonly IServiceProvider _services;

        public TrackedMessage(ulong messageID, Dictionary<string, ulong> roleButtons, IServiceProvider services)
        {
            MessageID = messageID;
            RoleButtons = roleButtons;
            _services = services;
        }

        public TrackedMessage(ulong messageId, IServiceProvider services)
            : this(messageId, new(), services)
        {
        }

        public async Task ProcessReaction(IEmote emote, IUser user)
        {
            if (user is not IGuildUser guildUser) return;
            if (!RoleButtons.ContainsKey(emote.Name)) return;
            await guildUser.AddRoleAsync(RoleButtons[emote.Name]);
        }
        
    }
}
