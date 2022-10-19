using EmuBot.Models;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace EmuBot.Serialization
{
    internal class GuildInfoJsonConverter : JsonConverter<GuildInfo>
    {

        private readonly IServiceProvider _services;

        public GuildInfoJsonConverter(IServiceProvider services)
        {
            _services = services;
        }

        public override GuildInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            /*
             * This block of code is incredibly cursed. Like, it's something that I would write in Java. Very bad.
             * Is there a way to do this in C# that isn't so convoluted? Probably. Do I know what that solution is?
             * No. So for now, this stays.
             * 
             * I know there are ways to let the JSON Serializer/Deserializer automatically fill in the properties of
             * classes. The big problem here is that the "_services" variable also needs to be inserted into the
             * constructor, which to my knowledge is not possible otherwise. I have seen solutions online that look
             * like they might be close, but to be honest, my knowledge of the JSON framework is incredibly sparse
             * at the moment. Definitely something to look into, but this will do for now.
             */

            var client = _services.GetRequiredService<DiscordSocketClient>();

            SocketGuild? guild = null;
            Dictionary<ulong, TrackedMessage>? messages = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    if (guild is not null && messages is not null)
                        return new(guild, messages, _services);
                    else
                        return null;
                }
                else if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();
                    switch (propertyName)
                    {
                        case "guildId":
                            guild = client.GetGuild(reader.GetUInt64());
                            break;
                        case "trackedMessages":
                            messages = DeserializeTrackedMessages(ref reader, options);
                            break;
                    }
                }
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, GuildInfo value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("guildId", value.Guild.Id);
            writer.WritePropertyName("trackedMessages");
            SerializeTrackedMessages(writer, value.Messages, options);
            writer.WriteEndObject();
        }

        private static Dictionary<ulong, TrackedMessage> DeserializeTrackedMessages(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            Dictionary<ulong, TrackedMessage> messages = new();
            var userInfoJsonConverter = (TrackedMessageJsonConverter)options.GetConverter(typeof(TrackedMessage));

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndArray:
                        return messages;
                    case JsonTokenType.StartObject:
                        var msg = userInfoJsonConverter.Read(ref reader, typeof(TrackedMessage), options)!;
                        messages.Add(msg.MessageID, msg);
                        break;
                }
            }

            return messages;
        }

        private static void SerializeTrackedMessages(Utf8JsonWriter writer, Dictionary<ulong, TrackedMessage> messages, JsonSerializerOptions options)
        {
            var trackedMessageJsonConverter = (TrackedMessageJsonConverter)options.GetConverter(typeof(TrackedMessage));

            writer.WriteStartArray();
            foreach (var msg in messages.Values)
            {
                trackedMessageJsonConverter.Write(writer, msg, options);
            }
            writer.WriteEndArray();
        }

    }
}
