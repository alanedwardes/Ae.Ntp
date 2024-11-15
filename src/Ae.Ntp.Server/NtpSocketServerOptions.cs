using System.Net;

namespace Ae.Ntp.Server
{
    /// <summary>
    /// Defines options common to all NTP socket servers.
    /// </summary>
    public abstract class NtpSocketServerOptions
    {
        /// <summary>
        /// The default endpoint to listen on, for example 0.0.0.0:123
        /// </summary>
        public EndPoint Endpoint { get; set; } = new IPEndPoint(IPAddress.Any, 123);
    }
}