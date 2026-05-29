using Dexcom.Services;
using Dexcom.Templates;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;

namespace Dexcom.Controllers;

[ApiController]
[Route("api/generate")]
public class PdfController : ControllerBase
{
    [HttpGet("{name}")]
    public IActionResult Download(string name)
    {
        var document = new AgreementDocument(name);

        byte[] pdf = document.GeneratePdf();

        return File(
            pdf,
            "application/pdf",
            $"agreement-{name}.pdf");
    }
}