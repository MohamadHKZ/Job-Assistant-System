using API.Controllers;
using API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Job_Assistant_System.API.Controllers
{
    [AllowAnonymous]
    public class TrendsController(ITrendsService _trendsService) : BaseController
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrendDTO>>> GetTrends()
        {
            var trends = await _trendsService.GetTrendsAsync();
            return Ok(trends);
        }
    }
}
