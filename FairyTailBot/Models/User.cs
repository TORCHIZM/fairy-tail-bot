using System;
using System.Collections.Generic;
using System.Linq;
using FairyTail_Bot.Attributes;
using FairyTail_Bot.Database;

namespace FairyTail_Bot.Models
{
    public class User
    {
        [IncludeColumn(false)]
        public int ID { get; set; }
        [ColumnName("uid")]
        public ulong UID { get; set; }
        [ColumnName("gamesplayed")]
        public int GamesPlayed { get; set; }
        [IncludeColumn(false)]
        public DateTime created_at { get; set; }
        [IncludeColumn(false)]
        public DateTime updated_at { get; set; }

        // Parameterless constructor for System.Activator assembly
        public User()
        {
        }

        public void Save()
        {
            MySQLCommands.Update(this);
        }

        public static User Get(ulong Uid)
        {
            var user = MySQLCommands.Select<User>($"SELECT * FROM users WHERE uid={Uid}").FirstOrDefault();

            if (user == null)
                New(Uid, 0);
            else
                return user;

            return new User() {
                UID = Uid,
                GamesPlayed = 0
            };
        }

        public static List<User> GetAll()
        {
            return MySQLCommands.Select<User>("SELECT * FROM users");
        }

        public static void New(ulong uid, int gamesPlayed = 0)
        {
            var user = new User
            {
                UID = uid,
                GamesPlayed = gamesPlayed
            };

            MySQLCommands.Insert(user);
        }
    }
}
