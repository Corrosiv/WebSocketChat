using System.Collections.Generic;
using System.Threading.Tasks;

namespace LiveChatServer.Data
{
    public interface IMessageRepository
    {
        Task AddMessageAsync(ChatMessage message);
        Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int limit);
        Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int limit, int offset);
        Task<int> GetTotalCountAsync();
    }
}
