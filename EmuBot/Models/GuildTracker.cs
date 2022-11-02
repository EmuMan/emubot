using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using EmuBot.Serialization;

namespace EmuBot.Models
{
    public class GuildTracker
    {

        private readonly IServiceProvider _services;
        private readonly GuildTrackerSerializer _serializer;

        public Dictionary<ulong, GuildInfo> Guilds { get; private set; } = new();

        public GuildTracker(GuildTrackerSerializer serializer, IServiceProvider services)
        {
            _services = services;
            _serializer = serializer;
        }

        public GuildTracker(Dictionary<ulong, GuildInfo> guilds, GuildTrackerSerializer serializer, IServiceProvider services)
            : this(serializer, services)
        {
            Guilds = guilds;
        }

        public GuildInfo LookupGuild(IGuild guild)
        {
            if (!Guilds.ContainsKey(guild.Id))
                Guilds.Add(guild.Id, new(this, guild.Id, _services));
            return Guilds[guild.Id];
        }

        public async Task LoadFromSerializer()
        {
            Guilds = await _serializer.Load(this);
        }

        public async Task SaveRoleButtonAdd(TrackedMessage message, string emoteName, ulong roleId)
        {
            if (!await _serializer.IsGuildRegistered(message.Guild))
                await _serializer.RegisterGuild(message.Guild);
            await _serializer.AddRoleButton(message, emoteName, roleId);
        }

        public async Task SaveRoleButtonRemove(TrackedMessage message, string emoteName)
        {
            await _serializer.DeleteRoleButton(message, emoteName);
        }

        public async Task DeleteAllRoleButtons(TrackedMessage message)
        {
            await _serializer.DeleteAllRoleButtons(message);
        }

    }
}
