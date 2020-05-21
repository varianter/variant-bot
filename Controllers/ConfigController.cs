using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VariantBot.Slack;

namespace VariantBot.Controllers
{
    [Route("api/config")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        [HttpPost("reload")]
        public async Task<IActionResult> ReloadConfigAsync()
        {
            await Config.LoadConfigFromSharePoint();
            return Ok();
        }
    }
}