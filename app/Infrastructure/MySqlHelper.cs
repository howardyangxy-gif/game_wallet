using Dapper;
using MySql.Data.MySqlClient;
using System.Data;

namespace app.Infrastructure
{
    public static class MySqlHelper
    {
        private static readonly string _connStr = "server=localhost;port=3306;database=game_center;user=root;password=123456;";
        public static string GetConnStr() => _connStr;

        public static T? QueryFirstOrDefault<T>(string sql, object? param = null)
        {
            using var conn = new MySqlConnection(_connStr);
            return conn.QueryFirstOrDefault<T>(sql, param);
        }

        public static IEnumerable<T> Query<T>(string sql, object? param = null)
        {
            using var conn = new MySqlConnection(_connStr);
            return conn.Query<T>(sql, param);
        }

        public static int Execute(string sql, object? param = null)
        {
            using var conn = new MySqlConnection(_connStr);
            return conn.Execute(sql, param);
        }
    }
}
