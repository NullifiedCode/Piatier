using Guna.UI2.WinForms;
using Newtonsoft.Json;
using Piatier.Formatting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piatier.Utils
{
    public class CacheUtils
    {
        public static List<Torrent> GetCache(string fileName, int minSeeders)
        {
            var torrents = new List<Torrent>();
            try
            {
                torrents = JsonConvert.DeserializeObject<List<Torrent>>(File.ReadAllText($"./piatier-cache/{fileName}.json"));
                torrents = torrents.Where(t => int.Parse(t.seeders) >= minSeeders).ToList();
                return torrents;
            } catch {
                return null;
            }
            return null;
        }
    }
}
