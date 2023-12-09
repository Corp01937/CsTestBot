using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsTestBot.Commands.Slash_Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using YouTubeTestBot.Commands.Prefix;
using YouTubeTestBot.Commands.Slash_Commands;
using YouTubeTestBot.Config;
using YouTubeTestBot.Engine;

namespace CsTestBot
{
    public sealed class Program
    {
        //Main Discord Properties
        public static DiscordClient Client { get; set; }
        private static CommandsNextExtension Commands { get; set; }

        //Miscaleneous Properties
        private static int ImageIDCounter = 0;
        private static Dictionary<string, ulong> voiceChannelIDs = new Dictionary<string, ulong>();

        static async Task Main(string[] args)
        {

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
            Client.ComponentInteractionCreated += InteractionEventHandler;
            Client.MessageCreated += MessageSendHandler;
            Client.ModalSubmitted += ModalEventHandler;
            Client.VoiceStateUpdated += VoiceChannelHandler;
            Client.GuildMemberAdded += UserJoinHandler;

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
            Commands.RegisterCommands<UserRequestedCommands>();
            Commands.RegisterCommands<DiscordComponentCommands>();

            //Slash Commands
            slashCommandsConfig.RegisterCommands<BasicSlashCmd>();
            slashCommandsConfig.RegisterCommands<ModerationSL>();
            //slashCommandsConfig.RegisterCommands<RoleReact>();

            //ERROR EVENT HANDLERS
            Commands.CommandErrored += OnCommandError;

        }

        private static Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }

        private static async Task UserJoinHandler(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            var defaultChannel = e.Guild.GetDefaultChannel();

            var welcomeEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Gold,
                Title = $"Welcome {e.Member.Username} to the server",
                Description = "Hope you enjoy your stay, please read the rules"
            };

            await defaultChannel.SendMessageAsync(embed: welcomeEmbed);
        }

        private static async Task VoiceChannelHandler(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            var channel = e.Channel;
            var mainUserName = e.User.Username;
            if (channel != null && channel.Name == "Create" && e.Before == null) //Joining a VC
            {
                Console.WriteLine($"Joined VC {channel.Name}");

                //Creating the VC
                var userVC = await e.Guild.CreateVoiceChannelAsync($"{e.User.Username}'s Channel", e.Channel.Parent);
                voiceChannelIDs.Add(e.User.Username, userVC.Id);

                var member = await e.Guild.GetMemberAsync(e.User.Id);
                await member.ModifyAsync(x => x.VoiceChannel = userVC);
            }
            if (e.User.Username == mainUserName && channel == null && e.Before != null && e.Before.Channel != null && e.Before.Channel.Name == $"{e.User.Username}'s Channel") //Leaving the VC
            {
                Console.WriteLine($"Left the VC {e.Before.Channel.Name}");
                var channelID = voiceChannelIDs.TryGetValue(e.User.Username, out ulong channelToDelete);
                var leftChannel = e.Guild.GetChannel(channelToDelete);
                await leftChannel.DeleteAsync();

                voiceChannelIDs.Remove(e.User.Username);
            }
        }

        private static async Task ModalEventHandler(DiscordClient sender, ModalSubmitEventArgs e)
        {
            if (e.Interaction.Type == InteractionType.ModalSubmit && e.Interaction.Data.CustomId == "modal")
            {
                var values = e.Values;
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{e.Interaction.User.Username} submitted a modal with the input {values.Values.First()}"));
            }
        }

        private static async Task MessageSendHandler(DiscordClient sender, MessageCreateEventArgs e)
        {
            //Swear Filter
            var swearFilter = new SwearFilter();
            foreach (var word in swearFilter.filter)
            {
                if (e.Message.Content.Contains(word))
                {
                    await e.Channel.SendMessageAsync("That message had a swear word in it, you have been warned");
                }
            }

            //Image Counter Reset
            if (e.Message.Content == "!image")
            {
                ImageIDCounter = 0; //Reset the counter when someone uses this command
            }

        }

        private static async Task InteractionEventHandler(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            //Drop-Down Events
            if (e.Id == "dropDownList" && e.Interaction.Data.ComponentType == ComponentType.StringSelect)
            {
                var options = e.Values;
                foreach (var option in options)
                {
                    switch (option)
                    {
                        case "option1":
                            await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent($"{e.User.Username} has selected Option 1"));
                            break;

                        case "option2":
                            await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent($"{e.User.Username} has selected Option 2"));
                            break;

                        case "option3":
                            await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent($"{e.User.Username} has selected Option 3"));
                            break;
                    }
                }
            }
            else if (e.Id == "channelDropDownList")
            {
                var options = e.Values;
                foreach (var channel in options)
                {
                    var selectedChannel = await Client.GetChannelAsync(ulong.Parse(channel));
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{e.User.Username} selected the channel with name {selectedChannel.Name}"));
                }
            }

            else if (e.Id == "mentionDropDownList")
            {
                var options = e.Values;
                foreach (var user in options)
                {
                    var selectedUser = await Client.GetUserAsync(ulong.Parse(user));
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent($"{selectedUser.Mention} was mentionned"));
                }
            }

            //Button Events
            if (e.Interaction.Data.CustomId == "1")
            {
                var options = e.Values;
                foreach (var option in options)
                {
                    switch (option)
                    {
                        case "option1":
                            await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent("You selected Option1"));
                            break;

                        case "option2":
                            await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent("You selected Option2"));
                            break;

                        case "option3":
                            await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent("You selected Option3"));
                            break;

                        default:
                            Console.WriteLine("Error in Interaction Event Handler");
                            break;
                    }
                }
            }

        }

        private static async Task OnCommandError(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            //Casting my ErrorEventArgs as a ChecksFailedException
            if (e.Exception is ChecksFailedException castedException)
            {
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

        private static void WebsiteMonitoring()
        {

        }
    }
}
