namespace LiveChatServer.Dtos
{
    public class ApiErrorDto
    {
        public string Code { get; set; } = "error";
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
    }
}
