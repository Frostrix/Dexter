﻿using Dexter.Core.Abstractions;
using Discord.Commands;
using System.Threading.Tasks;

namespace Dexter.Commands.UtilityCommands {
    public partial class UtilityCommands {

        [Command("ping")]
        [Summary("Displays the latency between both Discord and I.")]
        [Alias("latency")]
        public async Task PingCommand() {
            await Context.BuildEmbed(EmojiEnum.Love)
                .WithTitle("Gateway Ping")
                .WithDescription($"**{Context.Client.Latency}ms**")
                .SendEmbed(Context.Channel);
        }

    }
}
