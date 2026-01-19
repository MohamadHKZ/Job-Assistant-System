using System;
using API.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace JobAssistantSystem.API.Controllers
{
    public class TestingController : BaseController
    {
        public TestingController()
        {

        }

        [HttpGet("bad-request")]
        public IActionResult GetBadRequest()
        {
            return BadRequest();
        }

        [HttpGet("unauthorized")]
        public IActionResult GetUnauthorized()
        {
            return Unauthorized();
        }

        [HttpGet("server-error")]
        public IActionResult GetServerError()
        {
            throw new Exception("This is a server error for testing purposes.");
        }

        [HttpGet("not-found")]
        public IActionResult GetNotFound()
        {
            return NotFound();
        }

    }
}