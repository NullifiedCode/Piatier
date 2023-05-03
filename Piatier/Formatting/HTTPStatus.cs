using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piatier.Formatting
{
    internal class HTTPStatus
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public List<Torrent> Torrents { get; set; }
    }
}
