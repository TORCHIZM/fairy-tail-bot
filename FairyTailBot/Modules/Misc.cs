using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FairyTail_Bot.Database;
using FairyTail_Bot.Models;
using FairyTail_Bot.Core;
using Newtonsoft.Json;

namespace FairyTail_Bot.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        [Command("category")]
        [Alias("kategori")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task SetWordChannel(SocketCategoryChannel category)
        {
            var server = Server.Get(category.Guild.Id);
            if (server == null)
            {
                Server.New(category.Guild.Id, category.Id);
            }
            else
            {
                server.ChannelCategory = category.Id;
                server.Save();
            }

            await category.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, OverwritePermissions.InheritAll.Modify(
                viewChannel: PermValue.Deny
            ));

            await Context.Message.AddReactionAsync(new Emoji("👍"));
        }

        [Command("debug")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task Debug()
        {
            //if (args == "all")
            //{
            //	await Context.Channel.SendMessageAsync(JsonConvert.SerializeObject(Base.Queue));
            //	return;
            //}

            var lobby = Base.Queue.Where(x => x.TextChannelId == Context.Channel.Id);

            if (lobby != null)
                return;

            await Context.Channel.SendMessageAsync(JsonConvert.SerializeObject(lobby));
        }

        [Command("clearlobies")]
        [Alias("lobilerisil")]
        public async Task Clear()
        {
            var rooms = Context.Guild.Channels.ToArray();

            foreach (var room in rooms)
                if (room.Name.StartsWith("lobi-"))
                    await room.DeleteAsync();

            await Context.Message.AddReactionAsync(new Emoji("👍🏻"));
        }

        [Command("leave")]
        [Alias("ayrıl")]
        public async Task Leave()
        {
            if (Base.Queue.Any(x => x.UserIDs.Contains(Context.User.Id)))
            {
                var lobby = Base.Queue.Where(x => x.UserIDs.Contains(Context.User.Id)).FirstOrDefault();
                lobby.Remove(Context.User.Id);
            }
        }

        [Command("invite")]
        [Alias("davet")]
        public async Task Invite(IGuildUser user)
        {
            if (Base.Queue.Any(
                x => x.Owner == Context.User.Id &&
                x.GameState == GameState.Waiting &&
                x.Count < x.MaxPlayers))
            {
                Base.InviteList.Add(Context.Message.Id, user.Id);
                await Context.Message.AddReactionAsync(new Emoji("📬"));
            }
        }

        [Command("create")]
        [Alias("kur")]
        public async Task Create(int maxUser = 5, [Remainder] string name = null)
        {
            if (Base.CanCreateRoom(Context.User.Id))
            {
                var server = Server.Get(Context.Guild.Id);
                var lobby = new Lobby(Context.User.Id, Context.Guild.Id, maxUser, name);

                var textChannel = await Context.Guild.CreateTextChannelAsync($"lobi-{lobby.RoomId}");
                var voiceChannel = await Context.Guild.CreateVoiceChannelAsync($"lobi-{lobby.RoomId}");

                await textChannel.ModifyAsync(c => c.CategoryId = server.ChannelCategory);
                await voiceChannel.ModifyAsync(c =>
                {
                    c.CategoryId = server.ChannelCategory;
                    c.UserLimit = lobby.MaxPlayers;
                });

                await textChannel.SyncPermissionsAsync();
                await voiceChannel.SyncPermissionsAsync();

                lobby.InitializeChannels(textChannel, voiceChannel);

                await Context.Message.AddReactionAsync(new Emoji("👍🏻"));
            }
            else
            {
                await Context.Channel.SendMessageAsync("Zaten halihazırda bir lobiniz bulunmakta.");
            }
        }

        [Command("kick")]
        [Alias("at")]
        public async Task Kick(SocketUser user)
        {
            if (Base.Queue.Any(x => x.UserIDs.Contains(Context.User.Id) && x.UserIDs.Contains(user.Id)))
            {
                var lobby = Base.Queue.Where(x => x.UserIDs.Contains(Context.User.Id)).FirstOrDefault();
                lobby.Kick(user.Id, Context.User.Id);
            }
        }

        [Command("ready")]
        [Alias("hazır")]
        public async Task Ready()
        {
            if (Base.Queue.Any(x => x.TextChannelId == Context.Channel.Id && x.Owner == Context.User.Id))
            {
                var lobby = Base.Queue.Where(x => x.TextChannelId == Context.Channel.Id).FirstOrDefault();
                var usersInVoiceChannel = await lobby.VoiceChannel.GetUsersAsync().FlattenAsync();
                lobby.Start();
            }
        }

        [Command("find")]
        [Alias("bul")]
        public async Task Find()
        {
            //if (Base.Queue.Count == 0)
            //{
            //    var Lobby = new Lobby(Context.Guild.Id);
            //}

            var server = Server.Get(Context.Guild.Id);

            if (server == null)
            {
                await Context.Channel.SendMessageAsync("Bir kategori ayarlanmamış. **!kategori <kategori-id>** komutu ile odaların açılacağı kategori belirleyiniz.");
                return;
            }

            if (Base.Queue.Count != 0 &&
                Base.Queue.Any(x => x.Count != 0 && x.UserIDs.Contains(Context.User.Id)))
            {
                await Context.Channel.SendMessageAsync("Şuanda zaten bir lobidesin. Lobiden çıkmak için mesajının altındaki emojiye tıkla.");
                await Context.Message.AddReactionAsync(new Emoji("🖕🏻"));
                return;
            }

            var lobby = Base.Queue.Where(x => x.GameState == GameState.Waiting && x.Count < x.MaxPlayers && !x.Private).OrderBy(x => x.Count).FirstOrDefault();

            if (lobby == null)
                lobby = new Lobby(Context.User.Id, Context.Guild.Id);
            else
                lobby.Add(Context.User.Id, Context.Message);

            if (!lobby.IsReady())
            {
                var textChannel = await Context.Guild.CreateTextChannelAsync($"lobi-{lobby.RoomId}");
                var voiceChannel = await Context.Guild.CreateVoiceChannelAsync($"lobi-{lobby.RoomId}");

                await textChannel.ModifyAsync(c => c.CategoryId = server.ChannelCategory);
                await voiceChannel.ModifyAsync(c =>
                {
                    c.CategoryId = server.ChannelCategory;
                    c.UserLimit = lobby.MaxPlayers;
                });

                await textChannel.SyncPermissionsAsync();
                await voiceChannel.SyncPermissionsAsync();

                lobby.InitializeChannels(textChannel, voiceChannel);
            }
        }

        //[Command("top")]
        //public async Task TopTen()
        //{
        //    var user = MySQLCommands.Select<User>($"SELECT * FROM users WHERE uid={Context.User.Id}").FirstOrDefault();

        //    var embed = new EmbedBuilder()
        //    {
        //        Author = new EmbedAuthorBuilder()
        //        {
        //            Name = Context.User.Username,
        //            IconUrl = Context.User.GetAvatarUrl(size: 512)
        //        },
        //        Title = $"{Context.User.Username}, {user.Points} puana sahipsin."
        //    };

        //    await ReplyAsync(embed: embed.Build());
        //}

        //[Command("top")]
        //public async Task TopTen(string arg)
        //{
        //    var embed = new EmbedBuilder();

        //    string[] args = arg.Split(' ');

        //    if (args[0] == "yardım")
        //    {
        //        embed.Title = "Yardım";

        //        embed.AddField("10", "En çok puana sahip 10 kişiyi gösterir");
        //        embed.AddField("(@Kişi Etiketi)", "Etiketlediğiniz kişinin puanını gösterir");

        //        await ReplyAsync(embed: embed.Build());
        //    }

        //    if (args[0] == "10")
        //    {
        //        var users = MySQLCommands.Select<User>($"SELECT * FROM users ORDER BY points DESC LIMIT 10;");

        //        embed.Title = "En çok oyun oynayan 10 kişi";

        //        foreach (var user in users)
        //            embed.AddField(Global.Client.GetUser(Convert.ToUInt64(user.UID)).Username, $"{user.Points}");

        //        await ReplyAsync(embed: embed.Build());
        //    }

        //    if (args[0].StartsWith("<@"))
        //    {
        //        var mentionedUser = (Context.Message.MentionedUsers as IEnumerable<SocketUser>).FirstOrDefault();

        //        var user = MySQLCommands.Select<User>($"SELECT * FROM users WHERE uid={mentionedUser.Id}").FirstOrDefault();

        //        if (user == null)
        //        {
        //            embed.Description = "Veri bulunamadı!";
        //            await ReplyAsync(embed: embed.Build());
        //        }
        //        else
        //        {
        //            embed.Description = $"**{mentionedUser.Mention}, {user.Points} puana sahip.**";

        //            await ReplyAsync(embed: embed.Build());
        //        }
        //    }
        //}
    }
}