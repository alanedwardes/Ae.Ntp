namespace Ae.Ntp.Protocol;

public struct NtpShort
{
    public ushort Seconds;
    public ushort Fraction;

    public static readonly double Segment = 1000d / ushort.MaxValue;

    public TimeSpan Marshaled
    {
        readonly get => TimeSpan.FromSeconds(Seconds) + TimeSpan.FromMilliseconds(Fraction * Segment);
        set
        {
            Seconds = (ushort)Math.Floor(value.TotalMilliseconds / 1000);
            Fraction = (ushort)(value.TotalMilliseconds % 1000 / Segment);
        }
    }

    public static readonly NtpTimestamp Zero = new();

    public override string ToString() => Marshaled.ToString();
}
