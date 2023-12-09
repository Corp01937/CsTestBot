/*

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace CsTestBot.Commands.Slash_Commands
{
    public class RoleReact : ApplicationCommandModule
    {
        [SlashCommand("RoleReact", "SetUp roles from reactions to a message")]
        public async Task RoleReactCommand(InteractionContext ctx, [Option("Title", "The main subject")] string Title,
                                                              [Option("option1", "Option 1")] string Option1,
                                                              [Option("option2", "Option 1")] string Option2,
                                                              [Option("option3", "Option 1")] string Option3,
                                                              [Option("option4", "Option 1")] string Option4,
                                                              [Option("option5", "Option 1")] string Option5,
                                                              [Option("option6", "Option 1")] string Option6,
                                                              [Option("option7", "Option 1")] string Option7,
                                                              [Option("option8", "Option 1")] string Option8,
                                                              [Option("option9", "Option 1")] string Option9,
                                                              [Option("option10", "Option 1")] string Option10)//Asking for names of each option
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                                                            .WithContent("Creating Embed"));

            var interactvity = Program.Client.GetInteractivity(); //Getting the Interactivity Module

            DiscordEmoji[] optionEmojis = { DiscordEmoji.FromName(Program.Client, ":one:", false),
                                            DiscordEmoji.FromName(Program.Client, ":two:", false),
                                            DiscordEmoji.FromName(Program.Client, ":three:", false),
                                            DiscordEmoji.FromName(Program.Client, ":four:", false),
                                            DiscordEmoji.FromName(Program.Client, ":five:", false),
                                            DiscordEmoji.FromName(Program.Client, ":six:", false),
                                            DiscordEmoji.FromName(Program.Client, ":seven:", false),
                                            DiscordEmoji.FromName(Program.Client, ":eight:", false),
                                            DiscordEmoji.FromName(Program.Client, ":nine:", false),
                                            DiscordEmoji.FromName(Program.Client, ":ten:", false)}; //Array to store discord emojis

            string optionsString = optionEmojis[0] + " | " + Option1 + "\n" +
                                   optionEmojis[1] + " | " + Option2 + "\n" +
                                   optionEmojis[2] + " | " + Option3 + "\n" +
                                   optionEmojis[3] + " | " + Option4 + "\n" +      
                                   optionEmojis[4] + " | " + Option5 + "\n" +      
                                   optionEmojis[5] + " | " + Option6 + "\n" +      
                                   optionEmojis[6] + " | " + Option7 + "\n" +      
                                   optionEmojis[7] + " | " + Option8 + "\n" +      
                                   optionEmojis[8] + " | " + Option9 + "\n" +      
                                   optionEmojis[9] + " | " + Option10; //String to display each option with its associated emojis
            
            
            var RoleReactMessage = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Azure)
                    .WithTitle(string.Join(" ", Title))
                    .WithDescription(optionsString)); //Making the Role React message

            var putReactOn = await ctx.Channel.SendMessageAsync(RoleReactMessage); //Storing the await command in a variable

            foreach (var emoji in optionEmojis)
            {
                await putReactOn.CreateReactionAsync(emoji); //Adding each emoji from the array as a reaction on this message
            }

            var result = await interactvity.CollectReactionsAsync(putReactOn);
            
            await ctx.Channel.SendMessageAsync(RoleReactMessage); 

        }
    }
}
*/