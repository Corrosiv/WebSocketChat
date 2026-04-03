using System.Collections.Generic;

namespace LiveChatServer.Dtos
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int Total { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
    }
}
