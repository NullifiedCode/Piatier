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
    public class Kickass
    {
        public static List<Torrent> GetTorrents(string searchTerm)
        {
            List<Torrent> torrents = new List<Torrent>();
            var h1 = HTTPUtils.Get($"https://kickasstorrents.to/search/" + searchTerm.Replace(" ", " ") + $"/0?sortby=seeders&sort=desc");
            if (h1 != null)
            {
                if (h1.IsOK)
                {
                    var data3 = h1.ToString();
                    var lines = data3.Split('\n');
                    for (int k = 0; k < lines.Length; k++)
                    {

                        Torrent tor = new Torrent();
                        tor.source = "Kickass";

                        if (lines[k].Contains("markeredBlock torType filmType"))
                            tor.category = "TV / Movie";
                        else if (lines[k].Contains("markeredBlock torType musicType"))
                            tor.category = "Music";
                        else if (lines[k].Contains("markeredBlock torType exeType"))
                            tor.category = "Program";
                        else if (lines[k].Contains("markeredBlock torType Type"))
                            tor.category = "File";


                        if (!string.IsNullOrWhiteSpace(tor.category))
                        {
                            Main.indexCounter++;
                            tor.id = Main.indexCounter;
                            if (lines[k + 1].Contains("cellMainLink"))
                            {
                                tor.link = "https://kickasstorrents.to" + Regex.Match(lines[k + 1], "\\/[a-zA-Z0-9-_]+.html").ToString();
                                tor.name = Regex.Replace(lines[k + 2], "<a href=\"\\/[a-zA-Z0-9-_]+.html\" class=\"cellMainLink\">", "");
                                tor.name = tor.name.Replace("</a>", "");
                                tor.name = tor.name.Replace("<strong class=\"red\">", "");
                                tor.name = tor.name.Replace("</strong>", "");
                            }
                            if (lines[k + 22].Contains("nobr center"))
                            {
                                if (!string.IsNullOrEmpty(Regex.Match(lines[k + 22 + 1], "\\d+(.\\d+|)\\s(GB|MB|KB|B)").ToString()))
                                    tor.size = Regex.Match(lines[k + 22 + 1], "\\d+(.\\d+|)\\s(GB|MB|KB|B)").ToString();
                            }

                            if (lines[k + 28].Contains("green center"))
                            {
                                tor.seeders = Regex.Match(lines[k + 28 + 1], "\\d+").ToString();
                            }

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
