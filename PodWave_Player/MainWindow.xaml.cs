using MySqlConnector;
using PodWave_Player.Models;
using PodWave_Player.Services;// internal Service methode for the rsss thingy
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

// TODO: Alle Helper-Methoden in eigene Klasse (z. B. PlayerHelper) auslagern
// TODO: Play-Button-Animation 
#region helpers
// TODO: aa
#region
#endregion

#endregion

namespace PodWave_Player
{
    public partial class MainWindow : Window
    {
        #region Variables
        private List<Podcast> podcasts = new();
        private readonly DispatcherTimer timer = new DispatcherTimer();
        #endregion


        #region MainWindow
        public MainWindow()
        {
            InitializeComponent();
            //LoadDummyData();
            _ = LoadPodcastsFromDatabase(); // Async ohne await aufrufen
            VolumeSlider.Value = 0.5; // Initiale Lautstärke
            player.Volume = VolumeSlider.Value;

            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
        }
        #endregion


        #region CodeBehind
        private void PodcastList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PodcastList.SelectedItem is Podcast selected)
            {
                MetaTitle.Text = selected.TitleP;
                //PodcastDescriptionTextBlock.Text = selected.DescriptionP;

                EpisodeList.ItemsSource = selected.Episodes;
            }
        }


        private async Task LoadPodcastsFromDatabase() // Method to load podcasts from the database
        {
            podcasts.Clear();

            using var conn = DataBaseHelper.GetConnection();
            await conn.OpenAsync();

            string podcastQuery = "Select * from podcasts";
            using var podcastCmd = new MySqlCommand(podcastQuery, conn);
            using var reader = await podcastCmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var podcast = new Podcast()
                {
                    PodcastId = reader.GetInt32("PodcastId"),
                    TitleP = reader["Title"]?.ToString(),
                    DescriptionP = reader["Description"]?.ToString(),
                    AudioUrl = reader["Feedurl"]?.ToString(),
                    ImageUrl = reader["ImageUrl"]?.ToString()
                };
                podcasts.Add(podcast);
            }
            await reader.CloseAsync();

            foreach (var podcast in podcasts) // For each podcast, load its episodes from the database
            {
                podcast.Episodes = new();
                string episodeQuery = "SELECT * FROM episodes WHERE PodcastId = @PodcastId";
                using var episodeCmd = new MySqlCommand(episodeQuery, conn);
                episodeCmd.Parameters.AddWithValue("@PodcastId", podcast.PodcastId);

                using var episodeReader = await episodeCmd.ExecuteReaderAsync();
                while (await episodeReader.ReadAsync())
                {
                    var episode = new Episode() // Create a new episode object
                    {
                        EpisodeId = episodeReader.GetInt32("EpisodeId"), // Assuming EpisodeId is an int in the database
                        TitleE = episodeReader["Title"]?.ToString(), // Get the title of the episode
                        DescriptionE = episodeReader["Description"]?.ToString(), // Get the description of the episode
                        AudioUrl = episodeReader["AudioUrl"]?.ToString(),// Get the audio URL of the episode
                        DurationInSeconds = episodeReader.GetInt32("DurationInSeconds") // Get the duration of the episode in seconds
                    };

                }
                await reader.CloseAsync(); // Close the reader for episodes
            }
            PodcastList.ItemsSource = null; // Clear the current items source to refresh the list
            PodcastList.ItemsSource = podcasts; // Set the new items source to the updated podcasts list
        }

        private async void EpisodeList_SelectionChanged(object sender, SelectionChangedEventArgs e)// Play the selected episode
        {

            // TODO: Fortschritt der Episode in die Datenbank speichern (PositionSec)
            // TODO: Fortschritt beim Start automatisch aus DB laden


            if (EpisodeList.SelectedItem is Episode selectedEpisode)// Check if an episode is selected
            {
                if (!string.IsNullOrEmpty(selectedEpisode.AudioUrl))// Check if the selected episode has a valid audio URL
                    try
                    {
                        {
                            var source = new Uri(selectedEpisode.AudioUrl);// Create a new Uri from the episode's audio URL
                            player.Source = source;// Set the player's source to the selected episode's audio URL
                            EpisodeTitleTextBlock.Text = selectedEpisode.TitleE;// Set the title text block to the selected episode's title
                            

                            if (selectedEpisode.EpisodeId > 0)// Check if the episode has a valid ID
                            {
                                int? savedPosition = await DataBaseHelper.LoadPlaybackProgressAsync(selectedEpisode.EpisodeId);
                                if (savedPosition.HasValue)
                                {
                                    player.Position = TimeSpan.FromSeconds(savedPosition.Value);
                                }
                            }
                            player.Play();// Start playback of the selected episode
                            timer.Start(); // Start the timer to update the progress slider
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Fehler beim Abspielen: " + ex.Message); } // Catch any exceptions that occur during playback
            }
        }
        #endregion


        #region Buttons

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            player.Play(); // change aus pause wenn click!! noch einfügen
        }
        // TODO: Automatisch pausieren, wenn andere Episode gestartet wird
        // TODO: Lautstärke und Position aus vorheriger Sitzung laden

        private void Previous_Click(object sender, RoutedEventArgs e) { }

        private async void StopButton_Click(object sender, RoutedEventArgs e)// Stop playback and save progress
        {
            player.Stop();// Stop the player
            timer.Stop();// Stop the timer when playback stops

            if (EpisodeList.SelectedItem is Episode selectedEpisode && selectedEpisode.EpisodeId > 0)// Check if an episode is selected and has a valid ID
            {
                int currentPos = (int)player.Position.TotalSeconds;
                await DataBaseHelper.SavePlaybackProgressAsync(selectedEpisode.EpisodeId, currentPos);
            }
        }


        private void Next_Click(object sender, RoutedEventArgs e) { }

        

        private async void AddRssFeed(object sender, RoutedEventArgs e) // Button click event to add a new RSS feed
        {
            // TODO: Prüfen, ob Feed bereits vorhanden ist, bevor er hinzugefügt wird
            // TODO: Fehlerbehandlung für ungültige/fehlende RSS-URLs erweitern
            // TODO: Logging bei Feed-Fehlern einbauen


            {
                //FOR TESTINGG !!!!
                string feedUrl = "https://it-berufe-podcast.de/feed/";

                try
                {
                    Podcast podcast = await RssParser.LoadPodcastFromFeedAsync(feedUrl);
                    podcasts.Add(podcast);


                    PodcastList.ItemsSource = null;
                    PodcastList.ItemsSource = podcasts;

                    MessageBox.Show("Podcast \"" + podcast.TitleP + "\" loaded with " + podcast.Episodes.Count + " episodes.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading RSS feed:\n" + ex.Message);
                }


            }
        }

        private void BTN_Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BTN_Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        private void BTN_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion


        #region ProgressSlider

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) // Update the player's position when the slider value changes
        {
            if (player.NaturalDuration.HasTimeSpan && Math.Abs(player.Position.TotalSeconds - ProgressSlider.Value) > 1) // Check if the player has a valid duration and if the slider value is significantly different from the current position
            {
                player.Position = TimeSpan.FromSeconds(ProgressSlider.Value); // Set the player's position to the value of the progress slider
            }
        }

        private void Timer_Tick(object sender, EventArgs e) // Timer tick event to update the progress slider
        {
            if (player.NaturalDuration.HasTimeSpan) // Check if the player has a valid duration
            {
                ProgressSlider.Maximum = player.NaturalDuration.TimeSpan.TotalSeconds; // Set the maximum value of the progress slider to the total duration of the audio
                ProgressSlider.Value = player.Position.TotalSeconds; // Update the progress slider value to the current position of the player
            }
        }
        private void ProgressSlider_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { } // Handle mouse down event on the progress slider 
        #endregion


        #region VolumeSlider

        // TODO: Lautstärke-Icon anpassen (stumm vs. laut)

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (player != null)
            {
                player.Volume = VolumeSlider.Value;

                if (VolumeSlider.Value == 0)
                    VolumeIcon.Content = "🔇";
                else if (VolumeSlider.Value < 0.3)
                    VolumeIcon.Content = "🔈";
                else if (VolumeSlider.Value < 0.7)
                    VolumeIcon.Content = "🔉";
                else
                    VolumeIcon.Content = "🔊";
            }
        }
        #endregion


        #region TestData
        //private void LoadDummyData()
        //{
        //    // Dummy podcast for testing purposes
        //    podcasts.Add(new Podcast
        //    {
        //        TitleP = "IT Careers Podcast",
        //        DescriptionP = "A podcast about careers and opportunities in the IT industry.",
        //        AudioUrl = "https://it-berufe-podcast.de/?powerpress_pinw=5714-podcast"

        //    });

        //    PodcastList.ItemsSource = podcasts;
        //}
        #endregion
  
    }
}
