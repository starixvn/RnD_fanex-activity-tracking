namespace Fanex.ActivityTracking.Web
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Web;

    public interface IPService
    {
        string GetClientIPAddress(HttpRequest request);
    }

    public class IPServiceImpl : IPService
    {
        public string GetClientIPAddress(HttpRequest request)
        {
            var forwardHeader = GetForwardHeader(request.ServerVariables["HTTP_X_FORWARDED_FOR"]);

            if (!string.IsNullOrEmpty(forwardHeader))
            {
                return forwardHeader;
            }

            return GetInternalAddress(request.UserHostAddress);
        }

        internal static string GetForwardHeader(string forwardHeader)
        {
            if (forwardHeader != null)
            {
                return forwardHeader.Split(',').First().Trim();
            }

            return string.Empty;
        }

        internal static string GetInternalAddress(string userHostAddress)
        {
            IPAddress internalIPAddress = GetInternalIPAddress(userHostAddress);

            if (internalIPAddress == null)
            {
                var hostName = Dns.GetHostName();

                internalIPAddress = GetInternalIPAddress(hostName);
            }

            return internalIPAddress != null ? internalIPAddress.ToString() : string.Empty;
        }

        internal static IPAddress GetInternalIPAddress(string hostAddress)
        {
            foreach (var itemIPA in Dns.GetHostAddresses(hostAddress))
            {
                if (itemIPA.AddressFamily.ToString().Equals("InterNetwork", StringComparison.OrdinalIgnoreCase))
                {
                    return itemIPA;
                }
            }

            return null;
        }
    }
}
