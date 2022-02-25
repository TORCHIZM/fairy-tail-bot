using Discord;
using FairyTail_Bot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FairyTail_Bot.Core
{
    public static class Base
    {
        public static List<Lobby> Queue = new List<Lobby>();
        public static Dictionary<ulong, ulong> InviteList = new Dictionary<ulong, ulong>();

        public static void Remove(Lobby lobby)
        {
            Queue.Remove(lobby);
            UpdatePresence();

            long mem = GC.GetTotalMemory(true);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.WriteLine($"[Memory Info] Total memory: {mem}");
        }

        public static bool CanCreateRoom(ulong uid) => !Queue.Any(x => x.UserIDs.Contains(uid));

        public static string GetUniqueId()
        {
            var random = new Random();
            int unique = random.Next(1000, 10000);

            if (Queue.Count == 0)
                return unique.ToString();

            while (!Queue.Any(x => x.RoomId == unique.ToString()))
                unique = random.Next(1000, 10000);

            return unique.ToString();
        }

        public static void UpdatePresence()
        {
            Global.Client.SetGameAsync($"{Queue.Count} lobide {Queue.Sum(x => x.Count)} oyuncu", "https://twitch.tv/torchizm", ActivityType.Streaming);
        }
    }
}
