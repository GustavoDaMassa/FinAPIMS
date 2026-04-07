using FinanceApi.Finance.Application.Exceptions;
using FinanceApi.Finance.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApi.Finance.Api.Controllers;

[ApiController]
[Route("import")]
public class ImportController(IOfxImportService importService) : ControllerBase
{
    [HttpPost("ofx")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportOfx(IFormFile file, [FromQuery] Guid accountId)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file provided.");

        try
        {
            using var stream = file.OpenReadStream();
            var result = await importService.ImportAsync(stream, accountId);
            return Ok(result);
        }
        catch (AccountNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
