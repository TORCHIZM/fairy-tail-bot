using FairyTail_Bot.Attributes;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FairyTail_Bot.Database
{
    public static class MySQLCommands
    {
        public static List<T> Select<T>(string query)
        {
            using var connection = new MySqlConnection(MySQL.ConnectionString);
            connection.Open();

            using var cmd = new MySqlCommand(query, connection);
            using var dataReader = cmd.ExecuteReader();

            var list = DataReaderMapToList<T>(dataReader);

            connection.Close();

            return list;
        }

        public static List<T> DataReaderMapToList<T>(IDataReader dr)
        {
            List<T> list = new List<T>();
            T obj = default(T);
            while (dr.Read())
            {
                obj = Activator.CreateInstance<T>();

                foreach (PropertyInfo prop in obj.GetType().GetProperties())
                {
                    if (!object.Equals(dr[prop.Name], DBNull.Value))
                        prop.SetValue(obj, dr[prop.Name], null);
                }

                list.Add(obj);
            }
            return list;
        }

        public static void Insert(object obj)
        {
            string keys = "";
            string values = "";
            string table = $"{obj.GetType().Name.ToLower()}s";

            PropertyInfo[] array = obj.GetType().GetProperties().Where(x => x.GetValue(obj, null).GetType() != typeof(DateTime)).ToArray();
            
            for (int i = 0; i < array.Length; i++)
            {
                PropertyInfo prop = array[i];

                if (prop.Name != "ID")
                {
                    var columnName = AttributeController.GetAttributeValue<string, ColumnName>(prop, "Name");
                    if (columnName != null)
                    {
                        keys += columnName;
                    }
                    else
                    {
                        keys += prop.Name.ToLower();
                    }
                    values += prop.GetValue(obj, null).GetType() == typeof(int) ? $"{prop.GetValue(obj, null)}" : $"\"{prop.GetValue(obj, null)}\"";

                    if (prop.Name != array.Last().Name)
                    {
                        values += ",";
                        keys += ",";
                    }
                }
            }

            MySQL.Execute($"INSERT INTO {table} ({keys}) VALUES ({values})");
        }

        public static void Update(object obj)
        {
            string properties = "";
            string table = $"{obj.GetType().Name.ToLower()}s";
            string constant = $"ID={obj.GetType().GetProperties().Where(x => x.Name == "ID").Select(x => x.GetValue(obj, null)).FirstOrDefault()}";

            foreach (PropertyInfo prop in obj.GetType().GetProperties())
                if (prop.GetValue(obj, null).GetType() != typeof(DateTime))
                    properties += prop.GetValue(obj, null).GetType() == typeof(int) ? $"{prop.Name}={prop.GetValue(obj, null)}," : $"{prop.Name}=\"{prop.GetValue(obj, null)}\",";

            properties += "ID=ID";

            MySQL.Execute($"UPDATE {table} SET {properties} WHERE {constant}");
        }

        public static void Delete(object obj)
        {
            string table = $"{obj.GetType().Name.ToLower()}s";

            PropertyInfo[] array = obj.GetType().GetProperties().Where(x => x.GetValue(obj, null).GetType() != typeof(DateTime)).ToArray();

            string constant = $"ID={obj.GetType().GetProperties().Where(x => x.Name == "ID").Select(x => x.GetValue(obj, null)).FirstOrDefault()}";
            MySQL.Execute($"DELETE FROM {table} WHERE {constant}");
        }
    }
}
