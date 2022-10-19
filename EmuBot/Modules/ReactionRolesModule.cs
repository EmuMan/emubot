using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
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
        public async Task AddRR([Summary(description: "the channel the message is in")] ITextChannel channel,
                                [Summary(description: "message ID to track")] string messageId,
                                [Summary(description: "the emote/emoji to link to")] string emote,
                                [Summary(description: "the role to assign")] IRole role)
        {
            ulong msgId;
            if (!ulong.TryParse(messageId, out msgId))
            {
                await RespondAsync("Please pass a valid integer as the message ID.");
                return;
            }
            IMessage? msg = await channel.GetMessageAsync(msgId);
            if (msg is null)
            {
                await RespondAsync("Could not find that message in the specified channel!");
                return;
            }
            IEmote? e = GetEmote(emote.Trim());
            if (e is null)
            {
                await RespondAsync("Please pass a valid emote/emoji.");
                return;
            }
            var guilds = _services.GetRequiredService<GuildTracker>();
            var guild = guilds.LookupGuild(Context.Guild);
            var message = guild.GetMessage(msgId) ?? guild.TrackMessage(msgId);
            if (await message.AddRoleButton(msg, e, role))
            {
                await RespondAsync($"Reaction role for {e} successfully added!");
                return;
            }
            await RespondAsync("That reaction is already assigned. Please remove it before reassigning.");
        }

        [EnabledInDm(false)]
        [RequireOwner]
        [SlashCommand("remove-rr", "Removes a reaction role to a message")]
        public async Task RemoveRR([Summary(description: "the channel the message is in")] ITextChannel channel,
                                   [Summary(description: "tracked message ID")] string messageId,
                                   [Summary(description: "the emote/emoji to remove")] string emote)
        {
            ulong msgId;
            if (!ulong.TryParse(messageId, out msgId))
            {
                await RespondAsync("Please pass a valid integer as the message ID.");
                return;
            }
            IMessage? msg = await channel.GetMessageAsync(msgId);
            if (msg is null)
            {
                await RespondAsync("Could not find that message in the specified channel!");
                return;
            }
            IEmote? e = GetEmote(emote.Trim());
            if (e is null)
            {
                await RespondAsync("Please pass a valid emote/emoji.");
                return;
            }
            var guilds = _services.GetRequiredService<GuildTracker>();
            var guild = guilds.LookupGuild(Context.Guild);
            var message = guild.GetMessage(msgId) ?? guild.TrackMessage(msgId);
            if (await message.RemoveRoleButton(msg, e))
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
