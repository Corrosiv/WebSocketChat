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
        public async Task<PagedResponse<MessageDto>> Get([FromQuery] int limit = 50, [FromQuery] int offset = 0)
        {
            var msgs = await _repo.GetRecentMessagesAsync(limit, offset);
            var total = await _repo.GetTotalCountAsync();

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

            return new PagedResponse<MessageDto>
            {
                Items = list,
                Total = total,
                Limit = limit,
                Offset = offset
            };
        }
    }
}
