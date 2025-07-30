using PodWave_Player.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace PodWave_Player.Services
{
    public static class RssParser
    {
        public static async Task<(Podcast, List<Episode>)> ParseFeedAsync(string feedUrl)
        {
            using XmlReader reader = XmlReader.Create(feedUrl);
            SyndicationFeed feed = SyndicationFeed.Load(reader);

            if (feed == null)
                throw new Exception("Feed konnte nicht gelesen werden.");

            string imageUrl = null;

            // 🎯 itunes:image lesen
            var imageElement = feed.ElementExtensions.FirstOrDefault(e => e.OuterName == "image" || e.OuterName == "itunes:image");
            if (imageElement != null)
            {
                try
                {
                    var image = imageElement.GetObject<XmlElement>();
                    imageUrl = image.GetAttribute("href");
                }
                catch
                {
                    imageUrl = null;
                }
            }

            var podcast = new Podcast
            {
                TitleP = feed.Title?.Text,
                DescriptionP = feed.Description?.Text,
                FeedUrl = feedUrl,
                ImageUrl = imageUrl // 🎯 hier wird's gesetzt
            };

            var episodes = new List<Episode>();
            foreach (var item in feed.Items)
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

        private static int TryParseDuration(SyndicationItem item)
        {
            var durationElement = item.ElementExtensions
                .FirstOrDefault(e => e.OuterName == "duration");

            if (durationElement != null)
            {
                try
                {
                    string raw = durationElement.GetObject<string>();

                    if (TimeSpan.TryParse(raw, out var time))
                        return (int)time.TotalSeconds;

                    // Alternative: Format "1234" (nur Sekunden)
                    if (int.TryParse(raw, out var seconds))
                        return seconds;
                }
                catch
                {
                    // ignorieren, wenn nicht parsbar
                }
            }

            return 0;
        }

    }
}
