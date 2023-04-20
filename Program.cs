﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System;
using System.Threading.Tasks;
using System.Timers;
using YouTubeTestBot.Commands;
using YouTubeTestBot.Config;
using YouTubeTestBot.Engine.ImageHandler;
using YouTubeTestBot.Engine.LevelSystem;
using YouTubeTestBot.Engine.YouTube;
using YouTubeTestBot.Slash_Commands;

namespace YouTubeTestBot
{
    public sealed class Program
    {
        //Main Discord Properties
        private static DiscordClient Client { get; set; }
        private static InteractivityExtension Interactivity { get; set; }
        private static CommandsNextExtension Commands { get; set; }

        //YouTube Properties
        private static YouTubeVideo _video = new YouTubeVideo();
        private static YouTubeVideo temp = new YouTubeVideo();
        private static YouTubeEngine _YouTubeEngine = new YouTubeEngine();

        //Miscaleneous Properties
        private static int ImageIDCounter = 0;
        public static GoogleImageHandler imageHandler;

        static async Task Main(string[] args)
        {
            //Instantiating the class with the Instance property
            imageHandler = GoogleImageHandler.Instance;

            //Reading the Token & Prefix
            var configJson = new ConfigJSONReader();
            await configJson.ReadJSON();

            //Making a Bot Configuration with our token & additional settings
            var config = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = configJson.discordToken,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
            };

            //Initializing the client with this config
            Client = new DiscordClient(config);

