using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace VariantBot.Bots
{
    public class VariantBot : ActivityHandler
    {
        private readonly ILogger<VariantBot> _logger;

        public VariantBot(ILogger<VariantBot> logger)
        {
            _logger = logger;
        }
        
        protected override Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
