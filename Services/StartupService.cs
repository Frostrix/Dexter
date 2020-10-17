﻿using Dexter.Configuration;
using Dexter.Core.Abstractions;
using Dexter.Core.Enums;
using Dexter.Core.Extensions;
using Discord;
using Discord.WebSocket;
using System.Reflection;
using System.Threading.Tasks;

namespace Dexter.Services {
    public class StartupService : InitializableModule {

        private readonly DiscordSocketClient DiscordSocketClient;
        private readonly LoggingService LoggingService;
        private readonly BotConfiguration BotConfiguration;
        private readonly CommandModule Module;

        public StartupService(DiscordSocketClient _DiscordSocketClient, BotConfiguration _BotConfiguration, LoggingService _LoggingService, CommandModule _Module) {
            DiscordSocketClient = _DiscordSocketClient;
            BotConfiguration = _BotConfiguration;
            LoggingService = _LoggingService;
            Module = _Module;
        }

        public override void AddDelegates() {
            DiscordSocketClient.Ready += DisplayStartupVersionAsync;
        }

        public async Task StartAsync(string[] Arguments) {
            if (!string.IsNullOrEmpty(BotConfiguration.Token))
                await RunBot(BotConfiguration.Token);
            else if (Arguments.Length > 0)
                await RunBot(Arguments[0]);
            else
                await LoggingService.LogMessageAsync(new LogMessage(LogSeverity.Error, "Startup", $"The login token in the {BotConfiguration.GetType().Name.Prettify()} file was not set."));
        }

        private async Task RunBot(string Token) {
            await DiscordSocketClient.LoginAsync(TokenType.Bot, Token);
            await DiscordSocketClient.StartAsync();
        }

        private async Task DisplayStartupVersionAsync() {
            ulong Guild = BotConfiguration.GuildID;
            ulong LoggingChannel = BotConfiguration.ModerationLogChannelID;

            if (Guild == 0 || LoggingChannel == 0)
                return;

            ITextChannel Channel = DiscordSocketClient.GetGuild(Guild).GetTextChannel(LoggingChannel);

            if(BotConfiguration.EnableStartupAlert)
                await Module.BuildEmbed(EmojiEnum.Love)
                .WithTitle("Startup complete!")
                .WithDescription($"This is **{BotConfiguration.Bot_Name} v{Assembly.GetExecutingAssembly().GetName().Version}** running **Discord.Net v{Discord.DiscordConfig.Version}**!")
                .SendEmbed(Channel);
        }

    }
}
