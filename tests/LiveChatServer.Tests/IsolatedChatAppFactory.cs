using System;
using System.Linq;
using LiveChatServer.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace LiveChatServer.Tests
{
    /// <summary>
    /// Custom WebApplicationFactory that gives each test class its own in-memory
    /// SQLite database, preventing cross-test data contamination.
    /// </summary>
    public class IsolatedChatAppFactory : WebApplicationFactory<LiveChatServer.Program>
    {
        private readonly string _dbName = $"testdb_{Guid.NewGuid():N}";
        private SqliteConnection? _keepAlive;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var connStr = $"Data Source={_dbName};Mode=Memory;Cache=Shared";

            // Keep one connection open so the shared in-memory DB survives
            // across the short-lived connections the repository opens.
            _keepAlive = new SqliteConnection(connStr);
            _keepAlive.Open();

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IMessageRepository));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddSingleton<IMessageRepository>(
                    _ => new SqliteMessageRepository(connStr));
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _keepAlive?.Dispose();
            }
        }
    }
}
