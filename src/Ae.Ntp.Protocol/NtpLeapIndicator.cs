namespace Ae.Ntp.Protocol;

public enum NtpLeapIndicator : byte
{
    NoWarning,
    LastMinute61,
    LastMinute59,
    Alarm
}
