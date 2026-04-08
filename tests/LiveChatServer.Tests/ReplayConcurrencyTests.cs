using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LiveChatServer.Tests
{
    public class ReplayConcurrencyTests : IClassFixture<IsolatedChatAppFactory>
    {
        private readonly IsolatedChatAppFactory _factory;

        public ReplayConcurrencyTests(IsolatedChatAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task TooManyConcurrentReplays_Returns429()
        {
            var client = _factory.CreateClient();

            // Fire off more concurrent replay requests than the configured limit (5)
            var tasks = new Task<HttpResponseMessage>[8];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = client.GetAsync("/api/messages?limit=1");
            }

            // Await each task to avoid blocking on Task.Result (xUnit analyzer warning)
            await Task.WhenAll(tasks);

            var tooMany = 0;
            foreach (var t in tasks)
            {
                var res = await t;
                if (res.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    tooMany++;
            }

            // Expect at least some requests to be rejected with 429 when limit is exceeded
            Assert.True(tooMany >= 1, "Expected at least one request to receive 429 Too Many Requests when concurrent replay limit is exceeded");
        }
    }
}
