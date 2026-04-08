namespace LiveChatServer.Services.Options
{
    public class ReplayOptions
    {
        // Maximum number of concurrent replay (GET /api/messages) requests allowed.
        public int ConcurrentReplayLimit { get; set; } = 5;
    }
}
