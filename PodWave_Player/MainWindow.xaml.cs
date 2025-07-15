using PodWave_Player.Models;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PodWave_Player;

//Dummydata: it-berufe-podcast.de/? powerpress_pinw = 5714 - podcast
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
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
        podcasts.Add(new Podcast
        {
            Title = "IT Berufe Podcast",
            Description = "Ein Podcast über IT-Berufe und Karrierechancen in der IT-Branche.",
            AudioUrl = "https://it-berufe-podcast.de/?powerpress_pinw=5714-podcast",
            //Episodes = new List<Episode>
         
        });
        PodcastList.ItemsSource = podcasts;
    }

    private void PodcastList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PodcastList.SelectedItem is Podcast selected)
        {
            MetaTitle.Text = selected.Title;
            Description.Text = selected.Description;

            if (!string.IsNullOrEmpty(selected.AudioUrl))
            {
                var source = new Uri(selected.AudioUrl);
                player.Source = source;
                player.Play();
            }
        }

    }

  

    private void Select_Folder(object sender, RoutedEventArgs e)
    {

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

    }

    private void Previous_Click(object sender, RoutedEventArgs e)
    {

    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {

    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {

    }

  private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        player.Play();
    }

   
    private void ProgressSlider_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
       
    }

    
}