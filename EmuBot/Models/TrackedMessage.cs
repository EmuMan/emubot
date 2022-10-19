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
        public GuildInfo GuildInfo { get; init; }
        public ulong MessageID { get; init; }
        public Dictionary<string, IRole> RoleButtons { get; init; }

        private readonly IServiceProvider _services;

        public TrackedMessage(GuildInfo guildInfo, ulong messageId, IServiceProvider services)
        {
            this.GuildInfo = guildInfo;
            this.RoleButtons = new();
            this.MessageID = messageId;
            this._services = services;
        }

        public async Task ProcessReaction(IEmote emote, IUser user)
        {
            if (user is not IGuildUser guildUser) return;
            if (!RoleButtons.ContainsKey(emote.Name)) return;
            await guildUser.AddRoleAsync(RoleButtons[emote.Name]);
        }
        
    }
}
