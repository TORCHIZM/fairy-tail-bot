using Discord;
using Discord.Rest;
using Discord.WebSocket;
using FairyTail_Bot.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FairyTail_Bot.Models
{
    public class Lobby
    {
        public string RoomId { get; set; }
        public ulong GuildId { get; set; }
        public GameState GameState { get; set; }
        public int MaxPlayers { get; set; } = 5;
        public bool Private { get; set; }
        public ulong Owner { get; set; }
        public List<User> Users { get; set; }
        public List<ulong> UserIDs { get; set; }
        public ulong VoiceChannelId { get; set; }
        public ulong TextChannelId { get; set; }
        public IVoiceChannel VoiceChannel { get; set; }
        public ITextChannel TextChannel { get; set; }

        public int Count => UserIDs.Count;

        public Lobby(ulong Owner, ulong GuildId)
        {
            this.Owner = Owner;
            this.GuildId = GuildId;
            RoomId = Base.GetUniqueId();
            GameState = GameState.Waiting;
            Users = new List<User>();
            UserIDs = new List<ulong>();
            Add(Owner);

            Base.Queue.Add(this);
        }

        public Lobby(ulong Owner, ulong GuildId, int MaxUsers, string LobbyName = null)
        {
            this.Owner = Owner;
            this.GuildId = GuildId;
            this.MaxPlayers = MaxUsers;
            this.Private = true;
            RoomId = LobbyName ?? Base.GetUniqueId();
            GameState = GameState.Waiting;
            Users = new List<User>();
            UserIDs = new List<ulong>();
            Add(Owner);

            Base.Queue.Add(this);
        }

        public async void Add(ulong uid, SocketMessage message = null)
        {
            UserIDs.Add(uid);

            if (message != null)
                await message.AddReactionAsync(new Emoji("👍🏻"));

            if (IsReady())
            {
                SyncPermissions(uid, PermValue.Allow);
                await TextChannel.SendMessageAsync($"<@{uid}> lobiye katıldı.");
            }

            if (Count == MaxPlayers)
                StartNotification();

            Base.UpdatePresence();
        }

        public async void Remove(ulong uid, bool isKicked = false)
        {
            UserIDs.Remove(uid);

            if (Count == 0 || uid == Owner)
            {
                Destroy();
                return;
            }

            if (!IsReady())
                return;

            SyncPermissions(uid, PermValue.Deny);

            if (isKicked == false)
            {
                await TextChannel.SendMessageAsync($"<@{uid}> lobiden ayrıldı.");
                //await UserExtensions.SendMessageAsync(Global.Client.GetUser(uid), "Odadan ayrıldığınız için sıradan atıldınız.");
            }

            Base.UpdatePresence();
        }

        public async void Kick(ulong uid, ulong senderId)
        {
            if (senderId != Owner)
            {
                await TextChannel.SendMessageAsync("Bu odada yetkin yok.");
                return;
            }

            if (senderId == Owner && uid == Owner)
            {
                await TextChannel.SendMessageAsync("Kendini mi odadan atıcaksın çılgın şey!?");
                return;
            }

            if (IsReady())
            {
                await TextChannel.SendMessageAsync($"<@{uid}> lobiden atıldı.");
                Remove(uid, true);
            }
        }

        public async void Start()
        {
            this.GameState = GameState.InGame;

            foreach (var id in UserIDs)
            {
                var user = User.Get(id);
                user.GamesPlayed += 1;
                Users.Add(user);
                user.Save();
            }

            await TextChannel.SendMessageAsync($"@here çılgın mücadele başladı, yerlerinizi alın!");
        }

        public async void StartNotification()
        {
            if (IsReady())
                await TextChannel.SendMessageAsync($"Hey, <@{Owner}> herkes hazır, oyunu başlatmak için **!hazır** komutunu kullan.");
        }

        public async void Destroy()
        {
            await TextChannel.DeleteAsync();
            await VoiceChannel.DeleteAsync();

            Base.Remove(this);
        }

        public async void SyncPermissions(ulong userId = 0, PermValue permValue = PermValue.Allow)
        {
            var guild = Global.Client.GetGuild(GuildId);

            if (userId != 0)
            {
                var guildUser = guild.GetUser(userId);
                
                await TextChannel.AddPermissionOverwriteAsync(guildUser,
                    OverwritePermissions.InheritAll.Modify(
                        viewChannel: permValue,
                        readMessageHistory: permValue,
                        sendMessages: permValue,
                        attachFiles: permValue
                ));
                await VoiceChannel.AddPermissionOverwriteAsync(guildUser,
                    OverwritePermissions.InheritAll.Modify(
                        viewChannel: permValue,
                        connect: permValue,
                        speak: permValue,
                        useVoiceActivation: permValue
                ));

                return;
            }

            try
            {
                var ids = UserIDs;

                foreach (var id in ids)
                {
                    var guildUser = guild.GetUser(id);

                    await TextChannel.AddPermissionOverwriteAsync(guildUser,
                        OverwritePermissions.InheritAll.Modify(
                            viewChannel: PermValue.Allow,
                            readMessageHistory: PermValue.Allow,
                            sendMessages: PermValue.Allow,
                            attachFiles: PermValue.Allow
                    ));
                    await VoiceChannel.AddPermissionOverwriteAsync(guildUser,
                        OverwritePermissions.InheritAll.Modify(
                            viewChannel: PermValue.Allow,
                            connect: PermValue.Allow,
                            speak: PermValue.Allow,
                            useVoiceActivation: PermValue.Allow
                    ));
                }
            } catch(Exception e)
            {
                await TextChannel.SendMessageAsync("Yetkileri yenilerken bir sorun meydana geldi.");
                return;
            }

            //await TextChannel.SendMessageAsync("Permissions synced");
        }

        public async void InitializeChannels(RestTextChannel textChannel, RestVoiceChannel voiceChannel)
        {
            TextChannel = textChannel;
            TextChannelId = textChannel.Id;
            VoiceChannel = voiceChannel;
            VoiceChannelId = voiceChannel.Id;

            SyncPermissions();

            var e = new EmbedBuilder()
            {
                Color = new Color(0, 255, 0),
                Title = $"{RoomId} lobi oluşturuldu.",
                Description = $"Oda sahibi: <@{Owner}>\nKurallara uyulmadığı taktirde cezalandırılacağınızı unutmayın. Fairy Tail iyi oyunlar diler."
            };

            await textChannel.SendMessageAsync(embed: e.Build());
        }

        public bool IsReady()
        {
            return (TextChannel != null && VoiceChannel != null);
        }
    }
}
