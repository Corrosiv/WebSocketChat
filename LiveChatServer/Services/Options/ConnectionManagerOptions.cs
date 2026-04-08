namespace LiveChatServer.Services.Options
{
    public class ConnectionManagerOptions
    {
        // Number of consecutive send failures before a connection is considered dead.
        public int FailureThreshold { get; set; } = 3;
    }
}
