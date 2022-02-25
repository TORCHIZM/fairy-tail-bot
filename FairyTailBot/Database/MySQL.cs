using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace FairyTail_Bot.Database
{
    public static class MySQL
    {
        internal const string ConnectionString = @"server=localhost;userid=root;database=fairytail";

        public static void Execute(string query)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();

            using var cmd = new MySqlCommand(query, connection);
            using var dataReader = cmd.ExecuteReader();

            connection.Close();
        }
    }
}