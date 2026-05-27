using Microsoft.AspNetCore.Mvc;
using DrawItFaster.DAL;
using System.Collections.Generic;

namespace DrawItFaster.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WordAPIController : ControllerBase
    {
        private readonly WordRepository _wordRepo;

        public WordAPIController(WordRepository wordRepo)
        {
            _wordRepo = wordRepo;
        }

        [HttpGet("random")]
        public IActionResult GetThreeRandomWords()
        {
            List<string> words = _wordRepo.GetThreeRandomWords();

            return Ok(words);
        }
    }
}