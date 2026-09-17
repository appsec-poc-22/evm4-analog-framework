using DemoFramework;
using Microsoft.AspNetCore.Mvc;

namespace DemoService.Controllers;

/// <summary>
/// Stands in for an EVM4 service controller. Note that it has no direct
/// reference to Newtonsoft.Json: it reaches the vulnerable code only through
/// DemoFramework, exactly as our services reach the framework's 75 packages.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EvidenceController : ControllerBase
{
    /// <summary>
    /// TAINT SOURCE -> framework -> vulnerable sink.
    /// Untrusted request body flows into DataProcessor.Parse, which calls
    /// JsonConvert.DeserializeObject with TypeNameHandling.All.
    /// </summary>
    [HttpPost("import")]
    public IActionResult Import([FromBody] string payload)
    {
        var result = DataProcessor.Parse<Dictionary<string, object>>(payload);
        return Ok(result);
    }

    [HttpGet("export/{id}")]
    public IActionResult Export(string id)
    {
        return Content(DataProcessor.Serialize(new { id, retrieved = DateTime.UtcNow }));
    }
}
