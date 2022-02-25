using Discord.WebSocket;

namespace FairyTail_Bot
{
    internal static class Global
    {
        internal static DiscordSocketClient Client { get; set; }
        internal static ulong MessageIdToTrack { get; set; }
    }
}
