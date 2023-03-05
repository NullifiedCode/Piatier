using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piatier.Formatting
{
    public class Torrent
    {
        public int id { get; set; }
        public string link { get; set; }
        public string name { get; set; }
        public string seeders { get; set; }
        public string category { get; set; }
        public string size { get; set; }
        public string source { get; set; }
    }
}
