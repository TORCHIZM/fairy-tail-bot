using Discord;
using Discord.Rest;
using Discord.WebSocket;
using FairyTail_Bot.Core;
using FairyTail_Bot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FairyTail_Bot.Modules
{
	public static class PUG
	{
		internal static async Task ReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if (reaction.User.Value.IsBot) return;

			if (reaction.Emote.Name == "🖕🏻" &&
				cache.Value.Content == "!bul")
			{
				var lobby = Base.Queue.Where(x => x.UserIDs.Contains(reaction.UserId)).FirstOrDefault();

				if (lobby != null)
				{
					lobby.Remove(reaction.UserId);
					SocketGuildUser guildUser = reaction.User.Value as SocketGuildUser;
					await cache.Value.AddReactionAsync(new Emoji("👍"));

					if (await lobby.VoiceChannel.GetUserAsync(guildUser.Id) != null)
						await guildUser.ModifyAsync(x => x.ChannelId = 0);
				}
				else
				{
					await channel.SendMessageAsync("Zaten bir sırada değilsiniz.");
				}
			}

			if (reaction.Emote.Name == "📬")
            {
				var invitedUser = Base.InviteList[cache.Value.Id];
				
				if (invitedUser != 0 && invitedUser == reaction.User.Value.Id)
                {
					var lobby = Base.Queue.Where(x => x.Owner == reaction.Message.Value.Author.Id).FirstOrDefault();

					if (lobby != null)
					{
						Base.InviteList.Remove(cache.Value.Id);
						lobby.Add(reaction.User.Value.Id);
					}
                }
            }
		}

		internal static async Task VoiceStateUpdated(SocketUser user, SocketVoiceState disconnectedVoiceState, SocketVoiceState joinedVoiceState)
		{
			if (Base.Queue.Any(x => x.UserIDs.Contains(user.Id)))
			{
				var lobby = Base.Queue.Where(x => x.UserIDs.Contains(user.Id)).FirstOrDefault();

				if (joinedVoiceState.VoiceChannel != null &&
					joinedVoiceState.VoiceChannel.Users.Count == lobby.Count)
				{
					await lobby.TextChannel.SendMessageAsync($"Hey, <@{lobby.Owner}> lobi hazır, oyunu başlatmak için !hazır komutunu kullanın.");
					return;
				}

				if (disconnectedVoiceState.VoiceChannel != null &&
					lobby.GameState == GameState.InGame &&
					disconnectedVoiceState.VoiceChannel.Id == lobby.VoiceChannelId &&
					disconnectedVoiceState.VoiceChannel.Users.Count == 0)
				{
					lobby.Destroy();
				}
			}
		}
	}
}
