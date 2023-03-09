using Guna.UI2.WinForms;
using Piatier.Formatting;
using Piatier.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Piatier.Sources
{
    public class _1337x
    {
        public static List<Torrent> GetTorrents(string searchTerm)
        {
            List<Torrent> torrents = new List<Torrent>();
            var x2 = HTTPUtils.Get("https://www.1377x.to/sort-search/" + searchTerm.Replace(" ", " ") + "/seeders/desc/1/");
            if (x2 != null)
            {
                if (x2.IsOK)
                {
                    var xx = x2.ToString().Split('\n');
                    for (int i = 0; i < xx.Length; i++)
                    {
                        var line = xx[i];
                        var category = "";
                        if (line.Contains("/torrent/") && line.Contains("href"))
                        {
                            Main.indexCounter++;
                            category = Regex.Replace(xx[i - 1], "\\/sub\\/[a-zA-Z]+\\/", "").Replace("/1/", "").Replace("<td class=\"coll-1 name\"><a href=\"", "").Replace("\" class=\"icon\">", "");
                            var seeders = xx[i + 1].Replace("<td class=\"coll-2 seeds\">", "").Replace("</td>", "");
                            var size = xx[i + 4].Replace("<td class=\"coll-4 size mob-uploader\">", "").Replace("</td>", "");
                            var link = Regex.Replace(line, "<i class=\"flaticon-[a-zA-Z0-9]+\"><\\/i><\\/a><a href=\"", "");
                            link = Regex.Match(line, "\\/torrent\\/\\d+\\/[a-zA-Z0-9\\-_\\s]+\\/\"").Value.Replace("/\"", "");
                            //link = Regex.Replace(link, "\\/\\\">[a-zA-Z0-9-_\\\\-\\\\s.\\s+]+", "").Replace("</a></td>", "");


                            line = Regex.Replace(line, "<a href=\"\\/torrent\\/[0-9]+\\/[a-zA-Z0-9-_]+\\/\">", "").Replace("</a>", "");
                            line = Regex.Replace(line, "<i class=\\\"flaticon-[a-zA-Z0-9-]+\"><\\/i>", "").Replace("</td>", "");


                            Torrent tor = new Torrent();
                            tor.id = Main.indexCounter;
                            tor.link = "https://www.1377x.to" + link;
                            tor.seeders = seeders;
                            tor.category = category;
                            tor.size = size;
                            tor.name = line;
                            tor.source = "1337x";
                            if (!torrents.Contains(tor))
                                torrents.Add(tor);
                        }
                    }
                }
            }
            return torrents;
        }
    }
}
