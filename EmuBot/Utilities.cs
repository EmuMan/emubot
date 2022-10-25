using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace EmuBot
{
    public class Utilities
    {

        public static IEmote? GetEmote(string emoteName)
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
