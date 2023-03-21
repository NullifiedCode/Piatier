using Newtonsoft.Json;
using Piatier.Formatting;
using Piatier.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Piatier.Sources
{
    internal class YTS
    {
        public static List<Torrent> GetTorrents(string searchTerm)
        {
            List<Torrent> torrents = new List<Torrent>();

            for (int i = 0; i < 2; i++)
            {
                var response = HTTPUtils.Get($"https://yts.mx/api/v2/list_movies.json?&limit=50&page={i}&sort=seeds&query_term={searchTerm}");
                if (response != null)
                {
                    if (response.IsOK)
                    {
                        YTSItem yTSItem = JsonConvert.DeserializeObject<YTSItem>(response.ToString());
                        if (yTSItem.data.movie_count == 0) return torrents;

                        foreach (var movie in yTSItem.data.movies)
                        {
                            foreach (var torr in movie.torrents)
                            {
                                Main.indexCounter++;
                                var tor = new Torrent();
                                tor.id = Main.indexCounter;
                                tor.uploader = "YTS";
                                tor.category = "Movie";
                                tor.source = "YTS";
                                tor.name = movie.title_long + $" ({torr.quality})";
                                tor.seeders = torr.seeds.ToString();
                                tor.link = torr.url;
                                tor.size = torr.size;
                                torrents.Add(tor);
                            }
                        }
                    }
                }
            }

            return torrents;
        }
    }
}
