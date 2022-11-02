using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using EmuBot.Models;

namespace EmuBot.Serialization
{
    public class GuildTrackerSerializer
    {

        private IServiceProvider _services;
        private SqliteConnection _connection;

        public GuildTrackerSerializer(IServiceProvider services)
        {
            _services = services;
        }

        public void Initialize(string filename)
        {
            // insecure but all strings are statically supplied
            _connection = new SqliteConnection($"Data Source={filename}");
            _connection.Open();
        }

        public async Task<Dictionary<ulong, GuildInfo>> Load(GuildTracker tracker)
        {
            await CreateTablesIfNotExist();

            Dictionary<ulong, GuildInfo> guilds = new();

            using (var gCommand = _connection!.CreateCommand())
            {
                gCommand.CommandText = @"SELECT id FROM guilds";
                var guildIterator = await gCommand.ExecuteReaderAsync();

                while (await guildIterator.ReadAsync())
                {
                    var guildId = (ulong)guildIterator.GetInt64(0);

                    GuildInfo guild = new(tracker, guildId, _services);

                    using (var rbCommand = _connection!.CreateCommand())
                    {
                        rbCommand.CommandText = @"SELECT * FROM role_buttons WHERE guild = @guildId;";
                        rbCommand.Parameters.AddWithValue("@guildId", guildId);

                        var rbIterator = await rbCommand.ExecuteReaderAsync();

                        while (await rbIterator.ReadAsync())
                        {
                            ulong messageId = (ulong) rbIterator.GetInt64(1);
                            ulong roleId = (ulong) rbIterator.GetInt64(3);
                            string emoteName = rbIterator.GetString(4);
                            TrackedMessage tm = guild.GetOrTrackMessage(messageId);
                            tm.RegisterExistingRoleButton(emoteName, roleId);
                        }
                    }

                    guilds.Add(guild.GuildId, guild);
                }
            }

            return guilds;
        }

        public async Task<bool> IsGuildRegistered(GuildInfo guild)
        {
            using (var command = _connection!.CreateCommand())
            {
                command.CommandText = @"SELECT * FROM guilds WHERE id = @guildId";
                command.Parameters.AddWithValue("@guildId", guild.GuildId);
                var gIterator = await command.ExecuteReaderAsync();
                if (await gIterator.ReadAsync()) return true;
            }
            return false;
        }

        public async Task RegisterGuild(GuildInfo guild)
        {
            using (var command = _connection!.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO guilds(id)
                    VALUES(@guildId)";
                command.Parameters.AddRange(new SqliteParameter[] {
                    new SqliteParameter("@guildId", guild.GuildId),
                });
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task AddRoleButton(TrackedMessage message, string emoteName, ulong roleId)
        {
            using (var command = _connection!.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO role_buttons(message, guild, role, emote)
                    VALUES(@messageId, @guildId, @roleId, @emoteName)";
                command.Parameters.AddRange(new SqliteParameter[] {
                    new SqliteParameter("@messageId", message.MessageID),
                    new SqliteParameter("@guildId", message.Guild.GuildId),
                    new SqliteParameter("@roleId", roleId),
                    new SqliteParameter("@emoteName", emoteName),
                });
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteRoleButton(TrackedMessage message, string emoteName)
        {
            using (var command = _connection!.CreateCommand())
            {
                command.CommandText = @"DELETE FROM role_buttons WHERE message = @messageId AND emote = @emoteName";
                command.Parameters.AddRange(new SqliteParameter[] {
                    new SqliteParameter("@messageId", message.MessageID),
                    new SqliteParameter("@emoteName", emoteName),
                });
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAllRoleButtons(TrackedMessage message)
        {
            using (var command = _connection!.CreateCommand())
            {
                command.CommandText = @"DELETE FROM role_buttons WHERE message = @messageId";
                command.Parameters.AddRange(new SqliteParameter[] {
                    new SqliteParameter("@messageId", message.MessageID),
                });
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task CreateTablesIfNotExist()
        {
            using (var command = _connection!.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS role_buttons(
                        [id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        [message] INTEGER NOT NULL,
                        [guild] INTEGER NOT NULL,
                        [role] INTEGER NOT NULL,
                        [emote] TEXT NOT NULL
                    )";
                await command.ExecuteNonQueryAsync();
            }

            using (var command = _connection!.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS guilds(
                        [id] INTEGER NOT NULL PRIMARY KEY
                    )";
                await command.ExecuteNonQueryAsync();
            }
        }

    }
}
