using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace YouTubeTestBot.Commands.Prefix
{
    public class BasicCommands : BaseCommandModule
    {
        [Command("message")]
        [Cooldown(5, 10, CooldownBucketType.User)]
        public async Task TestCommand(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Test Message");
        }

        [Command("embedmessage1")]
        public async Task SendEmbedMessage2(CommandContext ctx) //Example 1
        {
            var embedMessage = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Azure)
                    .WithTitle("This is a title")
                    .WithDescription("This is a description"));

            await ctx.Channel.SendMessageAsync(embedMessage);
        }

        [Command("embedmessage2")]
        public async Task SendEmbedMessage1(CommandContext ctx) //Example 2
        {
            var embedMessage = new DiscordEmbedBuilder()
            {
                Title = "This is a title",
                Description = "This is a description",
                Color = DiscordColor.Azure,
            };

            await ctx.Channel.SendMessageAsync(embed: embedMessage);
        }

        [Command("help")]
        public async Task HelpCommand(CommandContext ctx) 
        {
            var funButton = new DiscordButtonComponent(ButtonStyle.Success, "funButton", "Fun");
            var gameButton = new DiscordButtonComponent(ButtonStyle.Success, "gameButton", "Games");

            var helpMessage = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Azure)
                    .WithTitle("Help Menu")
                    .WithDescription("Please pick a button for more information on the commands"))
                .AddComponents(funButton, gameButton);

            await ctx.Channel.SendMessageAsync(helpMessage);
        }
    }
}
