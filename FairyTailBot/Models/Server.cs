using System.Collections.Generic;
using FairyTail_Bot.Attributes;
using FairyTail_Bot.Database;
using FairyTail_Bot.Modules;
using System.Linq;

namespace FairyTail_Bot.Models
{
    public class Server
    {
        [IncludeColumn(false)]
        public int ID { get; set; }
        [ColumnName("uid")]
        public ulong UID { get; set; }
        [ColumnName("channelcategory")]
        public ulong ChannelCategory { get; set; }

        // Parameterless constructor for System.Activator assembly
        public Server()
        {
        }

        public void Save()
        {
            MySQLCommands.Update(this);
        }

        public static Server Get(ulong Uid)
        {
            return MySQLCommands.Select<Server>($"SELECT * FROM servers WHERE uid={Uid}").FirstOrDefault();
        }

        public static List<Server> GetAll()
        {
            return MySQLCommands.Select<Server>("SELECT * FROM servers");
        }

        public static void New(ulong uid, ulong channelCategory)
        {
            var server = new Server()
            {
                UID = uid,
                ChannelCategory = channelCategory
            };

            MySQLCommands.Insert(server);
        }
    }
}
