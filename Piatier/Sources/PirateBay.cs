using Guna.UI2.WinForms;
using Newtonsoft.Json;
using Piatier.Formatting;
using Piatier.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piatier.Sources
{
    public class PirateBay
    {
        public static List<Torrent> GetTorrents(string searchTerm)
        {
            List<Torrent> torrents = new List<Torrent>();
            var h = HTTPUtils.Get($"https://apibay.org/q.php?q=" + searchTerm.Replace(" ", "+") + "&cat=0");
            if (h != null)
            {
                if (h.IsOK)
                {
                    var data = h.ToString();

                    var o = JsonConvert.DeserializeObject<List<PirateBayItem>>(data);
                    if (o.Count() > 1)
                        foreach (var z in o)
                        {
                            if (!string.IsNullOrEmpty(z.name))
                            {
                                Main.indexCounter++;
                                var category = "";
                                switch (z.category)
                                {
                                    case "207":
                                        category = "HD Movies";
                                        break;
                                    case "201":
                                        category = "Movies";
                                        break;
                                    case "208":
                                        category = "HD TV-Show";
                                        break;
                                    case "205":
                                        category = "TV-Shows";
                                        break;
                                    case "101":
                                        category = "Music";
                                        break;
                                    case "601":
                                        category = "E-Book";
                                        break;
                                    case "504":
                                        category = "Games";
                                        break;
                                    case "401":
                                        category = "PC";
                                        break;
                                    case "404":
                                        category = "XBOX360";
                                        break;
                                    case "403":
                                        category = "PSx";
                                        break;
                                    case "203":
                                        category = "Music Video";
                                        break;
                                    case "104":
                                        category = "FLAC";
                                        break;
                                    case "506":
                                        category = "Movie Clips";
                                        break;
                                    case "602":
                                        category = "Comic";
                                        break;
                                    case "599":
                                        category = "Other";
                                        break;
                                    case "699":
                                        category = "Other";
                                        break;
                                    case "299":
                                        category = "Other";
                                        break;
                                    case "199":
                                        category = "Other";
                                        break;
                                    case "501":
                                        category = "Porn";
                                        break;
                                    case "505":
                                        category = "Porn";
                                        break;
                                    case "301":
                                        category = "Windows";
                                        break;
                                    case "302":
                                        category = "Mac/Apple";
                                        break;
                                    case "399":
                                        category = "Other OS";
                                        break;
                                    case "499":
                                        category = "Other OS";
                                        break;
                                    case "408":
                                        category = "Android";
                                        break;
                                    case "103":
                                        category = "Sound Clips";
                                        break;
                                    case "605":
                                        category = "Physibles";
                                        break;
                                    case "503":
                                        category = "Pictures";
                                        break;
                                    case "206":
                                        category = "Handheld";
                                        break;
                                    case "102":
                                        category = "Audio Books";
                                        break;
                                }
                                Torrent tor = new Torrent();
                                tor.id = Main.indexCounter;
                                tor.link = "https://thepiratebay.org/description.php?id=" + z.id;
                                tor.seeders = z.seeders;
                                tor.category = category;
                                tor.size = MiscUtils.BytesToString(long.Parse(z.size));
                                tor.name = z.name;
                                tor.uploader = z.username;
                                tor.source = "PirateBay";
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
