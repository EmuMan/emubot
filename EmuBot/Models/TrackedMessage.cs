using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

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

        public async Task<bool> AddRoleButton(IMessage message, IEmote emote, IRole role)
        {
            if (!RoleButtons.ContainsKey(emote.Name))
            {
                RoleButtons.Add(emote.Name, role.Id);
                await message.AddReactionAsync(emote);
                return true;
            }
            return false;
        }

        public async Task<bool> RemoveRoleButton(IMessage message, IEmote emote)
        {
            if (RoleButtons.Remove(emote.Name))
            {
                var user = _services.GetRequiredService<DiscordSocketClient>().CurrentUser;
                await message.RemoveReactionAsync(emote, user);
                return true;
            }
            return false;
        }

        public async Task ProcessReactionAdd(IEmote emote, IUser user)
        {
            if (user is not IGuildUser guildUser) return;
            if (!RoleButtons.ContainsKey(emote.Name)) return;
            await guildUser.AddRoleAsync(RoleButtons[emote.Name]);
        }

        public async Task ProcessReactionRemove(IEmote emote, IUser user)
        {
            if (user is not IGuildUser guildUser) return;
            if (!RoleButtons.ContainsKey(emote.Name)) return;
            await guildUser.RemoveRoleAsync(RoleButtons[emote.Name]);
        }
        
    }
}
