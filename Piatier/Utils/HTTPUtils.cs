using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Piatier.Utils
{
    internal class HTTPUtils
    {
        // Mullvad Testing - If you can improve it please fork and send back :)
        private static HttpResponse GetMullvadSocks5OpenVPN(string url)
        {
            HttpResponse response = null;
            HttpRequest httpRequest = null;
            try
            {
                httpRequest = new HttpRequest();
                httpRequest.ConnectTimeout = 30000;
                httpRequest.KeepAliveTimeout = 30000;
                httpRequest.ReadWriteTimeout = 30000;
                httpRequest.ReconnectLimit = 5;
                httpRequest.UserAgentRandomize();
                httpRequest.UserAgent = Http.RandomUserAgent();

                // Mullvad OpenVPN Proxy
                httpRequest.Proxy = Socks5ProxyClient.Parse("10.8.0.1:1080");

                response = httpRequest.Get(url);
                return response;
            }
            catch
            {
                return null;
            }
        }
        private static HttpResponse GetMullvadSocks5Wireguard(string url)
        {
            HttpResponse response = null;
            HttpRequest httpRequest = null;
            try
            {
                httpRequest = new HttpRequest();
                httpRequest.ConnectTimeout = 30000;
                httpRequest.KeepAliveTimeout = 30000;
                httpRequest.ReadWriteTimeout = 30000;
                httpRequest.ReconnectLimit = 5;
                httpRequest.UserAgentRandomize();
                httpRequest.UserAgent = Http.RandomUserAgent();

                // Mullvad Wireguard Proxy
                httpRequest.Proxy = Socks5ProxyClient.Parse("10.64.0.1:1080");

                response = httpRequest.Get(url);
                return response;
            }
            catch
            {
                return null;
            }
        }
        
        // Main get stuff
        public static HttpResponse Get(string url, string proxy = "")
        {
            HttpResponse response = null;
            HttpRequest httpRequest = null;

            if (Process.GetProcessesByName("mullvad-daemon").Length > 0)
            {
                if (response == null && Process.GetProcessesByName("openvpn").Length > 0)
                    response = GetMullvadSocks5OpenVPN(url);

                if (response == null)
                    response = GetMullvadSocks5Wireguard(url);
            }

            if(response == null)
            {
                try
                {
                    httpRequest = new HttpRequest();
                    httpRequest.ConnectTimeout = 30000;
                    httpRequest.KeepAliveTimeout = 30000;
                    httpRequest.ReadWriteTimeout = 30000;
                    httpRequest.ReconnectLimit = 5;
                    httpRequest.UserAgentRandomize();
                    httpRequest.UserAgent = Http.RandomUserAgent();

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
            }
            return response;
        }
    
    
        public static string GetIP()
        {
            HttpResponse response = null;
            response = HTTPUtils.Get("https://ipv4.icanhazip.com/");
            if (response != null) if (response.IsOK) return response.ToString();
            response = HTTPUtils.Get("https://api.ipify.org/");
            if (response != null) if (response.IsOK) return response.ToString();
            response = HTTPUtils.Get("https://api-ipv4.ip.sb/ip");
            if (response != null) if (response.IsOK) return response.ToString();
            response = HTTPUtils.Get("https://ipv4.getmyip.dev/");
            if (response != null) if (response.IsOK) return response.ToString();
            response = HTTPUtils.Get("https://api.my-ip.io/ip");
            if (response != null) if (response.IsOK) return response.ToString();
            return "";
        }
    }
}
