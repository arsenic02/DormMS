using DormMS.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DormMS.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatbotController : ControllerBase
{
    private readonly IKnowledgeBaseService _knowledgeBaseService;

    public ChatbotController(IKnowledgeBaseService knowledgeBaseService)
    {
        _knowledgeBaseService = knowledgeBaseService;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] AskChatbotRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest("Pitanje ne sme biti prazno.");

        // var result = await _knowledgeBaseService.AskAsync(request.Question);
        //  if (result is null)
        //     return Ok(new { answer = "Nemam odgovor na to pitanje. Obratite se referentu." });
        // return Ok(result);
        var results = await _knowledgeBaseService.AskAsync(request.Question,2);
        if (results.Count == 0)
            return Ok(new { message = "Nemam odgovor na to pitanje. Obratite se referentu." });

        return Ok(results);
    }
}

public class AskChatbotRequest
{
    public string Question { get; set; } = null!;
}