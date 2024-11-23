namespace Ae.Ntp.Protocol
{
    public sealed class NtpUtcTimeSource : INtpTimeSource
    {
        public DateTime Now => DateTime.UtcNow;
    }
}