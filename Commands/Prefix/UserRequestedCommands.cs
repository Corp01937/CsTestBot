using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;
using OpenAI_API;
using System.Linq;
using System.Threading.Tasks;

namespace YouTubeTestBot.Commands.Prefix
{
    public class UserRequestedCommands : BaseCommandModule
    {

        [Command("gpt")]
        public async Task ChatGPT(CommandContext ctx, params string[] message)
        {
            //Initialise the API
            var api = new OpenAIAPI("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");

            //Initialise a new Chat
            var chat = api.Chat.CreateConversation();
            chat.AppendSystemMessage("Type in a query");

            //Pass in the query to GPT
            chat.AppendUserInput(string.Join(" ", message));

            //Get the response
            string response = await chat.GetResponseFromChatbot();

            //Show in Discord Embed Message
            var responseMsg = new DiscordEmbedBuilder()
            {
                Title = string.Join(" ", message),
                Description = response,
                Color = DiscordColor.Green
            };

            await ctx.Channel.SendMessageAsync(embed: responseMsg);
        }
    }
}
