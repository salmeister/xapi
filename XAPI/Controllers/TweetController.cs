using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using XAPI.Models;
using XAPI.Services;

namespace XAPI.Controllers
{
    public class TweetController(ILogger<TweetController> logger, AppSettings appSettings, HttpClient httpClient, ITweetService tweetService) : ControllerBase
    {
        private readonly ITweetService tweetService = new TweetService(appSettings, httpClient);

        /// <summary>
        /// This endpoint returns the user information for the given user handle.
        /// </summary>
        /// <param name="userHandle"></param>
        /// <returns></returns>
        [SwaggerOperation(Tags = new[] { "Users" })]
        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser(string userHandle)
        {
            try
            {
                if (ModelState.IsValid)
                {
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
                if (!appSettings.Environment.Contains("prod", StringComparison.CurrentCultureIgnoreCase))
                {
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "An error occurred while getting the user info.");
                }
            }
        }

        /// <summary>
        /// This returns a 403 -- Forbidden error. The Basic API token is not authorized to access this endpoint.
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        [SwaggerOperation(Tags = new[] { "Users" })]
        [HttpGet("GetFollowing")]
        public async Task<IActionResult> GetFollowing(string userID)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = await tweetService.GetFollowingByUserID(userID);
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
                if (!appSettings.Environment.Contains("prod", StringComparison.CurrentCultureIgnoreCase))
                {
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "An error occurred while getting the user info.");
                }
            }
        }

        /// <summary>
        /// This endpoint returns the tweets for the given tweet IDs.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [SwaggerOperation(Tags = new[] { "Tweets" })]
        [HttpGet("GetTweets")]
        public async Task<IActionResult> GetTweets(List<string> ids)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = await tweetService.GetTweets(ids);
                    return Ok(response);
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while getting the tweets.");
                if (!appSettings.Environment.Contains("prod", StringComparison.CurrentCultureIgnoreCase))
                {
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "An error occurred while getting the tweets.");
                }
            }
        }

        /// <summary>
        /// This endpoint returns the tweets for the given user ID for the given time period.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="dayRange"></param>
        /// <returns></returns>
        [SwaggerOperation(Tags = new[] { "Tweets" })]
        [HttpGet("GetTweetsByUserIDForTimePeriod")]
        public async Task<IActionResult> GetTweetsByUserIDForTimePeriod(string userID, DayRange dayRange)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = await tweetService.GetTweetsByUserIDInTimeSpan(userID, dayRange);
                    return Ok(response);
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while getting the users tweets for today.");
                if (!appSettings.Environment.Contains("prod", StringComparison.CurrentCultureIgnoreCase))
                {
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "An error occurred while getting the users tweets for today.");
                }
            }
        }

        /// <summary>
        /// This endpoint returns the lists for the given user ID. -- Forbidden error. The Basic API token is not authorized to access this endpoint.
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        [SwaggerOperation(Tags = new[] { "Lists" })]
        [HttpGet("GetListsByUserID")]
        public async Task<IActionResult> GetListsByUserID(string userID)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = await tweetService.GetListsByUserID(userID);
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
                if (!appSettings.Environment.Contains("prod", StringComparison.CurrentCultureIgnoreCase))
                {
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "An error occurred while getting the user info.");
                }
            }
        }

        [SwaggerOperation(Tags = new[] { "Lists" })]
        [HttpGet("GetTweetsByListIDForTimePeriod")]
        public async Task<IActionResult> GetTweetsByListIDForTimePeriod(string listID, DayRange dayRange)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = await tweetService.GetTweetsByListIDInTimeSpan(listID, dayRange);
                    return Ok(response);
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while getting the users tweets for today.");
                if (!appSettings.Environment.Contains("prod", StringComparison.CurrentCultureIgnoreCase))
                {
                    return StatusCode(500, ex.Message);
                }
                else
                {
                    return StatusCode(500, "An error occurred while getting the users tweets for today.");
                }
            }
        }
    }
}
