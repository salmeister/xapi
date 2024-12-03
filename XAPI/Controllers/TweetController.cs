using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using XAPI.Models;
using XAPI.Services;

namespace XAPI.Controllers
{
    public class TweetController(ILogger<TweetController> logger, IOptions<AppSettings> settings) : ControllerBase
    {
        private readonly AppSettings appSettings = settings.Value;

        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser(string userHandle)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ITweetService tweetService = new TweetService(appSettings);
                    var response = await tweetService.GetUserByName(userHandle.TrimStart('@').Trim());
                    return Ok(response);
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while getting the users.");
                return StatusCode(500, "An error occurred while getting the users.");
            }
        }
    }
}
