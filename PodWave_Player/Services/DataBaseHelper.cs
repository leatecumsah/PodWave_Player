using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using System.Configuration;

namespace PodWave_Player.Services
{
    class DataBaseHelper
    {

        public static readonly string connectionString = "Server=localhost;Database=podwave_db;User ID=root;Password=;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}
// This class provides methods to connect to the MySQL database using the connection string defined above.