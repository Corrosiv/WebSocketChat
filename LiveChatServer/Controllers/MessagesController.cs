using Microsoft.AspNetCore.Mvc;
using LiveChatServer.Data;
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
        public async Task<IEnumerable<ChatMessage>> Get([FromQuery] int limit = 50)
        {
            return await _repo.GetRecentMessagesAsync(limit);
        }
    }
}
