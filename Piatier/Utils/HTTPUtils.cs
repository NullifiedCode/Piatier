using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piatier.Utils
{
    internal class HTTPUtils
    {
        public static HttpResponse Get(string url, string proxy = "")
        {
            HttpResponse response = null;
            try
            {
                HttpRequest httpRequest = new HttpRequest();
                httpRequest.ConnectTimeout = 120000; // 30 seconds
                if (!string.IsNullOrEmpty(proxy))
                    httpRequest.Proxy = HttpProxyClient.Parse(proxy);
                try
                {
                    httpRequest.UserAgentRandomize();
                    httpRequest.UserAgent = Http.RandomUserAgent();
                }
                catch
                {

                }
                response = httpRequest.Get(url);

                return response;
            }
            catch (ProxyException ex)
            {
                return null;
            }
            catch (HttpException ex)
            {
                return null;
            }
            return response;
        }
    }
}
