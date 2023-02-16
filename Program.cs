using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using AngleSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Program
{
    public class Game
    {
        public string Name { get; set; }

        public int Rank { get; set; }

        public double Weight { get; set; }
    }

    private static double GetWeight(string gameUrl)
    {
        var html = new WebClient().DownloadString(gameUrl);
        var line = html.Split('\n').Where(x => x.Contains("\"boardgameweight\"")).ToArray().First();
        var json = line.Replace("GEEK.geekitemPreload =", "").Trim().TrimEnd(';');
        var jobject = JObject.Parse(json);
        var weight = Double.Parse(jobject["item"]["polls"]["boardgameweight"]["averageweight"].ToString());
        return weight;
    }

    public static async Task Main()
    {
        var top100Url = "https://boardgamegeek.com/browse/boardgame?sort=rank&sortdir=asc";

        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(top100Url);

        var nodes = document.QuerySelectorAll("#collectionitems tr a.primary");
        int i = 0;
        var games = new List<Game>();
        foreach (var node in nodes)
        {
            var name = node.TextContent.Trim();
            Console.WriteLine("Starting #" + i + "\t" + name);
            var relativeUrl = node.GetAttribute("href");
            var absoluteUrl = "https://boardgamegeek.com" + relativeUrl;
            var game = new Game { Rank = i + 1, Name = name, Weight = GetWeight(absoluteUrl) };
            games.Add(game);
            i++;
            Console.WriteLine("Sleep");
            System.Threading.Thread.Sleep(3000);
        }
        //  System.IO.File.WriteAllText("export.json", JsonConvert.SerializeObject(games));

        var lines = games
            .Select(item => $"| {item.Rank} | {item.Name} | {Math.Round(item.Weight, 2)} |  {item.Weight} |")
            .Prepend($"|---|---|---|")
            .Prepend($"| Rank | Name | Weight (rounded) | Weight | ");
        System.IO.File.WriteAllLines("export.md", lines);
    }
}
