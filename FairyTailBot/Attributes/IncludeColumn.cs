using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FairyTail_Bot.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IncludeColumn : Attribute
    {
        public bool Include { get; set; }

        public IncludeColumn(bool include)
        {
            this.Include = include;
        }
    }
}
