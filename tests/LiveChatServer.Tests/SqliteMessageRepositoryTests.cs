using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiveChatServer.Data;
using Xunit;

namespace LiveChatServer.Tests
{
    public class SqliteMessageRepositoryTests : IDisposable
    {
        private readonly string _dbPath;
        private readonly SqliteMessageRepository _repo;

        public SqliteMessageRepositoryTests()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
            _repo = new SqliteMessageRepository($"Data Source={_dbPath}");
        }

        [Fact]
        public async Task AddMessages_WhenRequested_ReturnsMessagesInChronologicalOrder()
        {
            var msg1 = new ChatMessage { Username = "alice", Content = "hello", Timestamp = DateTime.UtcNow };
            var msg2 = new ChatMessage { Username = "bob", Content = "hi", Timestamp = DateTime.UtcNow };

            await _repo.AddMessageAsync(msg1);
            await _repo.AddMessageAsync(msg2);

            var list = (await _repo.GetRecentMessagesAsync(10)).ToList();

            Assert.Equal(2, list.Count);
            Assert.Equal("alice", list[0].Username);
            Assert.Equal("hello", list[0].Content);
            Assert.Equal("bob", list[1].Username);
        }

        public void Dispose()
        {
            try { File.Delete(_dbPath); } catch { }
        }
    }
}
