using CommonDTO;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Authorizers
{
    public class Global
    {
        static string _sqlConnection;

        public static void Init(string sqlConnection)
        {
            _sqlConnection = sqlConnection;
        }

        public static void Init(Microsoft.Extensions.Configuration.IConfiguration configuration, Serilog.ILogger logger)
        {
            Init(configuration.GetConnectionString("connectionString"));
            //register authroizer with Json parser


            //register all AuthorizerResponse and ClientAuthorizerConfig objects 
            Type authorizerResponseType = typeof(AuthorizerResponse);
            Type clientAuthorizerConfigType = typeof(Common.ClientAuthorizerConfig);
            foreach (var t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!t.IsClass || t.IsAbstract)
                    continue;

                if (authorizerResponseType.IsAssignableFrom(t))
                {
                    Helpers.JsonHelper.RegisterType(t);
                    continue;
                }
                if (clientAuthorizerConfigType.IsAssignableFrom(t))
                {
                    Helpers.JsonHelper.RegisterType(t);
                    continue;
                }
            }

            Log.Logger = logger;
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
