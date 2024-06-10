using System;
using Microsoft.AspNetCore.Mvc;
using ProducerConsumer.Services;

namespace ProducerConsumer.Controllers;


[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ProducerService<string, string> _producerService;

    public TestController(ProducerService<string, string> producerService)
    {
        _producerService = producerService;
    }

    [HttpGet("testPush")]
    public async Task<IActionResult> TestPush(int taskCount)
    {
        var tasks = new Task[taskCount];

        for (int i = 0; i < tasks.Length; i++)
        {
            var message = $"Message {i + 1}";
            tasks[i] = Task.Run(async () =>
            {
                await _producerService.PushMessageAsync(message);
            });
        }

        await Task.WhenAll(tasks);

        return Ok(new { Message = "Tests Finished" } );
    }
}
