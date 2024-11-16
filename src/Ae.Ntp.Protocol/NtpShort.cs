namespace Ae.Ntp.Protocol;

public struct NtpShort
{
    public ushort Seconds;
    public ushort Fraction;

    public TimeSpan Marshaled
    {
        readonly get => TimeSpan.FromSeconds(Seconds) + TimeSpan.FromMilliseconds(Fraction * (double)1000 / ushort.MaxValue);
        set
        {
            double fraction = value.TotalMilliseconds % 1000 / 1000;
            Seconds = (ushort)Math.Floor(value.TotalSeconds % ushort.MaxValue);
            Fraction = (ushort)Math.Round(fraction * ushort.MaxValue);
        }
    }

    public static readonly NtpTimestamp Zero = new();

    public override string ToString() => Marshaled.ToString();
}
