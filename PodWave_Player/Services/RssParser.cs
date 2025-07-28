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

            // Versuche das Podcast-Coverbild zu finden
            string imageUrl = null;
            var imageExtension = feed.ElementExtensions
                .FirstOrDefault(ext => ext.OuterName == "image" &&
                                       ext.OuterNamespace == "http://www.itunes.com/dtds/podcast-1.0.dtd");

            if (imageExtension != null)
            {
                var imgReader = imageExtension.GetReader();
                if (imgReader.MoveToAttribute("href"))
                    imageUrl = imgReader.Value;
            }

            var podcast = new Podcast
            {
                TitleP = feed.Title?.Text,
                DescriptionP = feed.Description?.Text,
                FeedUrl = feedUrl,
                ImageUrl = imageUrl // 🎯 Coverbild gespeichert
            };

            var episodes = new List<Episode>();

            foreach (var item in feed.Items)
            {
                var episode = new Episode
                {
                    TitleE = item.Title?.Text,
                    DescriptionE = item.Summary?.Text,
                    AudioUrl = item.Links.FirstOrDefault()?.Uri.ToString()
                };

                episodes.Add(episode);
            }

            return (podcast, episodes);
        }
    }
}
