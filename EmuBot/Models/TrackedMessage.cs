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
        public GuildInfo Guild { get; init; }
        public ulong MessageID { get; init; }
        public Dictionary<string, ulong> RoleButtons { get; init; }

        private readonly IServiceProvider _services;

        public TrackedMessage(GuildInfo guild, ulong messageId, Dictionary<string, ulong> roleButtons, IServiceProvider services)
        {
            Guild = guild;
            MessageID = messageId;
            RoleButtons = roleButtons;
            _services = services;
        }

        public TrackedMessage(GuildInfo guild, ulong messageId, IServiceProvider services)
            : this(guild, messageId, new(), services)
        {
        }

        public bool RegisterExistingRoleButton(string emoteName, ulong roleId)
        {
            if (!RoleButtons.ContainsKey(emoteName))
            {
                RoleButtons.Add(emoteName, roleId);
                return true;
            }
            return false;
        }

        public async Task<bool> AddRoleButton(IMessage message, IEmote emote, IRole role)
        {
            string emoteName = emote.ToString()!;
            if (!RoleButtons.ContainsKey(emoteName))
            {
                RoleButtons.Add(emoteName, role.Id);
                await message.AddReactionAsync(emote);
                await Guild.GuildTracker.SaveRoleButtonAdd(this, emoteName, role.Id);
                return true;
            }
            return false;
        }

        public async Task<bool> RemoveRoleButton(IMessage message, IEmote emote)
        {
            string emoteName = emote.ToString()!;
            if (RoleButtons.Remove(emoteName))
            {
                var user = _services.GetRequiredService<DiscordSocketClient>().CurrentUser;
                await message.RemoveReactionAsync(emote, user);
                await Guild.GuildTracker.SaveRoleButtonRemove(this, emoteName);
                return true;
            }
            return false;
        }
        
        public async Task RemoveAllRoleButtons(IMessage message)
        {
            IEmote? emote;
            var user = _services.GetRequiredService<DiscordSocketClient>().CurrentUser;
            foreach (var emoteName in RoleButtons.Keys)
            {
                emote = Utilities.GetEmote(emoteName);
                RoleButtons.Remove(emoteName);
                if (emote is not null)
                {
                    await message.RemoveReactionAsync(emote, user);
                }
            }
            await Guild.GuildTracker.DeleteAllRoleButtons(this);
        }

        public async Task ProcessReactionAdd(IEmote emote, IUser user)
        {
            string emoteName = emote.ToString()!;
            if (user is not IGuildUser guildUser) return;
            if (!RoleButtons.ContainsKey(emoteName)) return;
            await guildUser.AddRoleAsync(RoleButtons[emoteName]);
        }

        public async Task ProcessReactionRemove(IEmote emote, IUser user)
        {
            string emoteName = emote.ToString()!;
            if (user is not IGuildUser guildUser) return;
            if (!RoleButtons.ContainsKey(emoteName)) return;
            await guildUser.RemoveRoleAsync(RoleButtons[emoteName]);
        }
        
    }
}
