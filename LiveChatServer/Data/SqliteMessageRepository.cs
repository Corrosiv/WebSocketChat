using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace LiveChatServer.Data
{
    // Minimal SQLite-based repository implementation. Not optimized — suitable for prototype.
    public class SqliteMessageRepository : IMessageRepository, IDisposable
    {
        private readonly string _connectionString;

        public SqliteMessageRepository(string connectionString)
        {
            _connectionString = connectionString;
            EnsureDatabase();
        }

        private void EnsureDatabase()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Messages (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                Content TEXT NOT NULL,
                Timestamp TEXT NOT NULL
            );";
            cmd.ExecuteNonQuery();
        }

        public async Task AddMessageAsync(ChatMessage message)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Messages (Username, Content, Timestamp) VALUES ($u, $c, $t);";
            cmd.Parameters.AddWithValue("$u", message.Username ?? string.Empty);
            cmd.Parameters.AddWithValue("$c", message.Content ?? string.Empty);
            cmd.Parameters.AddWithValue("$t", message.Timestamp.ToString("o"));
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int limit)
        {
            var list = new List<ChatMessage>();
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Username, Content, Timestamp FROM Messages ORDER BY Id DESC LIMIT $l;";
            cmd.Parameters.AddWithValue("$l", limit);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var msg = new ChatMessage
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Content = reader.GetString(2),
                    Timestamp = DateTime.Parse(reader.GetString(3))
                };
                list.Add(msg);
            }
            // Return in chronological order (oldest first)
            list.Reverse();
            return list;
        }

        public void Dispose()
        {
            // Nothing to dispose for now. Placeholder for future resources.
        }
    }
}
