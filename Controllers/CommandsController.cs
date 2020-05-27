using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VariantBot.Slack;

namespace VariantBot.Controllers
{
    [Route("api/commands")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly ILogger<CommandsController> _logger;

        public CommandsController(ILogger<CommandsController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromForm] SlackCommandFormBody slackCommandFormBody)
        {
            if (string.IsNullOrWhiteSpace(slackCommandFormBody.Command))
                return BadRequest();

            switch (slackCommandFormBody.Command)
            {
                case "/info":
                case "/hjelp":
                {
                    if (!string.IsNullOrWhiteSpace(slackCommandFormBody.Text))
                    {
                        await Info.SendInteractionResponse(slackCommandFormBody.Text, slackCommandFormBody.ResponseUrl);

                        return Ok();
                    }

                    var infoCommandMessage = await SlackMessage.CreateInfoCommandMessage();
                    var jsonContent = JsonConvert.SerializeObject(infoCommandMessage);
                    await SlackMessage.Post(jsonContent, slackCommandFormBody.ResponseUrl);
                    return Ok();
                }
                
                case "/reload":
                {
                    try
                    {
                        return Ok();
                    }
                    finally
                    {
                        Response.OnCompleted(async () =>
                        {
                            await Config.LoadConfigFromSharePoint();
                        });
                    }
                }
            }

            return BadRequest();
        }

        public class SlackCommandFormBody
        {
            public string Text { get; set; }
            public string Command { get; set; }

            [FromForm(Name = "user_id")] public string UserId { get; set; }

            [FromForm(Name = "response_url")] public string ResponseUrl { get; set; }
        }
    }
}