using PodWave_Player.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;

namespace PodWave_Player.Helpers
{
    public static class DatabaseHelper
    {
        private const string ConnectionString = "Server=localhost;Database=podwave_db;Uid=root;Pwd=;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        #region InsertPodcast
        public static async Task<int> InsertPodcast(Podcast podcast)
        {
            using var conn = new MySqlConnection(ConnectionString);
            await conn.OpenAsync();

            string query = "INSERT INTO podcast (Title, DescriptionP, FeedUrl) VALUES (@title, @desc, @url); SELECT LAST_INSERT_ID();";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@title", podcast.TitleP);
            cmd.Parameters.AddWithValue("@desc", podcast.DescriptionP);
            cmd.Parameters.AddWithValue("@url", podcast.FeedUrl);

            object result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        #endregion

        #region InsertEpisode
        public static async Task InsertEpisode(Episode episode, int podcastId)
        {
            using var conn = new MySqlConnection(ConnectionString);
            await conn.OpenAsync();

            string query = "INSERT INTO episode (PodcastId, Title, DescriptionE, AudioUrl) VALUES (@pid, @title, @desc, @url)";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@pid", podcastId);
            cmd.Parameters.AddWithValue("@title", episode.TitleE);
            cmd.Parameters.AddWithValue("@desc", episode.DescriptionE);
            cmd.Parameters.AddWithValue("@url", episode.AudioUrl);

            await cmd.ExecuteNonQueryAsync();
        }
        #endregion

        #region LoadProgress
        public static async Task<int?> LoadPlaybackProgressAsync(int episodeId)
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = "SELECT PositionInSeconds FROM playbackprogress WHERE EpisodeId = @eid";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@eid", episodeId);

                var result = await cmd.ExecuteScalarAsync(); //Execute....to only call upon one number
                return result == null || result == DBNull.Value ? null : Convert.ToInt32(result);
            }
        #endregion

        #region SaveProgress
        public static async Task SavePlaybackProgressAsync(int episodeId, int positionInSeconds)
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                string query = @"INSERT INTO playbackprogress (EpisodeId, PositionInSeconds)
                                 VALUES (@eid, @pos)
                                 ON DUPLICATE KEY UPDATE PositionInSeconds = @pos";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@eid", episodeId);
                cmd.Parameters.AddWithValue("@pos", positionInSeconds);

                await cmd.ExecuteNonQueryAsync();
            }


    
        #endregion


        

        
        

   }     


}
