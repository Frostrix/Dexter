﻿using Dexter.Abstractions;
using Dexter.Databases.ReactionMenus;
using Dexter.Enums;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dexter.Services {

    public class ReactionMenuService : Service {

        public ReactionMenuDB ReactionMenuDB { get; set; }

        public override void Initialize() {
            DiscordSocketClient.ReactionAdded += ReactionMenu;
        }

        public async Task ReactionMenu(Cacheable<IUserMessage, ulong> CachedMessage, ISocketMessageChannel Channel, SocketReaction Reaction) {
            ReactionMenu ReactionMenu = ReactionMenuDB.ReactionMenus.Find(CachedMessage.Id);

            if (ReactionMenu == null || Reaction.User.Value.IsBot)
                return;

            IUserMessage Message = await CachedMessage.GetOrDownloadAsync();

            EmbedBuilder[] Menus = JsonConvert.DeserializeObject<EmbedBuilder[]>
                (ReactionMenuDB.EmbedMenus.Find(ReactionMenu.EmbedMenuIndex).EmbedMenuJSON);

            if (Reaction.Emote.Name.Equals("⬅️")) {
                ReactionMenu.CurrentPage--;
                if (ReactionMenu.CurrentPage < 1)
                    ReactionMenu.CurrentPage = Menus.Length;
            } else if (Reaction.Emote.Name.Equals("➡️")) {
                ReactionMenu.CurrentPage++;
                if (ReactionMenu.CurrentPage > Menus.Length)
                    ReactionMenu.CurrentPage = 1;
            }

            await Message.ModifyAsync(MessageP => MessageP.Embed = CreateMenuEmbed(ReactionMenu));

            await Message.RemoveReactionAsync(Reaction.Emote, Reaction.User.Value);
        }

        public async Task CreateReactionMenu(EmbedBuilder[] EmbedBuilders, ISocketMessageChannel Channel) {
            RestUserMessage Message = await Channel.SendMessageAsync(
                embed: BuildEmbed(EmojiEnum.Unknown).WithTitle("Setting up reaction menu-").Build()
            );

            List<uint> Colors = new ();

            foreach (EmbedBuilder Builder in EmbedBuilders)
                Colors.Add(Builder.Color.HasValue ? Builder.Color.Value.RawValue : Color.Blue.RawValue);

            int EmbedMenuID;
            string EmbedMenuJSON = JsonConvert.SerializeObject(EmbedBuilders);

            EmbedMenu EmbedMenu = await ReactionMenuDB.EmbedMenus.AsAsyncEnumerable()
                .Where(Menu => Menu.EmbedMenuJSON.Equals(EmbedMenuJSON)).FirstOrDefaultAsync();

            if (EmbedMenu != null)
                EmbedMenuID = EmbedMenu.EmbedIndex;
            else {
                EmbedMenuID = await ReactionMenuDB.EmbedMenus.AsAsyncEnumerable().CountAsync();

                await ReactionMenuDB.EmbedMenus.AddAsync(new EmbedMenu() {
                    EmbedIndex = EmbedMenuID,
                    EmbedMenuJSON = EmbedMenuJSON
                });
            }

            int ColorMenuID;
            string ColorMenuJSON = JsonConvert.SerializeObject(Colors.ToArray());

            ColorMenu ColorMenu = await ReactionMenuDB.ColorMenus.AsAsyncEnumerable()
                .Where(Menu => Menu.ColorMenuJSON.Equals(ColorMenuJSON)).FirstOrDefaultAsync();

            if (ColorMenu != null)
                ColorMenuID = ColorMenu.ColorIndex;
            else {
                ColorMenuID = await ReactionMenuDB.ColorMenus.AsAsyncEnumerable().CountAsync();

                await ReactionMenuDB.ColorMenus.AddAsync(new ColorMenu() {
                    ColorIndex = ColorMenuID,
                    ColorMenuJSON = ColorMenuJSON
                });
            }

            ReactionMenu ReactionMenu = new() {
                CurrentPage = 1,
                MessageID = Message.Id,
                ColorMenuIndex = ColorMenuID,
                EmbedMenuIndex = EmbedMenuID
            };

            ReactionMenuDB.ReactionMenus.Add(ReactionMenu);

            ReactionMenuDB.SaveChanges();

            await Message.ModifyAsync(MessageP => MessageP.Embed = CreateMenuEmbed(ReactionMenu));

            await Message.AddReactionAsync(new Emoji("⬅️"));
            await Message.AddReactionAsync(new Emoji("➡️"));
        }

        public Embed CreateMenuEmbed (ReactionMenu ReactionMenu) {
            EmbedBuilder[] Menus = JsonConvert.DeserializeObject<EmbedBuilder[]>
                (ReactionMenuDB.EmbedMenus.Find(ReactionMenu.EmbedMenuIndex).EmbedMenuJSON);

            uint[] Colors = JsonConvert.DeserializeObject<uint[]>
                (ReactionMenuDB.ColorMenus.Find(ReactionMenu.ColorMenuIndex).ColorMenuJSON);

            int CurrentPage = ReactionMenu.CurrentPage - 1;

            return Menus[CurrentPage]
                .WithColor(new Color(Colors[CurrentPage]))
                .WithFooter($"Page {ReactionMenu.CurrentPage}/{Menus.Length}")
                .WithCurrentTimestamp().Build();
        }

    }

}