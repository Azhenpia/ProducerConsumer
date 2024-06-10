using Microsoft.AspNetCore.Mvc;
using ProducerConsumer.Services;

namespace ProducerConsumer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProducerController : ControllerBase
{
    private readonly ProducerService<string, string> _producerService;

    public ProducerController(ProducerService<string, string> producerService)
    {
        _producerService = producerService;
    }

    [HttpPost("push")]
    public async Task<IActionResult> PushMessage([FromBody] string message)
    {
        var response = await _producerService.PushMessageAsync(message);
        return Ok(new { Message = "Message pushed", Response = response });
    }
}


