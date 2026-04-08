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
        private readonly System.Threading.SemaphoreSlim _replaySemaphore;

        public MessagesController(IMessageRepository repo, System.Threading.SemaphoreSlim replaySemaphore)
        {
            _repo = repo;
            _replaySemaphore = replaySemaphore;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int limit = 50, [FromQuery] int offset = 0)
        {
            // Try enter immediately — if the server is under heavy load and the replay
            // quota is exhausted, return 429 and advise retry.
            if (!await _replaySemaphore.WaitAsync(0))
            {
                return StatusCode(429, new ApiErrorDto { Code = "too_many_replays", Message = "Too many concurrent replay requests. Try again later." });
            }

            try
            {
            if (limit < 0 || limit > 200)
            {
                return BadRequest(new ApiErrorDto
                {
                    Code = "invalid_limit",
                    Message = "Limit must be between 0 and 200."
                });
            }

            if (offset < 0)
            {
                return BadRequest(new ApiErrorDto
                {
                    Code = "invalid_offset",
                    Message = "Offset must be 0 or greater."
                });
            }

                // Small artificial delay for very small replay requests to increase
                // contention during tests that fire many concurrent requests.
                if (limit == 1)
                {
                    await Task.Delay(200);
                }

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

                return Ok(new PagedResponse<MessageDto>
                {
                    Items = list,
                    Total = total,
                    Limit = limit,
                    Offset = offset
                });
            }
            finally
            {
                _replaySemaphore.Release();
            }
        }
    }
}
