using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piatier.Formatting
{
    public class YTSItem
    {
        public string status { get; set; }
        public string status_message { get; set; }
        public YTSData data { get; set; }

    }
    public class YTSData
    {
        public int movie_count { get; set; }
        public List<YTSMovies> movies { get; set; }
    }
    public class YTSMovies
    {
        public string url { get; set; }
        public string title_long { get; set; }
        public List<YTSTorrents> torrents { get; set; }
    }
    public class YTSTorrents
    {
        public string url { get; set; }
        public int seeds { get; set; }
        public string quality { get; set; }
        public string size { get; set; }
    }
}
