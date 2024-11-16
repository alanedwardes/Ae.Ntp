namespace Ae.Ntp.Protocol;

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
