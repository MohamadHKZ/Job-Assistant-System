
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using UglyToad.PdfPig;


namespace API.Controllers
{
    public class TestController(ILogger<TestController> _logger) : BaseController
    {
        [HttpPost("extract-info")]
        [Consumes("application/pdf")]
        public async Task<ActionResult<MatchingObject>> ExtractInfoFromPdf()
        {
            if (Request.Body == null || !Request.Body.CanRead)
                return BadRequest("Invalid or empty request body.");

            try
            {
                await using var memoryStream = new MemoryStream();
                await Request.Body.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using var document = PdfDocument.Open(memoryStream);

                var sb = new StringBuilder();
                foreach (var page in document.GetPages())
                {
                    if (!string.IsNullOrWhiteSpace(page.Text))
                        sb.AppendLine(page.Text);
                }

                if (sb.Length == 0)
                    return UnprocessableEntity("No extractable text found.");
                var profile = new ProfileDTO
                {
                    ProfileId = 10245,
                    SeekedJobTitle = new List<string>
    {
        "Backend Developer",
        "Software Engineer"
    },
                    TechnicalSkills = new List<string>
    {
        "C#",
        ".NET Core",
        "Entity Framework",
        "SQL Server"
    },
                    FieldSkills = new List<string>
    {
        "REST API Development",
        "Microservices Architecture"
    },
                    SoftSkills = new List<string>
    {
        "Team Collaboration",
        "Problem Solving",
        "Time Management"
    },
                    JobPositionSkills = new List<string>
    {
        "System Design",
        "Code Review",
        "Performance Optimization"
    },
                    Experience = "5+ years of experience in backend development using .NET technologies.",
                    ReceiveNotifications = true
                };

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract PDF text from raw request body.");
                return UnprocessableEntity("Could not read/extract text from the provided PDF.");
            }
        }
    }
}