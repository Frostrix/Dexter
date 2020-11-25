﻿using Dexter.Abstractions;
using Dexter.Enums;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dexter.Services {
    public class ReactionMenuService : InitializableModule {

        private readonly List<ReactionMenu> ReactionMenus;

        private readonly DiscordSocketClient DiscordSocketClient;

        public ReactionMenuService (DiscordSocketClient DiscordSocketClient) {
            ReactionMenus = new ();
            this.DiscordSocketClient = DiscordSocketClient;
        }

        public override void AddDelegates() {
            DiscordSocketClient.ReactionAdded += ReactionMenu;
        }

        public async Task ReactionMenu(Cacheable<IUserMessage, ulong> CachedMessage, ISocketMessageChannel Channel, SocketReaction Reaction) {
            ReactionMenu Menu = ReactionMenus.Where(Menu => Menu.MessageID == CachedMessage.Id).FirstOrDefault();

            if (Menu == null || Reaction.User.Value.IsBot)
                return;

            if (Reaction.Emote.Name.Equals("⬅️")) {
                Menu.CurrentPage--;
                if (Menu.CurrentPage < 1)
                    Menu.CurrentPage = Menu.EmbedMenus.Length;
            } else if (Reaction.Emote.Name.Equals("➡️")) {
                Menu.CurrentPage++;
                if (Menu.CurrentPage > Menu.EmbedMenus.Length)
                    Menu.CurrentPage = 1;
            }

            IUserMessage Message = await CachedMessage.GetOrDownloadAsync();

            await Message.ModifyAsync(MessageP => MessageP.Embed = CreateMenuEmbed(Menu));

            await Message.RemoveReactionAsync(Reaction.Emote, Reaction.User.Value);
        }

        public async Task CreateReactionMenu(EmbedBuilder[] EmbedBuilders, ISocketMessageChannel Channel) {
            ReactionMenu ReactionMenu = new () {
                CurrentPage = 1,
                EmbedMenus = EmbedBuilders
            };

            RestUserMessage Message = await Channel.SendMessageAsync(
                embed: BuildEmbed(EmojiEnum.Unknown).WithTitle("Setting up reaction menu-").Build()
            );

            await Message.ModifyAsync(MessageP => MessageP.Embed = CreateMenuEmbed(ReactionMenu));

            ReactionMenu.MessageID = Message.Id;

            ReactionMenus.Add(ReactionMenu);

            await Message.AddReactionAsync(new Emoji("⬅️"));
            await Message.AddReactionAsync(new Emoji("➡️"));
        }

        public Embed CreateMenuEmbed (ReactionMenu ReactionMenu) {
            return ReactionMenu.EmbedMenus[ReactionMenu.CurrentPage - 1]
                .WithFooter($"Page {ReactionMenu.CurrentPage}/{ReactionMenu.EmbedMenus.Length}")
                .WithCurrentTimestamp().Build();
        }

    }

    public class ReactionMenu {

        public ulong MessageID { get; set; }

        public int CurrentPage { get; set; }

        public EmbedBuilder[] EmbedMenus { get; set; }

    }
}