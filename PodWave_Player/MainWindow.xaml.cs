using PodWave_Player.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using PodWave_Player.Services;// internal Service methode for the rsss thingy


namespace PodWave_Player
{
    public partial class MainWindow : Window
    {
        private List<Podcast> podcasts = new();

        public MainWindow()
        {
            InitializeComponent();
            LoadDummyData();
        }

        private void LoadDummyData()
        {
            // Dummy podcast for testing purposes
            podcasts.Add(new Podcast
            {
                TitleP = "IT Careers Podcast",
                DescriptionP = "A podcast about careers and opportunities in the IT industry.",
                AudioUrl = "https://it-berufe-podcast.de/?powerpress_pinw=5714-podcast"
                
            });

            PodcastList.ItemsSource = podcasts;
        }

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
            player.Play();
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

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { }

        private void Previous_Click(object sender, RoutedEventArgs e) { }

        private void StopButton_Click(object sender, RoutedEventArgs e) { }

        private void Next_Click(object sender, RoutedEventArgs e) { }

        private void EpisodeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EpisodeList.SelectedItem is Episode selectedEpisode)
            {
                if (!string.IsNullOrEmpty(selectedEpisode.AudioUrl))
                {
                    var source = new Uri(selectedEpisode.AudioUrl);
                    player.Source = source;
                    player.Play();

                    // Optional: Zusatzinfos anzeigen
                    EpisodeTitleTextBlock.Text = selectedEpisode.TitleE;
                    EpisodeDescriptionTextBlock.Text = selectedEpisode.DescriptionE;
                }
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
} 
    }
