using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PodWave_Player
{
    /// <summary>
    /// Interaktionslogik für RssFeedInputWindow.xaml
    /// </summary>
    public partial class RssFeedInputWindow : Window
{
    public string FeedUrl { get; private set; }

    public RssFeedInputWindow()
    {
        InitializeComponent();
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        FeedUrl = FeedUrlBox.Text.Trim();
        if (!string.IsNullOrEmpty(FeedUrl))
        {
            DialogResult = true;
            Close();
        }
        else
        {
            MessageBox.Show("Bitte eine gültige URL eingeben.");
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

}
