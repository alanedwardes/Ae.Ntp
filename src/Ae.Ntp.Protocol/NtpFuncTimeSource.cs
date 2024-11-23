namespace Ae.Ntp.Protocol
{
    public sealed class NtpFuncTimeSource(Func<DateTime> generator) : INtpTimeSource
    {
        private readonly Func<DateTime> _generator = generator;
        public DateTime Now => _generator();
    }
}