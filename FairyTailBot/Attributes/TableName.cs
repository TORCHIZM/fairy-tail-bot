using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FairyTail_Bot.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableName : Attribute
    {
        public string Name { get; set; }

        public TableName(string columnName)
        {
            this.Name = columnName;
        }
    }
}
