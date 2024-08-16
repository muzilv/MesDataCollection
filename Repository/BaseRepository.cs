using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.IO;
using System;

namespace MesDataCollection.Repository
{
    public class BaseRepository : IDisposable
    {
        public static IConfigurationRoot Configuration { get; set; }

        private MySqlConnection conn;

        public MySqlConnection GetMySqlConnection( bool open = true,
            bool convertZeroDatetime = false, bool allowZeroDatetime = false)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();


            string cs = Configuration.GetConnectionString("DefaultConnection");
            var csb = new MySqlConnectionStringBuilder(cs)
            {
                AllowZeroDateTime = allowZeroDatetime,
                ConvertZeroDateTime = convertZeroDatetime
            };
            conn = new MySqlConnection(csb.ConnectionString);
            return conn;
        }

        public void Dispose()
        {
            if (conn != null && conn.State != System.Data.ConnectionState.Closed)
            {
                conn.Close();
            }
        }


    }



}