            //Setting our default timeout for Interactivity based commands
            Client.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(2)
            });

            //EVENT HANDLERS
            Client.Ready += OnClientReady;
            Client.ComponentInteractionCreated += ButtonPressResponse;
            Client.MessageCreated += MessageSendHandler;

            //Setting up our Commands Configuration with our Prefix
            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { configJson.discordPrefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false,
            };

            //Enabling the use of commands with our config & also enabling use of Slash Commands
            Commands = Client.UseCommandsNext(commandsConfig);
            var slashCommandsConfig = Client.UseSlashCommands();

            //Prefix Based Commands
            Commands.RegisterCommands<BasicCommands>();
            Commands.RegisterCommands<GameCommands>();
            Commands.RegisterCommands<UserRequestedCommands>();

            //Slash Commands
            slashCommandsConfig.RegisterCommands<FunSL>();
            slashCommandsConfig.RegisterCommands<ModerationSL>();

            //ERROR EVENT HANDLERS
            Commands.CommandErrored += OnCommandError;

            //Connect to the Client and get the Bot online
            await Client.ConnectAsync();

            ulong channelIdToNotify = 123456789; // your Discord channel ID
            await StartVideoUploadNotifier(_YouTubeEngine.channelId, _YouTubeEngine.apiKey, Client, channelIdToNotify);
            await Task.Delay(-1);
        }

        private static async Task MessageSendHandler(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (e.Message.Content == "!image")
            {
                ImageIDCounter = 0; //Reset the counter when someone uses this command
            }

            var levelEngine = new LevelEngine();
            var addedXP = levelEngine.AddXP(e.Author.Username, e.Guild.Id);
            if (levelEngine.levelledUp == true)
            {
                var levelledUpEmbed = new DiscordEmbedBuilder()
                {
                    Title = e.Author.Username + " has levelled up!!!!",
                    Description = "Level: " + levelEngine.GetUser(e.Author.Username, e.Guild.Id).Level.ToString(),
                    Color = DiscordColor.Chartreuse
                };

                await e.Channel.SendMessageAsync(e.Author.Mention, embed: levelledUpEmbed);
            }
        }

        private static async Task ButtonPressResponse(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            switch (e.Interaction.Data.CustomId)
            {
                case "1":
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You pressed the 1st Button"));
                    break;

                case "2":
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent("You pressed the 2nd Button"));
                    break;

                case "funButton":
                    string funCommandsList = "!message -> Send a message \n" +
                         "!embedmessage1 -> Sends an embed message \n" +
                         "!poll -> Starts a poll";

                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent(funCommandsList));

                    break;

                case "gameButton":
                    string gamesList = "!cardgame -> Play a simple card game. Whoever draws the highest wins the game";

                    var gamesCommandList = new DiscordEmbedBuilder()
                    {
                        Title = gamesList,
                    };

                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(gamesCommandList));

                    break;

                case "previousButton":
                    ImageIDCounter--; //Decrement the ID by 1 to get the ID for the previous image
                    string imageURL = Program.imageHandler.GetImageAtId(ImageIDCounter); //Get the image from the Dictionary

                    //Initialise the Buttons again

                    var previousEmoji = new DiscordComponentEmoji(DiscordEmoji.FromName(Client, ":track_previous:"));
                    var previousButton = new DiscordButtonComponent(ButtonStyle.Primary, "previousButton", "Previous", false, previousEmoji);

                    var nextEmoji = new DiscordComponentEmoji(DiscordEmoji.FromName(Client, ":track_next:"));
                    var nextButton = new DiscordButtonComponent(ButtonStyle.Primary, "nextButton", "Next", false, nextEmoji);

                    //Send the new image as a response to the button press, replacing the previous image

                    var imageMessage = new DiscordMessageBuilder()
                        .AddEmbed(new DiscordEmbedBuilder()
                        .WithColor(DiscordColor.Azure)
                        .WithTitle("Results")
                        .WithImageUrl(imageURL)
                        .WithFooter("Page " + ImageIDCounter)
                        )
                        .AddComponents(previousButton, nextButton);
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(imageMessage.Embed).AddComponents(imageMessage.Components));

                    break;

                case "nextButton":
                    ImageIDCounter++; //Same idea but this time you increment the counter by 1 to get the next image
                    string imageURL1 = Program.imageHandler.GetImageAtId(ImageIDCounter);

                    var previousEmoji1 = new DiscordComponentEmoji(DiscordEmoji.FromName(Client, ":track_previous:"));
                    var previousButton1 = new DiscordButtonComponent(ButtonStyle.Primary, "previousButton", "Previous", false, previousEmoji1);

                    var nextEmoji1 = new DiscordComponentEmoji(DiscordEmoji.FromName(Client, ":track_next:"));
                    var nextButton1 = new DiscordButtonComponent(ButtonStyle.Primary, "nextButton", "Next", false, nextEmoji1);

                    var imageMessage1 = new DiscordMessageBuilder()
                        .AddEmbed(new DiscordEmbedBuilder()
                        .WithColor(DiscordColor.Azure)
                        .WithTitle("Results")
                        .WithImageUrl(imageURL1)
                        .WithFooter("Page " + ImageIDCounter)
                        )
                        .AddComponents(previousButton1, nextButton1);
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(imageMessage1.Embed).AddComponents(imageMessage1.Components));

                    break;

                default:
                    Console.WriteLine("No Buttons were found with this ID");
                    break;
            }
        }

        private static Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }

        private static async Task OnCommandError(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            if (e.Exception is ChecksFailedException)
            {
                var castedException = (ChecksFailedException)e.Exception; //Casting my ErrorEventArgs as a ChecksFailedException
                string cooldownTimer = string.Empty;

                foreach (var check in castedException.FailedChecks)
                {
                    var cooldown = (CooldownAttribute)check; //The cooldown that has triggered this method
                    TimeSpan timeLeft = cooldown.GetRemainingCooldown(e.Context); //Getting the remaining time on this cooldown
                    cooldownTimer = timeLeft.ToString(@"hh\:mm\:ss");
                }

                var cooldownMessage = new DiscordEmbedBuilder()
                {
                    Title = "Wait for the Cooldown to End",
                    Description = "Remaining Time: " + cooldownTimer,
                    Color = DiscordColor.Red
                };

                await e.Context.Channel.SendMessageAsync(embed: cooldownMessage);
            }
        }

        private static async Task StartVideoUploadNotifier(string channelId, string apiKey, DiscordClient client, ulong channelIdToNotify)
        {
            var timer = new Timer(120000); //Timer set for 2 min
            timer.Elapsed += async (sender, e) => {
                _video = _YouTubeEngine.GetLatestVideo(channelId, apiKey); //Get latest video using API
                DateTime lastCheckedAt = DateTime.Now;

                if (_video != null)
                {
                    if (temp.videoTitle == _video.videoTitle) //This ensures that only the newest videos get sent through
                    {
                        Console.WriteLine("Same name");
                    }
                    else if (_video.PublishedAt < lastCheckedAt) //If the new video is actually new
                    {
                        var message = $"NEW VIDEO | **{_video.videoTitle}** \n" +
                                      $"Published at: {_video.PublishedAt} \n" +
                                      "URL: " + _video.videoUrl;

                        await client.GetChannelAsync(channelIdToNotify).Result.SendMessageAsync(message);
                        temp = _video;
                    }
                    else
                    {
                        Console.WriteLine("[" +lastCheckedAt.ToString()+ "]" + "YouTube API: No new videos were found");
                    }
                }
            };
            timer.Start();
        }
    }
}
