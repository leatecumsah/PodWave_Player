using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using PodWave_Player.Models;

namespace PodWave_Player.Services
{
    public static class RssParser
    {
        public static async Task<Podcast> LoadPodcastFromFeedAsync(string feedUrl)
        {
            using HttpClient client = new();
            using var stream = await client.GetStreamAsync(feedUrl);
            using XmlReader reader = XmlReader.Create(stream);

            SyndicationFeed feed = SyndicationFeed.Load(reader);

            Podcast podcast = new Podcast
            {
                TitleP = feed.Title?.Text ?? "No title",
                DescriptionP = feed.Description?.Text ?? "No description",
                Episodes = new List<Episode>()
            };

            foreach (var item in feed.Items)
            {
                string audioUrl = item.Links.FirstOrDefault(l => l.RelationshipType == "enclosure")?.Uri.ToString();

                var episode = new Episode
                {
                    TitleE = item.Title?.Text ?? "No title",
                    DescriptionE = item.Summary?.Text ?? "",
                    AudioUrl = audioUrl ?? "",
                    DurationInSeconds = 0 // Optional: parse duration
                };

                podcast.Episodes.Add(episode);
            }

            return podcast;
        }
    }
}
