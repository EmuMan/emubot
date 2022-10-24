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
    public class BirthdayModule : InteractionModuleBase<SocketInteractionContext>
    {

        public enum Month
        {
            JANUARY,
            FEBRUARY,
            MARCH,
            APRIL,
            MAY,
            JUNE,
            JULY,
            AUGUST,
            SEPTEMBER,
            OCTOBER,
            NOVEMBER,
            DECEMBER,
        }

        public InteractionService? Commands { get; set; }
        private readonly IServiceProvider _services;

        public BirthdayModule(IServiceProvider services)
        {
            _services = services;
            _services.GetRequiredService<EmuBot>().Log(
                LogSeverity.Debug, "Modules", "Creating BirthdayModule...");
        }

        [EnabledInDm(true)]
        [SlashCommand("birthday", "Tells EmuBot your birthday!")]
        public async Task SetBirthday([Summary(description: "birthday month")] Month month,
                                [Summary(description: "day of the month")]
                                [MaxValue(31)]
                                [MinValue(1)] int day)
        {

        }

    }
}
