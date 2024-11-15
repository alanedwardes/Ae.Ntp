using System.Net;
using System.Text;

namespace Ae.Ntp.Protocol;

public struct NtpPacket : INtpByteArrayReader, INtpByteArrayWriter
{
    public byte Flags;
    public sbyte Stratum;
    public sbyte PollInterval;
    public sbyte Precision;
    public NtpShort RootDelay;
    public NtpShort RootDispersion;
    public ReadOnlyMemory<byte> ReferenceIdentifer = new byte[4];
    public NtpTimestamp ReferenceTimestamp;
    public NtpTimestamp OriginateTimestamp;
    public NtpTimestamp ReceiveTimestamp;
    public NtpTimestamp TransmitTimestamp;

    public NtpPacket()
    {
    }

    public NtpLeapIndicator LeapIndicator
    {
        readonly get => (NtpLeapIndicator)(Flags >> 6);
        set => Flags = (byte)((Flags & ~0xc0) | ((byte)value << 6));
    }

    public byte VersionNumber
    {
        readonly get => (byte)((Flags & 0x38) >> 3);
        set => Flags = (byte)((Flags & ~0x38) | (value << 3));
    }

    public NtpMode Mode
    {
        readonly get => (NtpMode)(Flags & 0x7);
        set => Flags = (byte)((Flags & ~0x7) | ((byte)value << 0));
    }

    public double PollIntervalMarshaled
    {
        readonly get => Math.Pow(2, PollInterval);
        set => PollInterval = (sbyte)Math.Log2(value);
    }

    public double PrecisionMarshaled
    {
        readonly get => Math.Pow(2, Precision);
        set => Precision = (sbyte)Math.Log2(value);
    }

    public string ReferenceIdentiferMarshaled
    {
        get
        {
            if (Stratum == 1)
            {
                return Encoding.ASCII.GetString(ReferenceIdentifer.Span);
            }
            else if (Stratum > 1)
            {
                switch (VersionNumber)
                {
                    case 3:
                        return new IPAddress(ReferenceIdentifer.Span).ToString();
                    case 4:
                        throw new NotImplementedException();
                }
            }

            return string.Empty;
        }
    }

    public void ReadBytes(ReadOnlyMemory<byte> bytes, ref int offset)
    {
        Flags = NtpByteExtensions.ReadByte(bytes, ref offset);
        Stratum = (sbyte)NtpByteExtensions.ReadByte(bytes, ref offset);
        PollInterval = (sbyte)NtpByteExtensions.ReadByte(bytes, ref offset);
        Precision = (sbyte)NtpByteExtensions.ReadByte(bytes, ref offset);
        RootDelay = NtpByteExtensions.ReadNtpShort(bytes, ref offset);
        RootDispersion = NtpByteExtensions.ReadNtpShort(bytes, ref offset);
        ReferenceIdentifer = NtpByteExtensions.ReadBytes(bytes, 4, ref offset);
        ReferenceTimestamp = NtpByteExtensions.ReadNtpTimestamp(bytes, ref offset);
        OriginateTimestamp = NtpByteExtensions.ReadNtpTimestamp(bytes, ref offset);
        ReceiveTimestamp = NtpByteExtensions.ReadNtpTimestamp(bytes, ref offset);
        TransmitTimestamp = NtpByteExtensions.ReadNtpTimestamp(bytes, ref offset);
    }

    public void WriteBytes(Memory<byte> bytes, ref int offset)
    {
        NtpByteExtensions.ToBytes(Flags, bytes, ref offset);
        NtpByteExtensions.ToBytes(Stratum, bytes, ref offset);
        NtpByteExtensions.ToBytes(PollInterval, bytes, ref offset);
        NtpByteExtensions.ToBytes(Precision, bytes, ref offset);
        NtpByteExtensions.ToBytes(RootDelay, bytes, ref offset);
        NtpByteExtensions.ToBytes(RootDispersion, bytes, ref offset);
        NtpByteExtensions.ToBytes(ReferenceIdentifer, bytes, ref offset);
        NtpByteExtensions.ToBytes(ReferenceTimestamp, bytes, ref offset);
        NtpByteExtensions.ToBytes(OriginateTimestamp, bytes, ref offset);
        NtpByteExtensions.ToBytes(ReceiveTimestamp, bytes, ref offset);
        NtpByteExtensions.ToBytes(TransmitTimestamp, bytes, ref offset);
    }
}

public enum NtpMode : byte
{
    Reserved,
    SymmetricActive,
    SymmetricPassive,
    Client,
    Server,
    BroadcastOrMulticast,
    NtpControlMessage,
    ReservedForPrivateUse
}

public enum NtpLeapIndicator : byte
{
    NoWarning,
    LastMinute61,
    LastMinute59,
    Alarm
}

public struct NtpShort
{
    public ushort Seconds;
    public ushort Fraction;
    public TimeSpan Marshaled
    {
        readonly get => TimeSpan.FromSeconds(Seconds + (Fraction / (double)ushort.MaxValue));
        set
        {
            ushort integerPart = (ushort)(value.TotalSeconds % ushort.MaxValue);
            double fractionalPart = value.TotalSeconds - Math.Floor(value.TotalSeconds);
            ushort fractionalPartShort = (ushort)Math.Round(fractionalPart * ushort.MaxValue);
            Seconds = integerPart;
            Fraction = fractionalPartShort;
        }
    }
    public override string ToString() => Marshaled.ToString();
}

public struct NtpTimestamp
{
    private static readonly DateTime NtpEpoch = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public uint Seconds;
    public uint Fraction;

    public DateTime Marshaled
    {
        readonly get => NtpEpoch.AddSeconds(Seconds).AddMilliseconds(Fraction * (double)1000 / uint.MaxValue);
        set
        {
            TimeSpan span = value - NtpEpoch;
            double fraction = span.TotalMilliseconds % 1000 / 1000;

            Seconds = (uint)Math.Round(span.TotalSeconds);
            Fraction = (uint)Math.Round(fraction * uint.MaxValue);
        }
    }

    public override string ToString() => Marshaled.ToString();
}