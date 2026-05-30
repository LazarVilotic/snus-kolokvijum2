using Microsoft.AspNetCore.Mvc;
using CupidServer.Models;
using CupidServer.Services;

namespace CupidServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CupidController : ControllerBase
    {
        private readonly CupidService _service;

        public CupidController(CupidService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        public IActionResult Register(Person person)
        {
            _service.RegisterPerson(person);
            return Ok("User registered.");
        }

        [HttpGet("all")]
        public IActionResult GetAll() => Ok(_service.GetAllPersons());

        [HttpPost("confirm")]
        public IActionResult Confirm(string username)
        {
            _service.ConfirmLetter(username);
            return Ok("Confirmed.");
        }

        [HttpPost("block")]
        public IActionResult Block([FromQuery] string username, [FromQuery] string blockUser)
        {
            _service.BlockUser(username, blockUser);
            return Ok($"{blockUser} blocked for {username}");
        }
    }
}