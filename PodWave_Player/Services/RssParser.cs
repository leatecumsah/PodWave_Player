using PodWave_Player.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace PodWave_Player.Services
{
    public static class RssParser // Service to parse RSS feeds and extract podcast and episode information
    {
        public static async Task<(Podcast, List<Episode>)> ParseFeedAsync(string feedUrl) // Method to parse the RSS feed from the provided URL
        {
            using XmlReader reader = XmlReader.Create(feedUrl);
            SyndicationFeed feed = SyndicationFeed.Load(reader);

            if (feed == null)
                throw new Exception("Feed konnte nicht gelesen werden.");

            string imageUrl = null;

            // extract itunes:image 
            var imageElement = feed.ElementExtensions.FirstOrDefault(e => e.OuterName == "image" || e.OuterName == "itunes:image");
            if (imageElement != null)
            {
                try // Attempt to get the image URL from the feed's image element
                {
                    var image = imageElement.GetObject<XmlElement>();
                    imageUrl = image.GetAttribute("href");
                }
                catch // fallback if parsing fails
                {
                    imageUrl = null; // Set imageUrl to null if parsing fails so programm still runs
                }
            }

            var podcast = new Podcast // Create a new Podcast object with metadata from the feed
            {
                TitleP = feed.Title?.Text,
                DescriptionP = feed.Description?.Text,
                FeedUrl = feedUrl,
                ImageUrl = imageUrl 
            };

            var episodes = new List<Episode>();// Create a list to hold episodes extracted from the feed of the podcast

            foreach (var item in feed.Items) // Iterate through each item in the feed to extract episode information
            {
                var episode = new Episode
                {
                    TitleE = item.Title?.Text,
                    DescriptionE = item.Summary?.Text,
                    AudioUrl = item.Links.FirstOrDefault()?.Uri.ToString(),
                    DurationInSeconds = TryParseDuration(item)
                };
                episodes.Add(episode);
            }

            return (podcast, episodes);
        }

        private static int TryParseDuration(SyndicationItem item) // Method to parse the duration of an episode from the SyndicationItem
        {
            var durationElement = item.ElementExtensions
                .FirstOrDefault(e => e.OuterName == "duration");

            if (durationElement != null)
            {
                try
                {
                    string raw = durationElement.GetObject<string>();

                    if (TimeSpan.TryParse(raw, out var time)) // Try to parse the duration as a TimeSpan
                        return (int)time.TotalSeconds;


                    if (int.TryParse(raw, out var seconds)) //Alternative: Try to parse the duration as an integer (in seconds)
                        return seconds;
                }
                catch
                {
                    // ignore if not parsable 
                }
            }

            return 0;
        }

    }
}
