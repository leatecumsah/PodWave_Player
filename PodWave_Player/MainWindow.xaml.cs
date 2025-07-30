using MySqlConnector;
using PodWave_Player.Helpers;
using PodWave_Player.Models; 
using PodWave_Player.Services;// internal Service methode for the rsss thingy
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;



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
            _ = LoadPodcastsFromDatabase(); // Load podcasts asynchronously
            VolumeSlider.Value = 0.5; // initial volume
            player.Volume = VolumeSlider.Value;

            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;

            SelectLastPlayedPodcast();
        }
        #endregion


        #region CodeBehind 
        private void PodcastList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PodcastList.SelectedItem is Podcast selected)
            {
                MetaTitle.Text = selected.TitleP;
                EpisodeList.ItemsSource = null;
                EpisodeList.ItemsSource = selected.Episodes;

                CoverImage.Source = new BitmapImage(new Uri(selected.ImageUrl));// show Cover check


                Properties.Settings.Default.LastPlayedPodcastId = selected.PodcastId;
                Properties.Settings.Default.Save();
                // show Cover
                if (!string.IsNullOrEmpty(selected.ImageUrl))
                {
                    try
                    {
                        CoverImage.Source = new BitmapImage(new Uri(selected.ImageUrl));
                        Console.WriteLine("ImageUrl: " + selected.ImageUrl);
                    }
                    catch
                    {
                        CoverImage.Source = null; // if no pic, no error
                    }
                }
                else
                {
                    CoverImage.Source = null; // no pic there
                }
            }
            

        }


        private async Task LoadPodcastsFromDatabase()
        {
            podcasts.Clear();

            using var conn = DatabaseHelper.GetConnection();
            await conn.OpenAsync();

            // load all podcasts from the database
            string podcastQuery = "SELECT * FROM podcast";
            using var podcastCmd = new MySqlCommand(podcastQuery, conn);
            using var reader = await podcastCmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var podcast = new Podcast
                {
                    PodcastId = reader.GetInt32("Id"),
                    TitleP = reader["Title"]?.ToString(),
                    DescriptionP = reader["DescriptionP"]?.ToString(),
                    FeedUrl = reader["FeedUrl"]?.ToString(),
                    ImageUrl = reader["ImageUrl"]?.ToString()
                };
                podcasts.Add(podcast);
            }
            await reader.CloseAsync();

            // load episodes for each podcast
            foreach (var podcast in podcasts)
            {
                podcast.Episodes = new List<Episode>();

                string episodeQuery = "SELECT * FROM episode WHERE PodcastId = @PodcastId";
                using var episodeCmd = new MySqlCommand(episodeQuery, conn);
                episodeCmd.Parameters.AddWithValue("@PodcastId", podcast.PodcastId);

                using var episodeReader = await episodeCmd.ExecuteReaderAsync();
                while (await episodeReader.ReadAsync())
                {
                    var episode = new Episode
                    {
                        EpisodeId = episodeReader.GetInt32("Id"),
                        TitleE = episodeReader["Title"]?.ToString(),
                        DescriptionE = episodeReader["DescriptionE"]?.ToString(),
                        AudioUrl = episodeReader["AudioUrl"]?.ToString(),
                        DurationInSeconds = episodeReader["DurationInSeconds"] == DBNull.Value ? 0 : Convert.ToInt32(episodeReader["DurationInSeconds"])
                    };
                    podcast.Episodes.Add(episode);
                }
                await episodeReader.CloseAsync();

                // Sortiere die Episoden nach Id aufsteigend (älteste zuerst)
                podcast.Episodes = podcast.Episodes.OrderBy(e => e.EpisodeId).ToList();
            }

            PodcastList.ItemsSource = null;
            PodcastList.ItemsSource = podcasts;
            

            int lastId = Properties.Settings.Default.LastPlayedPodcastId;
            var lastPodcast = podcasts.FirstOrDefault(p => p.PodcastId == lastId);
            if (lastPodcast != null)
            {
                PodcastList.SelectedItem = lastPodcast;
            }
            ResumeLastPlayedEpisode();
        }



        private async void EpisodeList_SelectionChanged(object sender, SelectionChangedEventArgs e)// Play the selected episode
        {

          if (player.Source != null)
            {
                player.Pause();
            }


            if (EpisodeList.SelectedItem is Episode selectedEpisode)// Check if an episode is selected
            {
                if (!string.IsNullOrEmpty(selectedEpisode.AudioUrl))// Check if the selected episode has a valid audio URL
                    try
                    {
                        {
                            var source = new Uri(selectedEpisode.AudioUrl);// Create a new Uri from the episode's audio URL
                            player.Source = source;// Set the player's source to the selected episode's audio URL
                            EpisodeTitleTextBlock.Text = selectedEpisode.TitleE;// Set the title text block to the selected episode's title
                            MetaDuration.Text = selectedEpisode.DurationInSeconds > 0
                            ? TimeSpan.FromSeconds(selectedEpisode.DurationInSeconds).ToString(@"hh\:mm\:ss")
                            : "-";
                            EpisodeDescriptionTextBlock.Text = selectedEpisode.DescriptionE;// Set the description text block to the selected episode's description

                            if (selectedEpisode.EpisodeId > 0)// Check if the episode has a valid ID
                            {
                                int? savedPosition = await DatabaseHelper.LoadPlaybackProgressAsync(selectedEpisode.EpisodeId);
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

        private void SelectLastPlayedPodcast()
            {
                int lastId = Properties.Settings.Default.LastPlayedPodcastId;
                if (lastId <= 0 || podcasts == null) return;

                var podcast = podcasts.FirstOrDefault(p => p.PodcastId == lastId);
                if (podcast != null)
                {
                    PodcastList.SelectedItem = podcast;
                }
            }

        private void SaveLastPlayedState()
            {
                if (EpisodeList.SelectedItem is Episode currentEpisode)
                {
                    int position = (int)player.Position.TotalSeconds;

                    Properties.Settings.Default.LastPlayedEpisodeId = currentEpisode.EpisodeId;
                    Properties.Settings.Default.LastPlaybackPosition = position;
                    Properties.Settings.Default.Save();
                }
            }

        private void ResumeLastPlayedEpisode()
            {
                int lastEpisodeId = Properties.Settings.Default.LastPlayedEpisodeId;
                int lastPos = Properties.Settings.Default.LastPlaybackPosition;

                if (lastEpisodeId == 0)
                    return;

                foreach (var podcast in podcasts)
                {
                    var episode = podcast.Episodes.FirstOrDefault(e => e.EpisodeId == lastEpisodeId);
                    if (episode != null)
                    {
                        PodcastList.SelectedItem = podcast;
                        EpisodeList.SelectedItem = episode;

                        player.Source = new Uri(episode.AudioUrl);
                        player.MediaOpened += (s, e) =>
                        {
                            player.Position = TimeSpan.FromSeconds(lastPos);
                            player.Play();
                            timer.Start();
                        };
                        break;
                    }
                }
            }




        #endregion


        #region Buttons Player

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            player.Play();
            //Todo: PlayButton to pausebutton and reverse
        }



        private void Previous_Click(object sender, RoutedEventArgs e) //Button to play the previous episode
        {
                if (PodcastList.SelectedItem is Podcast selectedPodcast && 
                    EpisodeList.SelectedItem is Episode selectedEpisode)
                {
                    var episodes = selectedPodcast.Episodes;
                    int currentIndex = episodes.IndexOf(selectedEpisode);

                    if (currentIndex > 0)
                    {
                        EpisodeList.SelectedItem = episodes[currentIndex + 1];
                    }
                }
            player.Play();
        }



        private async void StopButton_Click(object sender, RoutedEventArgs e)// Stop playback and save progress
        {
            player.Stop();// Stop the player
            timer.Stop();// Stop the timer when playback stops

            if (EpisodeList.SelectedItem is Episode selectedEpisode && selectedEpisode.EpisodeId > 0)// Check if an episode is selected and has a valid ID
            {
                int currentPos = (int)player.Position.TotalSeconds;
                await DatabaseHelper.SavePlaybackProgressAsync(selectedEpisode.EpisodeId, currentPos);
            }
        }



        private void Next_Click(object sender, RoutedEventArgs e) //Button to play the next episode
         
            {
                if (PodcastList.SelectedItem is Podcast selectedPodcast && 
                    EpisodeList.SelectedItem is Episode selectedEpisode)
                {
                    var episodes = selectedPodcast.Episodes;
                    int currentIndex = episodes.IndexOf(selectedEpisode);

                    if (currentIndex < episodes.Count - 1)
                    {
                        EpisodeList.SelectedItem = episodes[currentIndex - 1];
                    }
                }
            player.Play();
        }


        #endregion

        #region Add new Podcast to DB
        private async void AddRssFeed(object sender, RoutedEventArgs e)
        {
            //new window to add a new RSS feed
            string feedUrl = Microsoft.VisualBasic.Interaction.InputBox("RSS-Feed-URL eingeben:", "Neuen Feed hinzufügen");

            if (string.IsNullOrWhiteSpace(feedUrl))
                return;

            try
            {
                // parse the RSS feed
                var result = await RssParser.ParseFeedAsync(feedUrl);
                Podcast podcast = result.Item1;
                List<Episode> episodes = result.Item2;

                // save in db
                int podcastId = await DatabaseHelper.InsertPodcast(podcast);

                foreach (var episode in episodes)
                {
                    await DatabaseHelper.InsertEpisode(episode, podcastId);
                }

                // update the ui
                await LoadPodcastsFromDatabase();

                MessageBox.Show($"Podcast '{podcast.TitleP}' mit {episodes.Count} Episoden wurde hinzugefügt.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden des Feeds:\n" + ex.Message);
            }
        }
        #endregion

        #region Buttons Window
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
            SaveLastPlayedState();

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
