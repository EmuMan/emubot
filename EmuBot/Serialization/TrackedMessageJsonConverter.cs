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
    public class TrackedMessageJsonConverter : JsonConverter<TrackedMessage>
    {

        private readonly IServiceProvider _services;

        public TrackedMessageJsonConverter(IServiceProvider services)
        {
            _services = services;
        }

        public override TrackedMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

            ulong messageId = 0;
            Dictionary<string, ulong> reactions = new();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;
                else if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();
                    switch (propertyName)
                    {
                        case "messageId":
                            messageId = reader.GetUInt64();
                            break;
                        case "reactions":
                            reactions = DeserializeReactions(ref reader, options);
                            break;
                    }
                }
            }

            return new(messageId, reactions, _services);
        }

        public override void Write(Utf8JsonWriter writer, TrackedMessage value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("messageId", value.MessageID);
            writer.WritePropertyName("reactions");
            SerializeReactions(writer, value.RoleButtons, options);
            writer.WriteEndObject();
        }

        private static Dictionary<string, ulong> DeserializeReactions(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            Dictionary<string, ulong> reactions = new();
            string? lastEmote = null;
            ulong? lastId = null;

            int currentArrayPlacement = 0;

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndArray:
                        if (lastEmote is not null && lastId is not null)
                        reactions.Add(lastEmote, (ulong)lastId);
                        currentArrayPlacement--;
                        break;
                    case JsonTokenType.StartArray:
                        lastEmote = null;
                        lastId = null;
                        currentArrayPlacement++;
                        break;
                    case JsonTokenType.Number:
                        lastId = reader.GetUInt64();
                        break;
                    case JsonTokenType.String:
                        lastEmote = reader.GetString();
                        break;
                }

                if (currentArrayPlacement == 0)
                    break;
            }

            return reactions;
        }

        private static void SerializeReactions(Utf8JsonWriter writer, Dictionary<string, ulong> reactions, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var entry in reactions)
            {
                writer.WriteStartArray();
                writer.WriteStringValue(entry.Key);
                writer.WriteNumberValue(entry.Value);
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
        }

    }
}
