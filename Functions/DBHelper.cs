using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace gm.Functions
{
    public class DbHelper
    {
        public string ConnectionString { get; set; }

        public DbHelper(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public DataTable GetResult(string queryString, params MySqlParameter[] param)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();

                MySqlCommand command = GetParamCommand(queryString, connection, param);

                MySqlDataAdapter mysqlDataAdapter = new MySqlDataAdapter(command);
                mysqlDataAdapter.Fill(dataSet);
                dataTable = dataSet.Tables[0];
            }
            return dataTable;
        }

        public int ExecuteNonQuery(string queryString, params MySqlParameter[] param)
        {
            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();
                MySqlCommand command = GetParamCommand(queryString, connection, param);

                try
                {
                    return command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed: " + ex.Message);
                    return -1;
                }
            }
        }

        private static MySqlCommand GetParamCommand(string queryString, MySqlConnection connection, params MySqlParameter[] param)
        {
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = queryString;
            if (param != null || param.Length > 0)
            {
                foreach (var item in param)
                {
                    command.Parameters.Add(item);
                }
            }

            return command;
        }
    }
}
