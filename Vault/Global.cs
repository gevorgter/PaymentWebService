using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vault
{
    public class Global
    {
        static string _sqlConnection;

        public static void Init(string sqlConnection)
        {
            _sqlConnection = sqlConnection;
        }

        public static void Init(IConfiguration configuration)
        {
            Init(configuration.GetConnectionString("connectionString"));
        }

        public static SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_sqlConnection);
            }
        }


    }
}
