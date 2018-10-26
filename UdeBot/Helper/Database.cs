using MySql.Data.MySqlClient;
using System.Data;

namespace UdeBot.Helper
{
    public class Database
    {

        internal static MySqlConnection GetConnection()
        {
            return new MySqlConnection(Common.cfg.dbConnectionString);
        }


        internal static MySqlDataReader runQuery(MySqlConnection m, string sqlString, params MySqlParameter[] parameters)
        {
            m.Open();
            MySqlCommand c = m.CreateCommand();
            if (parameters != null)
                c.Parameters.AddRange(parameters);
            c.CommandText = sqlString;
            c.CommandTimeout = 5;
            return c.ExecuteReader(CommandBehavior.CloseConnection);
        }

        internal static MySqlDataReader RunQuery(string sqlString, params MySqlParameter[] parameters)
        {
            return runQuery(GetConnection(), sqlString, parameters);
        }


        internal static object RunQueryOne(string sqlString, params MySqlParameter[] parameters)
        {

            using (MySqlConnection m = GetConnection())
            {
                m.Open();
                using (MySqlCommand c = m.CreateCommand())
                {
                    c.Parameters.AddRange(parameters);
                    c.CommandText = sqlString;
                    c.CommandTimeout = 5;
                    return c.ExecuteScalar();
                }
            }
        }

        internal static int RunNonQuery(string sqlString, params MySqlParameter[] parameters)
        {

            using (MySqlConnection m = GetConnection())
            {
                m.Open();
                using (MySqlCommand c = m.CreateCommand())
                {
                    c.Parameters.AddRange(parameters);
                    c.CommandText = sqlString;
                    c.CommandTimeout = 5;
                    return c.ExecuteNonQuery();
                }
            }
        }

        internal static DataSet RunDataset(string sqlString, params MySqlParameter[] parameters)
        {

            using (MySqlConnection m = GetConnection())
            {
                m.Open();
                return MySqlHelper.ExecuteDataset(m, sqlString, parameters);
            }
        }
    }
}
