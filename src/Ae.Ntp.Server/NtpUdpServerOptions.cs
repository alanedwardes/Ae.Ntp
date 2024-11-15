namespace Ae.Ntp.Server
{
    /// <summary>
    /// Defines options for the <see cref="NtpUdpServer"/>.
    /// </summary>
    public sealed class NtpUdpServerOptions : NtpSocketServerOptions
    {
        /// <summary>
        /// The maximum datagram size before truncation.
        /// </summary>
        public uint DefaultMaximumDatagramSize { get; set; } = 512;
    }
}