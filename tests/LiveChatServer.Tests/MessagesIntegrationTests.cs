using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LiveChatServer.Tests
{
    public class MessagesIntegrationTests : IClassFixture<IsolatedChatAppFactory>
    {
        private readonly IsolatedChatAppFactory _factory;

        public MessagesIntegrationTests(IsolatedChatAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetMessages_WhenCalled_ReturnsSuccessStatus()
        {
            var client = _factory.CreateClient();

            var resp = await client.GetAsync("/api/messages?limit=10");
            resp.EnsureSuccessStatusCode();
        }
    }
}
