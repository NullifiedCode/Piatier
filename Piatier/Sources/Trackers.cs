using Piatier.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Piatier.Sources
{
    public class Trackers
    {
        public static List<string> GetTrackers(string[] trackerSources)
        {
            var trackers = new List<string>();
            for (int i = 0; i < trackerSources.Length; i++)
            {
                if (!string.IsNullOrEmpty(trackerSources[i]))
                {
                    var r = HTTPUtils.Get(trackerSources[i]);
                    if (r != null)
                    {
                        if (r.IsOK)
                        {
                            var data = r.ToString();

                            MatchCollection collection = Regex.Matches(data, @"(udp|http|https):\/\/([a-zA-Z0-9-]+\.|)[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+.\d+(\/announce|)");

                            foreach (var match in collection)
                            {
                                if (!trackers.Contains(match.ToString()))
                                    trackers.Add(match.ToString());
                            }
                        }
                    }
                }
            }
            return trackers;
        }
    }
}
