using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FairyTail_Bot.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnName : Attribute
    {
        public string Name { get; set; }

        public ColumnName(string columnName)
        {
            this.Name = columnName;
        }
    }
}
