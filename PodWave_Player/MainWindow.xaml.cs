using MySqlConnector;
using PodWave_Player.Models;
using PodWave_Player.Services;// internal Service methode for the rsss thingy
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;


namespace PodWave_Player
{
    public partial class MainWindow : Window
    {
        private List<Podcast> podcasts = new();
        private readonly DispatcherTimer timer = new DispatcherTimer();

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

        private void PodcastList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PodcastList.SelectedItem is Podcast selected)
            {
                MetaTitle.Text = selected.TitleP;
                PodcastDescriptionTextBlock.Text = selected.DescriptionP;

                EpisodeList.ItemsSource = selected.Episodes;
            }
        }


        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            player.Play(); // change aus pause wenn click!! noch einfügen
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


        private void Previous_Click(object sender, RoutedEventArgs e) { }

        private void StopButton_Click(object sender, RoutedEventArgs e) { }

        private void Next_Click(object sender, RoutedEventArgs e) { }

        private void EpisodeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EpisodeList.SelectedItem is Episode selectedEpisode)
            {
                if (!string.IsNullOrEmpty(selectedEpisode.AudioUrl))
                    try{ 
                {
                    var source = new Uri(selectedEpisode.AudioUrl);
                    player.Source = source;
                    player.Play();
                            timer.Start();


                    // Optional: Zusatzinfos anzeigen
                    EpisodeTitleTextBlock.Text = selectedEpisode.TitleE;
                    EpisodeDescriptionTextBlock.Text = selectedEpisode.DescriptionE;
                }
                        }
                    catch(Exception ex){ MessageBox.Show("Fehler beim Abspielen: "+ex.Message);}
            }
        }


        private void ProgressSlider_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { }

        private async void AddRssFeed(object sender, RoutedEventArgs e) //warum async??
        {

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

        private async Task LoadPodcastsFromDatabase()
        {
            podcasts.Clear();

            using var conn= DataBaseHelper.GetConnection();
            await conn.OpenAsync();

            string podcastQuery="Select * from podcasts";
            using var podcastCmd = new MySqlCommand(podcastQuery, conn);
            using var reader= await podcastCmd.ExecuteReaderAsync();

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

            foreach (var podcast in podcasts)
            {
                podcast.Episodes=new();
                string episodeQuery= "SELECT * FROM episodes WHERE PodcastId = @PodcastId";
                using var episodeCmd = new MySqlCommand(episodeQuery, conn);
                episodeCmd.Parameters.AddWithValue("@PodcastId", podcast.PodcastId);

                using var episodeReader = await episodeCmd.ExecuteReaderAsync();
                while (await episodeReader.ReadAsync())
                {
                    var episode = new Episode()
                    {
                        EpisodeId = episodeReader.GetInt32("EpisodeId"),
                        TitleE = episodeReader["Title"]?.ToString(),
                        DescriptionE = episodeReader["Description"]?.ToString(),
                        AudioUrl = episodeReader["AudioUrl"]?.ToString(),
                        DurationInSeconds = episodeReader.GetInt32("DurationInSeconds")
                    };
                
                }
                await reader.CloseAsync();
            }
            PodcastList.ItemsSource = null;
            PodcastList.ItemsSource = podcasts;
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
{
    if (player.NaturalDuration.HasTimeSpan && Math.Abs(player.Position.TotalSeconds - ProgressSlider.Value) > 1)
    {
        player.Position = TimeSpan.FromSeconds(ProgressSlider.Value);
    }
}

        private void Timer_Tick(object sender, EventArgs e)
{
    if (player.NaturalDuration.HasTimeSpan)
    {
        ProgressSlider.Maximum = player.NaturalDuration.TimeSpan.TotalSeconds;
        ProgressSlider.Value = player.Position.TotalSeconds;
    }
}
    }
}
