using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using System.Configuration;

namespace PodWave_Player.Services
{
    class DataBaseHelper// This class provides methods to connect to the MySQL database using the connection string defined above.
    {

        public static readonly string connectionString = "Server=localhost;Database=podwave_db;User ID=root;Password=;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        //save Progress of playback in the database
        public static async Task SavePlaybackProgressAsync(int episodeId, int positionSec)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            string query = "INSERT INTO progress (EpisodeId, PositionSec) VALUES (@EpisodeId, @PositionSec) " +
                           "ON DUPLICATE KEY UPDATE PositionSec = @positionSec";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@EpisodeId", episodeId);
            cmd.Parameters.AddWithValue("@PositionSec", positionSec);
            await cmd.ExecuteNonQueryAsync();


        }

        // load playback progress from the database
        public static async Task<int?> LoadPlaybackProgressAsync(int episodeId)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "SELECT PositionSec FROM progress WHERE EpisodeId = @EpisodeId";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@EpisodeId", episodeId);

            var result = await cmd.ExecuteScalarAsync();
            return result == null ? null : Convert.ToInt32(result);
        }



    }
}
