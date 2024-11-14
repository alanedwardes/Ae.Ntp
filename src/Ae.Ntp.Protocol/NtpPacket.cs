using System.Collections;
using System.Threading.Tasks;

namespace Ae.Ntp.Protocol;

public struct NtpPacket
{
    public byte Flags;
    public sbyte Stratum;
    public sbyte Poll;
    public sbyte Precision;
    public uint RootDelay;
    public uint RootDispersion;
    public uint ReferenceIdentifer;
    public ulong ReferenceTimestamp;
    public ulong OriginateTimestamp;
    public ulong ReceiveTimestamp;
    public ulong TransmitTimestamp;

    public NtpLeapIndicator LeapIndicator
    {
        get => (NtpLeapIndicator)(Flags >> 6);
        set => Flags = (byte)((Flags & ~0xc0) | ((byte)value << 6));
    }
    public byte VersionNumber
    {
        get => (byte)((Flags & 0x38) >> 3);
        set => Flags = (byte)((Flags & ~0x38) | (value << 3));
    }
    public NtpMode Mode
    {
        get => (NtpMode)(Flags & 0x7);
        set => Flags = (byte)((Flags & ~0x7) | ((byte)value << 0));
    }
    public double PrecisionMarshaled
    {
        get => Math.Pow(2, Precision);
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