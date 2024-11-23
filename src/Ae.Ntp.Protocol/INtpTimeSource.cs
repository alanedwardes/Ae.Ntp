namespace Ae.Ntp.Protocol
{
    public interface INtpTimeSource
    {
        DateTime Now { get; }
    }
}