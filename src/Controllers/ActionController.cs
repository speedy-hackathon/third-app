using covidSim.Models;
using covidSim.Services;
using Microsoft.AspNetCore.Mvc;

namespace covidSim.Controllers
{
    [Route("api/action")]
    public class ActionController : Controller
    {
        [HttpPost]
        public IActionResult Action([FromBody] UserActionDto userAction)
        {
            var game = Game.Instance;
            var person = game.People.Find(p => p.Id == userAction.PersonClicked);
            person.GoHome();
            game.InfectNeighbors();
            return NoContent();
        }

        [HttpPost]
        [Route("Reset")]
        public IActionResult Reset()
        {
            return Ok(Game.Reset());
        }
    }
}