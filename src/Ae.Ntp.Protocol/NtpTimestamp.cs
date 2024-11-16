namespace Ae.Ntp.Protocol;

public struct NtpTimestamp
{
    public static readonly DateTime NtpEpoch = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public uint Seconds;
    public uint Fraction;

    public DateTime Marshaled
    {
        readonly get => NtpEpoch.AddSeconds(Seconds).AddMilliseconds(Fraction * (double)1000 / uint.MaxValue);
        set
        {
            TimeSpan span = value - NtpEpoch;
            double fraction = span.TotalMilliseconds % 1000 / 1000;

            Seconds = (uint)Math.Floor(span.TotalSeconds);
            Fraction = (uint)Math.Round(fraction * uint.MaxValue);
        }
    }

    public static readonly NtpTimestamp Zero = new();

    public override string ToString() => Marshaled.ToString();
}