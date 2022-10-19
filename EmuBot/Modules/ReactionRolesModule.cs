using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using EmuBot.Models;

namespace EmuBot.Modules
{
    public class ReactionRolesModule : InteractionModuleBase<SocketInteractionContext>
    {

        public InteractionService? Commands { get; set; }
        private readonly IServiceProvider _services;

        public ReactionRolesModule(IServiceProvider services)
        {
            _services = services;
            _services.GetRequiredService<EmuBot>().Log(
                LogSeverity.Debug, "Modules", "Creating ReactionRolesModule...");
        }

        [EnabledInDm(false)]
        [RequireOwner]
        [SlashCommand("add-rr", "Adds a reaction role to a message")]
        public async Task AddRR([Summary(description: "message ID to track")] string messageId,
                               [Summary(description: "the emote/emoji to link to")] string emote,
                               [Summary(description: "the role to assign")] IRole role)
        {
            ulong msgId;
            if (!ulong.TryParse(messageId, out msgId))
            {
                await RespondAsync("Please pass a valid integer as the message ID.");
                return;
            }
            IEmote? e = GetEmote(emote.Trim());
            if (e is null)
            {
                await RespondAsync("Please pass a valid emote/emoji.");
            }
            var guilds = _services.GetRequiredService<GuildTracker>();
            var guild = guilds.LookupGuild(Context.Guild);
            var message = guild.GetMessage(msgId) ?? guild.TrackMessage(msgId);
            if (message.RoleButtons.ContainsKey(e.Name))
            {
                await RespondAsync("That reaction is already assigned. Please remove it before reassigning.");
                return;
            }
            message.RoleButtons.Add(e.Name, role.Id);
            await RespondAsync($"Reaction role for {e} successfully added!");
        }

        [EnabledInDm(false)]
        [RequireOwner]
        [SlashCommand("remove-rr", "Removes a reaction role to a message")]
        public async Task RemoveRR([Summary(description: "tracked message ID")] string messageId,
                               [Summary(description: "the emote/emoji to remove")] string emote)
        {
            ulong msgId;
            if (!ulong.TryParse(messageId, out msgId))
            {
                await RespondAsync("Please pass a valid integer as the message ID.");
                return;
            }
            IEmote? e = GetEmote(emote.Trim());
            if (e is null)
            {
                await RespondAsync("Please pass a valid emote/emoji.");
            }
            var guilds = _services.GetRequiredService<GuildTracker>();
            var guild = guilds.LookupGuild(Context.Guild);
            var message = guild.GetMessage(msgId) ?? guild.TrackMessage(msgId);
            if (message.RoleButtons.Remove(e.Name))
            {
                await RespondAsync($"Reaction role for {e} successfully removed!");
                return;
            }
            await RespondAsync($"There is no reaction role for {e} on that message.");
        }

        private IEmote? GetEmote(string emoteName)
        {
            Emote emote;
            Emoji emoji;
            if (Emote.TryParse(emoteName, out emote))
                return emote;
            if (Emoji.TryParse(emoteName, out emoji))
                return emoji;
            return null;
        }

    }
}
