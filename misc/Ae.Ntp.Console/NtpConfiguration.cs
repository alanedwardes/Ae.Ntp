namespace Ae.Ntp.Console
{
    public sealed class NtpServer
    {
        public Uri Endpoint { get; set; } = new Uri("udp://0.0.0.0:123");
        public string TimeZone { get; set; } = "UTC";
        public string Source { get; set; } = "static";
        public TimeSpan Offset { get; set; } = TimeSpan.Zero;
    }

    public sealed class NtpConfiguration
    {
        public NtpServer[] Servers { get; set; }
    }
}