using Microsoft.AspNetCore.Mvc;
using LiveChatServer.Data;
using LiveChatServer.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LiveChatServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageRepository _repo;

        public MessagesController(IMessageRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<IEnumerable<MessageDto>> Get([FromQuery] int limit = 50)
        {
            var msgs = await _repo.GetRecentMessagesAsync(limit);
            // Map domain ChatMessage to a client-friendly DTO
            var list = new List<MessageDto>();
            foreach (var m in msgs)
            {
                list.Add(new MessageDto
                {
                    Type = "message",
                    Timestamp = m.Timestamp,
                    Username = m.Username,
                    Content = m.Content,
                    IsTyping = false
                });
            }

            return list;
        }
    }
}
