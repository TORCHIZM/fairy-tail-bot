using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FairyTail_Bot.Attributes
{
    public static class AttributeController
    {
        public static bool HasAttribute<T>(PropertyInfo property)
            where T : Attribute
        {
            var attributes = property.GetCustomAttributes(true);
            var result = attributes.Where(x => x is T).FirstOrDefault() != null;
            return result;
        }

        public static Tout GetAttributeValue<Tout, T>(PropertyInfo property, string key = "Name")
            where T : Attribute
        {
            var attributes = property.GetCustomAttributes(true);
            var attribute = attributes.Where(x => x is T).FirstOrDefault();
            if (attribute is null) return default;
            var prop = attribute.GetType().GetProperties().Where(x => x.Name == key).FirstOrDefault();
            return (Tout)prop.GetValue(attribute) ?? default;
        }
    }
}