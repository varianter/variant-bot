using System.Collections.Generic;
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

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id == turnContext.Activity.Recipient.Id) continue;
                var welcomeText = $"Hello, {member.Name}/{member.Id} ,and welcome!";
                await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
            }
        }
    }
}
