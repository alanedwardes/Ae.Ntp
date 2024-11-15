using System.Net;

namespace Ae.Ntp.Client
{
    /// <summary>
    /// Describes options common to all socket-based clients.
    /// </summary>
    public abstract class NtpSocketClientOptions
    {
        /// <summary>
        /// The time before a NTP query is considered failed.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(2);
        /// <summary>
        /// The endpoint to connect to.
        /// </summary>
        public EndPoint Endpoint { get; set; }
    }

    /// <summary>
    /// Defines options for the <see cref="NtpUdpClient"/>.
    /// </summary>
    public sealed class NtpUdpClientOptions : NtpSocketClientOptions
    {
        /// <summary>
        /// Convert an <see cref="IPAddress"/> to a <see cref="NtpUdpClientOptions"/>.
        /// </summary>
        /// <param name="d"></param>
        public static implicit operator NtpUdpClientOptions(IPAddress d) => new NtpUdpClientOptions { Endpoint = new IPEndPoint(d, 123) };
        /// <summary>
        /// Convert an <see cref="IPEndPoint"/> to a <see cref="NtpUdpClientOptions"/>.
        /// </summary>
        /// <param name="d"></param>
        public static implicit operator NtpUdpClientOptions(IPEndPoint d) => new NtpUdpClientOptions { Endpoint = d };
    }

    /// <summary>
    /// Defines options for the <see cref="NtpTcpClient"/>.
    /// </summary>
    public sealed class NtpTcpClientOptions : NtpSocketClientOptions
    {
        /// <summary>
        /// Convert an <see cref="IPAddress"/> to a <see cref="NtpTcpClientOptions"/>.
        /// </summary>
        /// <param name="d"></param>
        public static implicit operator NtpTcpClientOptions(IPAddress d) => new NtpTcpClientOptions { Endpoint = new IPEndPoint(d, 123) };
        /// <summary>
        /// Convert an <see cref="IPEndPoint"/> to a <see cref="NtpTcpClientOptions"/>.
        /// </summary>
        /// <param name="d"></param>
        public static implicit operator NtpTcpClientOptions(IPEndPoint d) => new NtpTcpClientOptions { Endpoint = d };
    }
}